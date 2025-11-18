/**
 * aiMate.nz - JavaScript Utilities
 * Helper functions for Blazor interop
 */

/**
 * Download a file from base64 data
 * @param {string} filename - The name of the file to download
 * @param {string} contentType - MIME type (e.g., 'application/json', 'text/plain')
 * @param {string} base64Data - Base64 encoded file content
 */
window.downloadFile = function (filename, contentType, base64Data) {
    try {
        // Create a blob from the base64 data
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);

        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType });

        // Create download link
        const url = URL.createObjectURL(blob);
        const linkElement = document.createElement('a');
        linkElement.setAttribute('href', url);
        linkElement.setAttribute('download', filename);
        linkElement.style.display = 'none';

        // Trigger download
        document.body.appendChild(linkElement);
        linkElement.click();

        // Cleanup
        document.body.removeChild(linkElement);
        URL.revokeObjectURL(url);

        return true;
    } catch (error) {
        console.error('Download failed:', error);
        return false;
    }
};

/**
 * Copy text to clipboard
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} Success status
 */
window.copyToClipboard = async function (text) {
    try {
        if (navigator.clipboard && navigator.clipboard.writeText) {
            await navigator.clipboard.writeText(text);
            return true;
        } else {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            textArea.style.top = '-999999px';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();

            const successful = document.execCommand('copy');
            document.body.removeChild(textArea);
            return successful;
        }
    } catch (error) {
        console.error('Copy to clipboard failed:', error);
        return false;
    }
};

/**
 * Focus an element by ID
 * @param {string} elementId - DOM element ID
 */
window.focusElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};

/**
 * Scroll an element into view
 * @param {string} elementId - DOM element ID
 * @param {boolean} smooth - Use smooth scrolling (default: true)
 */
window.scrollToElement = function (elementId, smooth = true) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            behavior: smooth ? 'smooth' : 'auto',
            block: 'nearest'
        });
    }
};

/**
 * Get element dimensions
 * @param {string} elementId - DOM element ID
 * @returns {object} Width and height
 */
window.getElementDimensions = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            width: rect.width,
            height: rect.height
        };
    }
    return { width: 0, height: 0 };
};

console.log('aiMate.nz - JavaScript utilities loaded ✓');
