// Chat-related JavaScript utilities

window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.isScrolledToBottom = (element, threshold = 100) => {
    if (!element) return true;
    const distanceFromBottom = element.scrollHeight - element.scrollTop - element.clientHeight;
    return distanceFromBottom <= threshold;
};

window.smoothScrollToBottom = (element) => {
    if (element) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: 'smooth'
        });
    }
};

// Initialize scroll observer for chat container
window.initChatScrollObserver = (element, dotNetHelper) => {
    if (!element) return;

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                dotNetHelper.invokeMethodAsync('OnScrolledToBottom');
            }
        });
    }, {
        root: element,
        threshold: 1.0
    });

    // Observe a sentinel element at the bottom
    const sentinel = document.createElement('div');
    sentinel.className = 'scroll-sentinel';
    element.appendChild(sentinel);
    observer.observe(sentinel);

    return () => {
        observer.disconnect();
        sentinel.remove();
    };
};

// Copy code block to clipboard
window.copyCodeBlock = async (code) => {
    try {
        await navigator.clipboard.writeText(code);
        return true;
    } catch (err) {
        console.error('Failed to copy code:', err);
        return false;
    }
};

// Initialize syntax highlighting (using highlight.js if available)
window.initSyntaxHighlighting = () => {
    if (typeof hljs !== 'undefined') {
        document.querySelectorAll('pre code').forEach((block) => {
            if (!block.classList.contains('hljs')) {
                hljs.highlightElement(block);
            }
        });
    }
};

// Auto-resize textarea
window.autoResizeTextarea = (element) => {
    if (!element) return;
    element.style.height = 'auto';
    element.style.height = element.scrollHeight + 'px';
};

// Focus element
window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

// Initialize speech synthesis for read-aloud
window.readAloud = (text, options = {}) => {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel(); // Cancel any ongoing speech

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = options.rate || 1.0;
        utterance.pitch = options.pitch || 1.0;
        utterance.volume = options.volume || 1.0;

        if (options.lang) {
            utterance.lang = options.lang;
        }

        window.speechSynthesis.speak(utterance);
        return true;
    }
    return false;
};

window.stopReading = () => {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
};
