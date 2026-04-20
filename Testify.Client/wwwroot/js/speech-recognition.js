window.meetingSpeechRecognition = {
    _recognition: null,
    _dotNetRef: null,

    init: function (dotNetRef) {
        this._dotNetRef = dotNetRef;

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            console.warn('[SpeechRecognition] Not supported in this browser');
            return;
        }

        this._recognition = new SpeechRecognition();
        this._recognition.continuous = true;
        this._recognition.interimResults = true;
        this._recognition.lang = 'vi-VN'; // Vietnamese - change as needed

        this._recognition.onresult = (event) => {
            let interimTranscript = '';
            let finalTranscript = '';

            for (let i = event.resultIndex; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;
                if (event.results[i].isFinal) {
                    finalTranscript += transcript;
                } else {
                    interimTranscript += transcript;
                }
            }

            if (finalTranscript) {
                this._dotNetRef.invokeMethodAsync('OnSpeechResult', finalTranscript, true);
            } else if (interimTranscript) {
                this._dotNetRef.invokeMethodAsync('OnSpeechResult', interimTranscript, false);
            }
        };

        this._recognition.onerror = (event) => {
            if (event.error !== 'no-speech' && event.error !== 'aborted') {
                console.error('[SpeechRecognition] Error:', event.error);
                this._dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
            }
        };

        this._recognition.onend = () => {
            this._dotNetRef.invokeMethodAsync('OnSpeechEnd');
        };
    },

    start: function () {
        if (this._recognition) {
            try {
                this._recognition.start();
            } catch (e) {
                // Already started - ignore
            }
        }
    },

    stop: function () {
        if (this._recognition) {
            try {
                this._recognition.stop();
            } catch (e) {
                // Already stopped - ignore
            }
        }
    },

    dispose: function () {
        this.stop();
        this._recognition = null;
        this._dotNetRef = null;
    },

    scrollToBottom: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.scrollTop = el.scrollHeight;
        }
    }
};
