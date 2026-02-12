// WebRTC JS Interop for Blazor
// This is the minimal JavaScript required because WebRTC APIs
// (RTCPeerConnection, getUserMedia, MediaStream) are browser-only APIs
// with no C#/WASM equivalent.

window.WebRtcInterop = {
    _peerConnection: null,
    _localStream: null,
    _dotNetRef: null,
    _remoteDescriptionSet: false,
    _pendingCandidates: [],
    _ringtoneContext: null,
    _ringtoneOscillator: null,
    _ringtoneInterval: null,

    /**
     * Initialize a new WebRTC peer connection
     * @param {object} dotNetRef - .NET object reference for callbacks
     * @param {boolean} isVideo - Whether to include video track
     * @param {Array|null} iceServers - ICE server config from appsettings (optional)
     */
    initialize: async function (dotNetRef, isVideo, iceServers) {
        this._dotNetRef = dotNetRef;
        this._remoteDescriptionSet = false;
        this._pendingCandidates = [];

        // Use provided ICE servers or fall back to free Google STUN
        const servers = (iceServers && iceServers.length > 0)
            ? iceServers
            : [
                { urls: 'stun:stun.l.google.com:19302' },
                { urls: 'stun:stun1.l.google.com:19302' }
            ];

        const config = { iceServers: servers };

        this._peerConnection = new RTCPeerConnection(config);

        // ICE candidate handler
        this._peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                dotNetRef.invokeMethodAsync('OnIceCandidateGenerated',
                    event.candidate.candidate,
                    event.candidate.sdpMid,
                    event.candidate.sdpMLineIndex
                );
            }
        };

        // Remote stream handler
        this._peerConnection.ontrack = (event) => {
            const remoteVideo = document.getElementById('remoteVideo');
            if (remoteVideo && event.streams[0]) {
                remoteVideo.srcObject = event.streams[0];
            }
        };

        // Connection state change
        this._peerConnection.onconnectionstatechange = () => {
            const state = this._peerConnection.connectionState;
            dotNetRef.invokeMethodAsync('OnConnectionStateChanged', state);
        };

        // ICE connection state
        this._peerConnection.oniceconnectionstatechange = () => {
            const state = this._peerConnection.iceConnectionState;
            dotNetRef.invokeMethodAsync('OnIceConnectionStateChanged', state);
        };

        // Get local media
        try {
            const constraints = {
                audio: true,
                video: isVideo ? { width: { ideal: 640 }, height: { ideal: 480 }, facingMode: 'user' } : false
            };

            this._localStream = await navigator.mediaDevices.getUserMedia(constraints);

            // Show local video
            const localVideo = document.getElementById('localVideo');
            if (localVideo) {
                localVideo.srcObject = this._localStream;
            }

            // Add tracks to peer connection
            this._localStream.getTracks().forEach(track => {
                this._peerConnection.addTrack(track, this._localStream);
            });

            return true;
        } catch (err) {
            console.error('Failed to get media:', err);
            dotNetRef.invokeMethodAsync('OnMediaError', err.message);
            return false;
        }
    },

    /**
     * Create an SDP offer (caller side)
     */
    createOffer: async function () {
        if (!this._peerConnection) return null;
        try {
            const offer = await this._peerConnection.createOffer();
            await this._peerConnection.setLocalDescription(offer);
            return JSON.stringify(offer);
        } catch (err) {
            console.error('Failed to create offer:', err);
            return null;
        }
    },

    /**
     * Create an SDP answer (callee side)
     */
    createAnswer: async function () {
        if (!this._peerConnection) return null;
        try {
            const answer = await this._peerConnection.createAnswer();
            await this._peerConnection.setLocalDescription(answer);
            return JSON.stringify(answer);
        } catch (err) {
            console.error('Failed to create answer:', err);
            return null;
        }
    },

    /**
     * Set remote SDP description (offer or answer)
     */
    setRemoteDescription: async function (sdpJson) {
        if (!this._peerConnection) return false;
        try {
            const sdp = JSON.parse(sdpJson);
            await this._peerConnection.setRemoteDescription(new RTCSessionDescription(sdp));
            this._remoteDescriptionSet = true;

            // Flush any ICE candidates that arrived before remote description was set
            if (this._pendingCandidates.length > 0) {
                console.log(`Flushing ${this._pendingCandidates.length} buffered ICE candidates`);
                for (const c of this._pendingCandidates) {
                    await this._peerConnection.addIceCandidate(new RTCIceCandidate(c));
                }
                this._pendingCandidates = [];
            }

            return true;
        } catch (err) {
            console.error('Failed to set remote description:', err);
            return false;
        }
    },

    /**
     * Add a received ICE candidate
     */
    addIceCandidate: async function (candidate, sdpMid, sdpMLineIndex) {
        if (!this._peerConnection) return false;

        const iceCandidate = {
            candidate: candidate,
            sdpMid: sdpMid,
            sdpMLineIndex: sdpMLineIndex
        };

        // Buffer if remote description hasn't been set yet
        if (!this._remoteDescriptionSet) {
            this._pendingCandidates.push(iceCandidate);
            return true;
        }

        try {
            await this._peerConnection.addIceCandidate(new RTCIceCandidate(iceCandidate));
            return true;
        } catch (err) {
            console.error('Failed to add ICE candidate:', err);
            return false;
        }
    },

    /**
     * Toggle audio track on/off
     */
    toggleAudio: function (enabled) {
        if (!this._localStream) return;
        this._localStream.getAudioTracks().forEach(track => {
            track.enabled = enabled;
        });
    },

    /**
     * Toggle video track on/off
     */
    toggleVideo: function (enabled) {
        if (!this._localStream) return;
        this._localStream.getVideoTracks().forEach(track => {
            track.enabled = enabled;
        });
    },

    /**
     * Check if local stream has video track
     */
    hasVideoTrack: function () {
        if (!this._localStream) return false;
        return this._localStream.getVideoTracks().length > 0;
    },

    /**
     * Play a ringing tone using Web Audio API (no external file needed)
     * Pattern: dual-tone beep (440Hz + 480Hz) for 1s, silence for 2s, repeat
     */
    playRingtone: function () {
        this.stopRingtone();
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            this._ringtoneContext = ctx;

            // Resume context to handle browser autoplay policy
            if (ctx.state === 'suspended') {
                ctx.resume();
            }

            const playBeep = () => {
                if (ctx.state === 'closed') return;

                // Dual-tone: 440Hz + 480Hz for realistic ring sound
                const osc1 = ctx.createOscillator();
                const osc2 = ctx.createOscillator();
                const gain = ctx.createGain();

                osc1.connect(gain);
                osc2.connect(gain);
                gain.connect(ctx.destination);

                osc1.frequency.value = 440;
                osc2.frequency.value = 480;
                gain.gain.value = 0.15;

                const now = ctx.currentTime;
                osc1.start(now);
                osc2.start(now);
                osc1.stop(now + 1.0);
                osc2.stop(now + 1.0);

                this._ringtoneOscillator = osc1;
            };

            playBeep();
            this._ringtoneInterval = setInterval(playBeep, 3000);
        } catch (err) {
            console.error('Failed to play ringtone:', err);
        }
    },

    /**
     * Stop the ringing tone
     */
    stopRingtone: function () {
        if (this._ringtoneInterval) {
            clearInterval(this._ringtoneInterval);
            this._ringtoneInterval = null;
        }
        if (this._ringtoneOscillator) {
            try { this._ringtoneOscillator.stop(); } catch (_) {}
            this._ringtoneOscillator = null;
        }
        if (this._ringtoneContext) {
            try { this._ringtoneContext.close(); } catch (_) {}
            this._ringtoneContext = null;
        }
    },

    /**
     * Cleanup everything
     */
    dispose: function () {
        this.stopRingtone();

        // Stop all local tracks
        if (this._localStream) {
            this._localStream.getTracks().forEach(track => track.stop());
            this._localStream = null;
        }

        // Close peer connection
        if (this._peerConnection) {
            this._peerConnection.close();
            this._peerConnection = null;
        }

        // Clear video elements
        const localVideo = document.getElementById('localVideo');
        const remoteVideo = document.getElementById('remoteVideo');
        if (localVideo) localVideo.srcObject = null;
        if (remoteVideo) remoteVideo.srcObject = null;

        this._dotNetRef = null;
        this._remoteDescriptionSet = false;
        this._pendingCandidates = [];
    }
};
