window.editorExtensions = {
    getCursorIndex: function (element) {
        if (!element) return 0;
        return element.selectionStart;
    },

    insertText: function (element, text) {
        if (!element) return;
        const start = element.selectionStart;
        const end = element.selectionEnd;
        const value = element.value;
        
        element.value = value.substring(0, start) + text + value.substring(end);
        element.focus();
        element.selectionStart = element.selectionEnd = start + text.length;
        
        element.dispatchEvent(new Event('input', { bubbles: true }));
        element.dispatchEvent(new Event('change', { bubbles: true }));
    },
    
    getCaretCoordinates: function (element, position) {
        if (!element) return { top: 0, left: 0, lineHeight: 20 };
        
        const div = document.createElement('div');
        const style = getComputedStyle(element);
        
        Array.from(style).forEach(prop => {
            div.style[prop] = style[prop];
        });
        
        div.textContent = element.value.substring(0, position);
        const span = document.createElement('span');
        span.textContent = element.value.substring(position) || '.';
        div.appendChild(span);
        
        document.body.appendChild(div);
        div.style.position = 'absolute';
        div.style.top = '-9999px';
        div.style.left = '-9999px';
        div.style.whiteSpace = 'pre-wrap';
        
        const coordinates = {
            top: span.offsetTop - element.scrollTop + element.offsetTop,
            left: span.offsetLeft - element.scrollLeft + element.offsetLeft,
            lineHeight: parseFloat(style.lineHeight) || 20
        };
        
        document.body.removeChild(div);
        return coordinates;
    }
};
