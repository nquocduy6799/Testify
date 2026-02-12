// WebRTC JS Interop for Blazor
// This is the minimal JavaScript required because WebRTC APIs
// (RTCPeerConnection, getUserMedia, MediaStream) are browser-only APIs
// with no C#/WASM equivalent.

window.WebRtcInterop = {
    _peerConnection: null,
    _localStream: null,
    _dotNetRef: null,

    /**
     * Initialize a new WebRTC peer connection
     * @param {object} dotNetRef - .NET object reference for callbacks
     * @param {boolean} isVideo - Whether to include video track
     */
    initialize: async function (dotNetRef, isVideo) {
        this._dotNetRef = dotNetRef;

        // ICE servers - free Google STUN for dev
        // Add TURN server here for production
        const config = {
            iceServers: [
                { urls: 'stun:stun.l.google.com:19302' },
                { urls: 'stun:stun1.l.google.com:19302' }
            ]
        };

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
        try {
            await this._peerConnection.addIceCandidate(new RTCIceCandidate({
                candidate: candidate,
                sdpMid: sdpMid,
                sdpMLineIndex: sdpMLineIndex
            }));
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
     * Cleanup everything
     */
    dispose: function () {
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
    }
};
