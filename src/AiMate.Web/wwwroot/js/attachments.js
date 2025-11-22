// File Upload Functionality
window.attachmentHelpers = {
    // Trigger file input click
    triggerFileInput: function (inputId) {
        const input = document.getElementById(inputId);
        if (input) {
            input.click();
        }
    },

    // Read file as base64
    readFileAsBase64: async function (file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => {
                const base64 = reader.result.split(',')[1];
                resolve({
                    name: file.name,
                    size: file.size,
                    type: file.type,
                    data: base64
                });
            };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    },

    // Handle file selection
    handleFileSelection: async function (inputElement) {
        if (!inputElement || !inputElement.files || inputElement.files.length === 0) {
            return null;
        }

        const files = [];
        for (let i = 0; i < inputElement.files.length; i++) {
            const fileData = await this.readFileAsBase64(inputElement.files[i]);
            files.push(fileData);
        }

        // Clear input for reuse
        inputElement.value = '';
        return files;
    },

    // Screenshot Capture
    captureScreenshot: async function () {
        try {
            const stream = await navigator.mediaDevices.getDisplayMedia({
                video: { mediaSource: 'screen' }
            });

            const video = document.createElement('video');
            video.srcObject = stream;
            video.play();

            // Wait for video to be ready
            await new Promise(resolve => {
                video.onloadedmetadata = resolve;
            });

            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;

            const ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0);

            // Stop all tracks
            stream.getTracks().forEach(track => track.stop());

            // Convert to base64
            const base64 = canvas.toDataURL('image/png').split(',')[1];

            return {
                name: `screenshot_${new Date().getTime()}.png`,
                type: 'image/png',
                data: base64
            };
        } catch (err) {
            console.error('Screenshot capture failed:', err);
            throw err;
        }
    },

    // Camera/Webcam Capture
    capturePhoto: async function () {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: 'user' }
            });

            // Create video element
            const video = document.createElement('video');
            video.srcObject = stream;
            video.autoplay = true;
            video.style.position = 'fixed';
            video.style.top = '50%';
            video.style.left = '50%';
            video.style.transform = 'translate(-50%, -50%)';
            video.style.zIndex = '10000';
            video.style.maxWidth = '80%';
            video.style.maxHeight = '80%';
            video.style.border = '4px solid #8B5CF6';
            video.style.borderRadius = '8px';

            // Create capture button
            const button = document.createElement('button');
            button.textContent = 'Capture Photo';
            button.style.position = 'fixed';
            button.style.bottom = '20px';
            button.style.left = '50%';
            button.style.transform = 'translateX(-50%)';
            button.style.zIndex = '10001';
            button.style.padding = '12px 24px';
            button.style.backgroundColor = '#8B5CF6';
            button.style.color = 'white';
            button.style.border = 'none';
            button.style.borderRadius = '8px';
            button.style.fontSize = '16px';
            button.style.fontWeight = 'bold';
            button.style.cursor = 'pointer';

            // Create cancel button
            const cancelButton = document.createElement('button');
            cancelButton.textContent = 'Cancel';
            cancelButton.style.position = 'fixed';
            cancelButton.style.bottom = '20px';
            cancelButton.style.right = '20px';
            cancelButton.style.zIndex = '10001';
            cancelButton.style.padding = '12px 24px';
            cancelButton.style.backgroundColor = '#6B7280';
            cancelButton.style.color = 'white';
            cancelButton.style.border = 'none';
            cancelButton.style.borderRadius = '8px';
            cancelButton.style.fontSize = '16px';
            cancelButton.style.cursor = 'pointer';

            // Create overlay
            const overlay = document.createElement('div');
            overlay.style.position = 'fixed';
            overlay.style.top = '0';
            overlay.style.left = '0';
            overlay.style.width = '100%';
            overlay.style.height = '100%';
            overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.8)';
            overlay.style.zIndex = '9999';

            document.body.appendChild(overlay);
            document.body.appendChild(video);
            document.body.appendChild(button);
            document.body.appendChild(cancelButton);

            return new Promise((resolve, reject) => {
                button.onclick = () => {
                    const canvas = document.createElement('canvas');
                    canvas.width = video.videoWidth;
                    canvas.height = video.videoHeight;

                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(video, 0, 0);

                    // Stop stream
                    stream.getTracks().forEach(track => track.stop());

                    // Clean up
                    document.body.removeChild(overlay);
                    document.body.removeChild(video);
                    document.body.removeChild(button);
                    document.body.removeChild(cancelButton);

                    // Convert to base64
                    const base64 = canvas.toDataURL('image/jpeg', 0.95).split(',')[1];

                    resolve({
                        name: `photo_${new Date().getTime()}.jpg`,
                        type: 'image/jpeg',
                        data: base64
                    });
                };

                cancelButton.onclick = () => {
                    // Stop stream
                    stream.getTracks().forEach(track => track.stop());

                    // Clean up
                    document.body.removeChild(overlay);
                    document.body.removeChild(video);
                    document.body.removeChild(button);
                    document.body.removeChild(cancelButton);

                    reject(new Error('Cancelled by user'));
                };
            });
        } catch (err) {
            console.error('Camera capture failed:', err);
            throw err;
        }
    },

    // Clipboard functionality
    readClipboardText: async function () {
        try {
            return await navigator.clipboard.readText();
        } catch (err) {
            console.error('Clipboard read failed:', err);
            throw err;
        }
    },

    // Download file
    downloadFile: function (filename, content, mimeType) {
        const blob = new Blob([content], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};
