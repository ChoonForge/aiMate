// Sidebar menu management for aiMate

window.sidebarHelper = {
    // Initialize click-outside handler for sidebar menus
    initializeClickOutside: function (dotNetHelper) {
        document.addEventListener('click', function (e) {
            // Check if click is outside all kebab menus
            const isKebabButton = e.target.closest('[data-kebab-button]');
            const isKebabMenu = e.target.closest('[data-kebab-menu]');

            if (!isKebabButton && !isKebabMenu) {
                // Close all menus
                dotNetHelper.invokeMethodAsync('CloseAllMenus');
            }
        });
    },

    // Add escape key handler to close menus
    initializeEscapeKey: function (dotNetHelper) {
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                dotNetHelper.invokeMethodAsync('CloseAllMenus');
            }
        });
    },

    // Smooth scroll to element in sidebar
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    }
};
