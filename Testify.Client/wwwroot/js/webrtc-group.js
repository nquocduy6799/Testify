// WebRTC Group Call - Mesh topology
// Each participant maintains one RTCPeerConnection per remote peer.
window.webRtcGroup = {
    _localStream: null,
    _dotNetRef: null,
    _peers: {},          // { peerId: { pc, remoteStream, iceCandidateBuffer } }
    _prePeerIceBuffer: {}, // ICE candidates that arrived before peer connection was created
    _videoContainer: null,
    _isCameraOn: true,
    _isMicOn: true,
    _mediaRecorder: null,
    _audioChunks: [],

    /**
     * Acquire local media and render the local preview.
     * @param {object} dotNetRef - .NET interop reference
     * @param {boolean} withVideo - whether to capture video
     * @returns {boolean}
     */
    init: async function (dotNetRef, withVideo) {
        this._dotNetRef = dotNetRef;
        this._peers = {};
        this._prePeerIceBuffer = {};
        // Store userId for glare resolution (backup if initChat hasn't been called)
        this._localUserId = this._chatCurrentUserId || '';

        try {
            // Verify existing stream has live audio tracks; discard if dead
            if (this._localStream) {
                const audioTracks = this._localStream.getAudioTracks();
                const hasLiveAudio = audioTracks.length > 0 && audioTracks.some(t => t.readyState === 'live');
                if (!hasLiveAudio) {
                    console.warn('[WebRTCGroup] Existing stream is dead — requesting new stream');
                    this._localStream.getTracks().forEach(t => t.stop());
                    this._localStream = null;
                }
            }

            // Acquire media with fallback: try audio+video first, then audio-only
            if (!this._localStream) {
                try {
                    this._localStream = await navigator.mediaDevices.getUserMedia({
                        audio: true,
                        video: { width: { ideal: 640 }, height: { ideal: 480 }, facingMode: 'user' }
                    });
                } catch (videoErr) {
                    console.warn('[WebRTCGroup] Camera not available, falling back to audio-only:', videoErr.message);
                    this._localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
                }
            }

            // Sync track enabled state with the requested camera/mic state
            const hasVideo = this._localStream.getVideoTracks().length > 0;
            this._localStream.getVideoTracks().forEach(t => t.enabled = withVideo);
            this._localStream.getAudioTracks().forEach(t => t.enabled = true);
            this._isCameraOn = hasVideo && withVideo;
            this._isMicOn = true;

            // Attach to the in-meeting local video element
            const localVideo = document.getElementById('local-video');
            if (localVideo) {
                localVideo.srcObject = this._localStream;
                try { await localVideo.play(); } catch (e) { console.log('[WebRTCGroup] local-video play():', e.message); }
            }

            // Retry attachment after a short delay in case Blazor re-rendered the element
            setTimeout(() => {
                const lv = document.getElementById('local-video');
                if (lv && this._localStream) {
                    if (!lv.srcObject || lv.srcObject !== this._localStream) {
                        lv.srcObject = this._localStream;
                    }
                    lv.play().catch(() => {});
                }
            }, 500);

            return true;
        } catch (err) {
            console.error('[WebRTCGroup] getUserMedia failed:', err);
            dotNetRef.invokeMethodAsync('OnWebRTCError', 'Failed to access camera/microphone: ' + err.message);
            return false;
        }
    },

    /**
     * Create RTCPeerConnection for a specific peer, create an offer, return the SDP.
     * The caller (existing participant) calls this when a new peer joins.
     * @param {string} peerId
     * @returns {string|null} JSON stringified SDP offer
     */
    createOffer: async function (peerId) {
        // If a peer connection already exists and isn't dead, skip — avoid disrupting
        // an in-progress or established connection with a redundant offer.
        const existing = this._peers[peerId];
        if (existing) {
            const conn = existing.pc.connectionState;
            if (conn !== 'failed' && conn !== 'closed') {
                console.log(`[WebRTCGroup] Skipping offer for ${peerId} — peer already exists (conn: ${conn}, sig: ${existing.pc.signalingState})`);
                return null;
            }
            // Clean up dead connection before recreating
            existing.pc.close();
            delete this._peers[peerId];
        }

        const pc = this._createPeerConnection(peerId);
        try {
            const offer = await pc.createOffer();
            await pc.setLocalDescription(offer);
            return JSON.stringify(offer);
        } catch (err) {
            console.error('[WebRTCGroup] createOffer failed for', peerId, err);
            return null;
        }
    },

    /**
     * Handle an incoming SDP offer from a remote peer, create an answer.
     * @param {string} peerId
     * @param {string} sdpOfferJson
     * @returns {string|null} JSON stringified SDP answer
     */
    handleOffer: async function (peerId, sdpOfferJson) {
        const pc = this._createPeerConnection(peerId);
        try {
            const offer = JSON.parse(sdpOfferJson);

            // Handle glare: both sides sent offers simultaneously
            if (pc.signalingState === 'have-local-offer') {
                // "Polite peer" pattern: lower userId rolls back, higher userId wins
                const myId = this._localUserId || this._chatCurrentUserId || '';
                const isPolite = myId < peerId;
                if (!isPolite) {
                    console.log(`[WebRTCGroup] Glare with ${peerId}: ignoring their offer (we have priority)`);
                    return null;
                }
                console.log(`[WebRTCGroup] Glare with ${peerId}: rolling back our offer (they have priority)`);
                await pc.setLocalDescription({ type: 'rollback' });
            }

            await pc.setRemoteDescription(new RTCSessionDescription(offer));
            this._flushIceCandidates(peerId);

            const answer = await pc.createAnswer();
            await pc.setLocalDescription(answer);
            return JSON.stringify(answer);
        } catch (err) {
            console.error('[WebRTCGroup] handleOffer failed for', peerId, err);
            return null;
        }
    },

    /**
     * Handle an incoming SDP answer from a remote peer.
     * @param {string} peerId
     * @param {string} sdpAnswerJson
     */
    handleAnswer: async function (peerId, sdpAnswerJson) {
        const peer = this._peers[peerId];
        if (!peer) return;
        try {
            const answer = JSON.parse(sdpAnswerJson);
            await peer.pc.setRemoteDescription(new RTCSessionDescription(answer));
            this._flushIceCandidates(peerId);
        } catch (err) {
            console.error('[WebRTCGroup] handleAnswer failed for', peerId, err);
        }
    },

    /**
     * Add an ICE candidate received from a remote peer.
     */
    addIceCandidate: async function (peerId, candidate, sdpMid, sdpMLineIndex) {
        const peer = this._peers[peerId];
        const iceCandidate = { candidate, sdpMid, sdpMLineIndex };

        if (!peer) {
            // Peer connection not yet created — buffer until _createPeerConnection is called
            if (!this._prePeerIceBuffer[peerId]) this._prePeerIceBuffer[peerId] = [];
            this._prePeerIceBuffer[peerId].push(iceCandidate);
            console.log(`[WebRTCGroup] Buffered pre-peer ICE for ${peerId} (total: ${this._prePeerIceBuffer[peerId].length})`);
            return;
        }

        if (!peer.pc.remoteDescription) {
            peer.iceCandidateBuffer.push(iceCandidate);
            return;
        }

        try {
            await peer.pc.addIceCandidate(new RTCIceCandidate(iceCandidate));
        } catch (err) {
            console.error('[WebRTCGroup] addIceCandidate failed for', peerId, err);
        }
    },

    /**
     * Re-attach all known remote streams to their video elements.
     * Call this after Blazor re-renders to restore srcObject on recreated video elements.
     */
    reattachStreams: function () {
        for (const peerId of Object.keys(this._peers)) {
            const peer = this._peers[peerId];
            if (peer.remoteStream) {
                const videoEl = document.getElementById('peer-video-' + peerId);
                if (videoEl && videoEl.srcObject !== peer.remoteStream) {
                    videoEl.srcObject = peer.remoteStream;
                    videoEl.play().catch(() => {});
                    console.log(`[WebRTCGroup] Reattached stream for peer ${peerId}`);
                }
            }
        }
        // Also reattach local stream
        if (this._localStream) {
            const localVideo = document.getElementById('local-video');
            if (localVideo) {
                if (localVideo.srcObject !== this._localStream) {
                    localVideo.srcObject = this._localStream;
                }
                localVideo.play().catch(() => {});
            }
        }
    },

    /**
     * Toggle local camera on/off.
     */
    toggleCamera: function (enabled) {
        this._isCameraOn = enabled;
        if (this._localStream) {
            this._localStream.getVideoTracks().forEach(t => t.enabled = enabled);
        }
    },

    /**
     * Toggle local mic on/off.
     */
    toggleMic: function (enabled) {
        this._isMicOn = enabled;
        if (this._localStream) {
            this._localStream.getAudioTracks().forEach(t => t.enabled = enabled);
        }
    },

    /**
     * Remove a specific peer connection (when they leave).
     */
    removePeer: function (peerId) {
        const peer = this._peers[peerId];
        if (!peer) return;

        peer.pc.close();
        delete this._peers[peerId];

        // Remove the video element
        const el = document.getElementById('peer-video-' + peerId);
        if (el) el.remove();
    },

    /**
     * Cleanup everything — call when leaving the meeting.
     */
    dispose: function () {
        // Close all peer connections
        for (const peerId of Object.keys(this._peers)) {
            this.removePeer(peerId);
        }
        this._prePeerIceBuffer = {};

        // Stop local media tracks
        if (this._localStream) {
            this._localStream.getTracks().forEach(t => t.stop());
            this._localStream = null;
        }

        const localVideo = document.getElementById('local-video');
        if (localVideo) localVideo.srcObject = null;

        this._dotNetRef = null;
        this._peers = {};
    },

    // ======================== INTERNAL HELPERS ========================

    _createPeerConnection: function (peerId) {
        // If already exists, reuse
        if (this._peers[peerId]) {
            return this._peers[peerId].pc;
        }

        const config = {
            iceServers: [
                { urls: 'stun:stun.l.google.com:19302' },
                { urls: 'stun:stun1.l.google.com:19302' }
            ]
        };

        const pc = new RTCPeerConnection(config);
        // Move any pre-peer buffered ICE candidates into the peer's buffer
        const preBuf = this._prePeerIceBuffer[peerId] || [];
        delete this._prePeerIceBuffer[peerId];
        const peerEntry = { pc, remoteStream: null, iceCandidateBuffer: preBuf };
        if (preBuf.length > 0) console.log(`[WebRTCGroup] Moved ${preBuf.length} pre-peer ICE candidates for ${peerId}`);
        this._peers[peerId] = peerEntry;

        // Add local tracks to this connection
        if (this._localStream) {
            this._localStream.getTracks().forEach(track => {
                pc.addTrack(track, this._localStream);
            });
        }

        // ICE candidate → relay via SignalR
        pc.onicecandidate = (event) => {
            if (event.candidate && this._dotNetRef) {
                this._dotNetRef.invokeMethodAsync(
                    'OnIceCandidateForPeer',
                    peerId,
                    event.candidate.candidate,
                    event.candidate.sdpMid,
                    event.candidate.sdpMLineIndex
                );
            }
        };

        // Remote track → render video
        pc.ontrack = (event) => {
            if (event.streams && event.streams[0]) {
                peerEntry.remoteStream = event.streams[0];
                this._attachRemoteStream(peerId, event.streams[0]);
                // Notify .NET to trigger re-render so reattachStreams runs
                if (this._dotNetRef) {
                    this._dotNetRef.invokeMethodAsync('OnPeerTrackReceived', peerId);
                }
            }
        };

        // Connection state monitoring
        pc.onconnectionstatechange = () => {
            const state = pc.connectionState;
            console.log(`[WebRTCGroup] Peer ${peerId} connection state: ${state}`);
            if (state === 'failed' || state === 'disconnected' || state === 'closed') {
                if (this._dotNetRef) {
                    this._dotNetRef.invokeMethodAsync('OnPeerConnectionStateChanged', peerId, state);
                }
            }
        };

        pc.oniceconnectionstatechange = () => {
            console.log(`[WebRTCGroup] Peer ${peerId} ICE state: ${pc.iceConnectionState}`);
        };

        return pc;
    },

    _flushIceCandidates: function (peerId) {
        const peer = this._peers[peerId];
        if (!peer || peer.iceCandidateBuffer.length === 0) return;

        console.log(`[WebRTCGroup] Flushing ${peer.iceCandidateBuffer.length} ICE candidates for ${peerId}`);
        for (const c of peer.iceCandidateBuffer) {
            peer.pc.addIceCandidate(new RTCIceCandidate(c)).catch(() => {});
        }
        peer.iceCandidateBuffer = [];
    },

    _attachRemoteStream: function (peerId, stream) {
        let videoEl = document.getElementById('peer-video-' + peerId);
        if (videoEl) {
            videoEl.srcObject = stream;
            videoEl.play().catch(() => {});
            console.log(`[WebRTCGroup] Attached stream for peer ${peerId}`);
        } else {
            console.log(`[WebRTCGroup] Video element not found for peer ${peerId}, will reattach after render`);
        }
    },

    /**
     * Check how many active peers we have.
     */
    getPeerCount: function () {
        return Object.keys(this._peers).length;
    },

    /**
     * Start local camera preview (waiting room / pre-meeting).
     * @param {string} videoElementId - id of <video> element to attach stream
     */
    startLocalPreview: async function (videoElementId) {
        try {
            // Reuse existing stream if available
            if (!this._localStream) {
                try {
                    this._localStream = await navigator.mediaDevices.getUserMedia({
                        audio: true,
                        video: { width: { ideal: 1280 }, height: { ideal: 720 }, facingMode: 'user' }
                    });
                } catch (videoErr) {
                    console.warn('[WebRTCGroup] Camera not available for preview, falling back to audio-only:', videoErr.message);
                    this._localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
                }
            }
            // Ensure video tracks are enabled (may have been disabled by toggleCamera)
            this._localStream.getVideoTracks().forEach(t => t.enabled = true);
            const el = document.getElementById(videoElementId);
            if (el) {
                el.srcObject = this._localStream;
            }
            return this._localStream.getVideoTracks().length > 0;
        } catch (err) {
            console.error('[WebRTCGroup] startLocalPreview failed:', err);
            return false;
        }
    },

    /**
     * Stop local camera preview without disposing peer connections.
     */
    stopLocalPreview: function () {
        if (this._localStream) {
            this._localStream.getTracks().forEach(t => t.stop());
            this._localStream = null;
        }
        const el = document.getElementById('local-preview');
        if (el) el.srcObject = null;
    },

    /**
     * Scroll a chat/transcript container to bottom.
     */
    scrollToBottom: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) el.scrollTop = el.scrollHeight;
    },

    // ===================== Chat Module (fully JS-driven) =====================
    _chatDotNetRef: null,
    _chatInitialized: false,
    _chatMessages: [],       // { userName, content, time, isMe, initials }
    _chatPanelOpen: false,
    _chatUnread: 0,
    _chatCurrentUserId: '',

    /**
     * Initialize the chat module.
     * @param {object} dotNetRef - .NET ref with OnJsSendChat(string content)
     * @param {string} currentUserId - the current user's ID
     */
    initChat: function (dotNetRef, currentUserId) {
        this._chatDotNetRef = dotNetRef;
        this._chatCurrentUserId = currentUserId || '';
        this._localUserId = currentUserId || '';
        this._chatMessages = [];
        this._chatUnread = 0;
        this._chatPanelOpen = false;

        if (this._chatInitialized) return;
        this._chatInitialized = true;

        // Global Enter key listener for chat input
        document.addEventListener('keydown', (e) => {
            if (e.target && e.target.id === 'chat-input' && e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this._sendChat();
            }
        });
    },

    /**
     * Called when chat panel opens — renders all buffered messages.
     */
    openChatPanel: function () {
        this._chatPanelOpen = true;
        this._chatUnread = 0;
        this._updateBadge();

        // Wait a tick for DOM to be ready, then render all messages
        setTimeout(() => {
            this._renderAllMessages();
            const textarea = document.getElementById('chat-input');
            if (textarea) textarea.focus();
        }, 50);
    },

    /**
     * Called when chat panel closes.
     */
    closeChatPanel: function () {
        this._chatPanelOpen = false;
    },

    /**
     * Add a message (from local send or from SignalR receive).
     * @param {string} userId
     * @param {string} userName
     * @param {string} content
     * @param {string} time
     * @param {string} initials
     */
    addMessage: function (userId, userName, content, time, initials) {
        const isMe = (userId === this._chatCurrentUserId);
        const msg = { userId, userName, content, time, isMe, initials };
        this._chatMessages.push(msg);

        if (this._chatPanelOpen) {
            // Panel is open — append directly to DOM
            this._removeEmptyState();
            this._appendMessageDom(msg);
        } else {
            // Panel closed — buffer it, increment unread
            this._chatUnread++;
            this._updateBadge();
        }
    },

    /**
     * Internal: read textarea, call .NET to send via SignalR.
     */
    _sendChat: function () {
        const textarea = document.getElementById('chat-input');
        if (!textarea) return;
        const content = textarea.value.trim();
        if (!content || !this._chatDotNetRef) return;

        textarea.value = '';
        textarea.focus();

        this._chatDotNetRef.invokeMethodAsync('OnJsSendChat', content);
    },

    /**
     * Render all buffered messages into the chat container (used when panel opens).
     */
    _renderAllMessages: function () {
        const container = document.getElementById('chat-messages');
        if (!container) return;

        // Clear existing content
        container.innerHTML = '';

        if (this._chatMessages.length === 0) {
            container.innerHTML = '<div class="flex flex-col items-center justify-center h-full text-center gap-2 py-12" id="chat-empty-state"><span class="text-3xl">💬</span><p class="text-xs font-medium text-slate-500">Chưa có tin nhắn<br/>Hãy gửi tin nhắn đầu tiên!</p></div>';
            return;
        }

        for (const msg of this._chatMessages) {
            this._appendMessageDom(msg);
        }
    },

    /**
     * Append a single message bubble to the DOM.
     */
    _appendMessageDom: function (msg) {
        const container = document.getElementById('chat-messages');
        if (!container) return;

        const wrapper = document.createElement('div');
        wrapper.className = 'flex ' + (msg.isMe ? 'flex-row-reverse' : 'flex-row') + ' gap-2 items-end';

        let html = '';
        if (!msg.isMe) {
            html += '<div class="h-7 w-7 rounded-lg bg-slate-700 flex items-center justify-center text-[10px] font-black text-slate-300 shrink-0">' + this._escapeHtml(msg.initials) + '</div>';
        }
        html += '<div class="max-w-[200px] flex flex-col gap-1 ' + (msg.isMe ? 'items-end' : 'items-start') + '">';
        if (!msg.isMe) {
            html += '<span class="text-[10px] font-bold text-slate-400 px-1">' + this._escapeHtml(msg.userName) + '</span>';
        }
        html += '<div class="rounded-2xl px-3 py-2 text-xs leading-relaxed ' + (msg.isMe ? 'bg-[#E85D3F] text-white rounded-br-sm' : 'bg-white/[.07] text-slate-200 rounded-bl-sm') + '">' + this._escapeHtml(msg.content) + '</div>';
        html += '<span class="text-[9px] text-slate-600 px-1">' + this._escapeHtml(msg.time) + '</span>';
        html += '</div>';

        wrapper.innerHTML = html;
        container.appendChild(wrapper);
        container.scrollTop = container.scrollHeight;
    },

    _removeEmptyState: function () {
        const empty = document.getElementById('chat-empty-state');
        if (empty) empty.remove();
    },

    _updateBadge: function () {
        const badge = document.getElementById('chat-unread-badge');
        if (!badge) return;
        if (this._chatUnread > 0) {
            badge.textContent = this._chatUnread;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    },

    _escapeHtml: function (str) {
        const div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    },

    // ===================== Live Speech Recognition (Captions) =====================
    _recognition: null,
    _captionDotNetRef: null,
    _isSpeechActive: false,

    /**
     * Initialize and start Web Speech API for live captions.
     * Results are sent back to .NET via OnLiveCaptionResult callback.
     * @param {object} dotNetRef - .NET interop reference with OnLiveCaptionResult(text, isFinal)
     */
    startSpeechRecognition: function (dotNetRef) {
        if (this._recognition) {
            // Already running — just update the ref
            this._captionDotNetRef = dotNetRef;
            return true;
        }

        var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            console.warn('[Speech] SpeechRecognition API not supported in this browser');
            return false;
        }

        this._captionDotNetRef = dotNetRef;

        try {
            var recognition = new SpeechRecognition();
            recognition.continuous = true;
            recognition.interimResults = true;
            recognition.lang = 'vi-VN';
            recognition.maxAlternatives = 1;

            var self = this;

            recognition.onresult = function (event) {
                if (!self._captionDotNetRef) return;

                var current = event.resultIndex;
                var transcript = event.results[current][0].transcript;
                var isFinal = event.results[current].isFinal;

                if (transcript && transcript.trim().length > 0) {
                    self._captionDotNetRef.invokeMethodAsync('OnLiveCaptionResult', transcript.trim(), isFinal);
                }
            };

            recognition.onend = function () {
                // Auto-restart if still supposed to be active (browser may stop after silence)
                if (self._isSpeechActive && self._recognition) {
                    console.log('[Speech] Auto-restarting after onend');
                    try { self._recognition.start(); } catch (e) {
                        // Small delay before retry in case of rapid stop/start
                        setTimeout(function () {
                            try { if (self._isSpeechActive && self._recognition) self._recognition.start(); } catch (e2) { }
                        }, 300);
                    }
                }
            };

            recognition.onerror = function (event) {
                console.warn('[Speech] Error:', event.error);
                // 'no-speech' and 'aborted' are recoverable — onend will auto-restart
                if (event.error === 'not-allowed' || event.error === 'service-not-available') {
                    self._isSpeechActive = false;
                }
            };

            this._recognition = recognition;
            this._isSpeechActive = true;
            recognition.start();
            console.log('[Speech] Live caption started');
            return true;
        } catch (err) {
            console.error('[Speech] Failed to start:', err);
            return false;
        }
    },

    /**
     * Stop live speech recognition.
     */
    stopSpeechRecognition: function () {
        this._isSpeechActive = false;
        if (this._recognition) {
            try { this._recognition.stop(); } catch (e) { }
            this._recognition = null;
        }
        this._captionDotNetRef = null;
        console.log('[Speech] Live caption stopped');
    },

    // ===================== Whisper Transcription Module =====================

    /**
     * Start recording local mic audio for post-meeting Whisper transcription.
     * Must be called after init() so that _localStream is available.
     */
    startAudioRecording: function () {
        if (!this._localStream || this._mediaRecorder) return;
        try {
            const audioStream = new MediaStream(this._localStream.getAudioTracks());
            const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
                ? 'audio/webm;codecs=opus' : 'audio/webm';
            this._mediaRecorder = new MediaRecorder(audioStream, { mimeType });
            this._audioChunks = [];
            this._mediaRecorder.ondataavailable = (e) => {
                if (e.data && e.data.size > 0) this._audioChunks.push(e.data);
            };
            this._mediaRecorder.start(1000);
            console.log('[Whisper] Audio recording started');
        } catch (err) {
            console.warn('[Whisper] MediaRecorder start failed:', err);
        }
    },

    /**
     * Stop the MediaRecorder. Call this before dispose() to capture the last chunks.
     */
    stopAudioRecording: function () {
        if (this._mediaRecorder && this._mediaRecorder.state !== 'inactive') {
            this._mediaRecorder.stop();
            console.log('[Whisper] Audio recording stopped');
        }
    },

    /**
     * Transcribe recorded audio using Whisper (Xenova/whisper-tiny) in the background.
     * Progress and result are reported back via .NET JSInvokable callbacks.
     * @param {object} dotNetRef - .NET reference with OnTranscription* callbacks
     * @param {number} meetingId - passed back to OnTranscriptionComplete
     */
    transcribeInBackground: async function (dotNetRef, meetingId) {
        // Wait for MediaRecorder to finish flushing its final chunk
        await new Promise(r => setTimeout(r, 600));

        const chunks = [...this._audioChunks];
        this._audioChunks = [];
        this._mediaRecorder = null;

        if (chunks.length === 0) {
            console.warn('[Whisper] No audio chunks recorded');
            dotNetRef.invokeMethodAsync('OnTranscriptionFailed', 'Không có dữ liệu âm thanh được ghi lại.');
            return;
        }

        try {
            dotNetRef.invokeMethodAsync('OnTranscriptionStarted');
            dotNetRef.invokeMethodAsync('OnTranscriptionProgress', 5);

            const { pipeline } = await import('https://cdn.jsdelivr.net/npm/@xenova/transformers@2.17.2');

            dotNetRef.invokeMethodAsync('OnTranscriptionProgress', 15);

            const transcriber = await pipeline(
                'automatic-speech-recognition',
                'Xenova/whisper-tiny',
                { language: 'vietnamese', task: 'transcribe', chunk_length_s: 30, stride_length_s: 5 }
            );

            dotNetRef.invokeMethodAsync('OnTranscriptionProgress', 50);

            const blob = new Blob(chunks, { type: chunks[0].type || 'audio/webm' });
            const arrayBuffer = await blob.arrayBuffer();
            const audioCtx = new AudioContext({ sampleRate: 16000 });
            const decoded = await audioCtx.decodeAudioData(arrayBuffer);
            const float32 = decoded.getChannelData(0);
            audioCtx.close();

            dotNetRef.invokeMethodAsync('OnTranscriptionProgress', 70);

            const result = await transcriber(float32);

            // Bỏ qua nếu transcript quá ngắn (im lặng / noise)
            const text = (result.text || '').trim();
            if (text.length < 5) {
                dotNetRef.invokeMethodAsync('OnTranscriptionFailed', 'Không phát hiện giọng nói trong cuộc họp.');
                return;
            }

            dotNetRef.invokeMethodAsync('OnTranscriptionComplete', meetingId, text);
        } catch (err) {
            console.error('[Whisper] Transcription error:', err);
            dotNetRef.invokeMethodAsync('OnTranscriptionFailed', err.message || 'Lỗi không xác định.');
        }
    }
};
