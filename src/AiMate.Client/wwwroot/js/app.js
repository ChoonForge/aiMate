// aiMate JavaScript Interop

// Highlight code blocks
window.highlightCode = function () {
    document.querySelectorAll('pre code').forEach((block) => {
        if (!block.classList.contains('hljs')) {
            hljs.highlightElement(block);
        }
    });
};

// Setup keyboard shortcuts
window.setupKeyboardShortcuts = function () {
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + K: Open search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            DotNet.invokeMethodAsync('AiMate.Client', 'OpenSearch');
        }

        // Ctrl/Cmd + N: New conversation
        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            DotNet.invokeMethodAsync('AiMate.Client', 'NewConversation');
        }

        // Ctrl/Cmd + ,: Open settings
        if ((e.ctrlKey || e.metaKey) && e.key === ',') {
            e.preventDefault();
            DotNet.invokeMethodAsync('AiMate.Client', 'OpenSettings');
        }
    });
};

// Copy text to clipboard
window.copyToClipboard = function (text) {
    return navigator.clipboard.writeText(text)
        .then(() => true)
        .catch(() => false);
};

// Scroll to bottom of chat
window.scrollToBottom = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

// Auto-grow textarea
window.autoGrowTextarea = function (element) {
    if (element) {
        element.style.height = 'auto';
        element.style.height = (element.scrollHeight) + 'px';
    }
};

// Text-to-Speech (Read Aloud)
window.speakText = function (text) {
    if ('speechSynthesis' in window) {
        // Cancel any ongoing speech
        window.speechSynthesis.cancel();
        
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = 1.0;
        utterance.pitch = 1.0;
        utterance.volume = 1.0;
        
        window.speechSynthesis.speak(utterance);
        return true;
    }
    return false;
};

// Stop speech
window.stopSpeaking = function () {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
};

// Download file
window.downloadFile = function (filename, content) {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// Local storage helpers
window.localStorage = {
    setItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    getItem: function (key) {
        return localStorage.getItem(key);
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    },
    clear: function () {
        localStorage.clear();
    }
};

// IndexedDB wrapper for large file storage
window.fileStorage = {
    async saveFile(id, file) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('AiMateFiles', 1);
            
            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                const db = request.result;
                const transaction = db.transaction(['files'], 'readwrite');
                const store = transaction.objectStore('files');
                store.put({ id, file });
                transaction.oncomplete = () => resolve(true);
                transaction.onerror = () => reject(transaction.error);
            };
            
            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                if (!db.objectStoreNames.contains('files')) {
                    db.createObjectStore('files', { keyPath: 'id' });
                }
            };
        });
    },
    
    async getFile(id) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('AiMateFiles', 1);
            
            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                const db = request.result;
                const transaction = db.transaction(['files'], 'readonly');
                const store = transaction.objectStore('files');
                const getRequest = store.get(id);
                
                getRequest.onsuccess = () => resolve(getRequest.result?.file);
                getRequest.onerror = () => reject(getRequest.error);
            };
        });
    },
    
    async deleteFile(id) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('AiMateFiles', 1);
            
            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                const db = request.result;
                const transaction = db.transaction(['files'], 'readwrite');
                const store = transaction.objectStore('files');
                store.delete(id);
                transaction.oncomplete = () => resolve(true);
                transaction.onerror = () => reject(transaction.error);
            };
        });
    }
};

// Initialize highlight.js
if (typeof hljs !== 'undefined') {
    hljs.configure({
        languages: ['javascript', 'python', 'java', 'csharp', 'cpp', 'css', 'html', 'sql', 'bash', 'json', 'xml']
    });
}

console.log('aiMate initialized! 🚀');
