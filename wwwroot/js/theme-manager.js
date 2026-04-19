/**
 * PayroTech Theme Manager
 * Handles light/dark mode switching and persistence
 * Version: 3.0
 */

class ThemeManager {
    constructor() {
        this.storageKey = 'payrotech-theme';
        this.roleKey = 'payrotech-role';
        this.init();
    }

    /**
     * Initialize theme manager
     */
    init() {
        // Get saved theme or default to light
        const savedTheme = this.getSavedTheme();
        const savedRole = this.getSavedRole();
        
        // Apply theme
        this.applyTheme(savedTheme);
        
        // Apply role if available
        if (savedRole) {
            this.applyRole(savedRole);
        }
        
        // Listen for system theme changes
        this.watchSystemTheme();
        
        // Setup toggle buttons
        this.setupToggleButtons();
    }

    /**
     * Get saved theme from localStorage
     */
    getSavedTheme() {
        const saved = localStorage.getItem(this.storageKey);
        
        // If no saved preference, check system preference
        if (!saved) {
            return this.getSystemTheme();
        }
        
        return saved;
    }

    /**
     * Get system theme preference
     */
    getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

    /**
     * Get saved role from localStorage or meta tag
     */
    getSavedRole() {
        // Try localStorage first
        let role = localStorage.getItem(this.roleKey);
        
        // If not in localStorage, try meta tag
        if (!role) {
            const metaRole = document.querySelector('meta[name="user-role"]');
            if (metaRole) {
                role = metaRole.getAttribute('content');
                // Save to localStorage for future use
                if (role) {
                    localStorage.setItem(this.roleKey, role);
                }
            }
        }
        
        return role;
    }

    /**
     * Apply theme to document
     */
    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(this.storageKey, theme);
        
        // Update toggle button icons
        this.updateToggleIcons(theme);
        
        // Dispatch custom event
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme } }));
    }

    /**
     * Apply role to document
     */
    applyRole(role) {
        // Normalize role name
        const normalizedRole = role.toLowerCase().replace(/\s+/g, '');
        document.documentElement.setAttribute('data-role', normalizedRole);
        localStorage.setItem(this.roleKey, normalizedRole);
    }

    /**
     * Toggle between light and dark theme
     */
    toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.applyTheme(newTheme);
    }

    /**
     * Set specific theme
     */
    setTheme(theme) {
        if (theme === 'light' || theme === 'dark') {
            this.applyTheme(theme);
        }
    }

    /**
     * Get current theme
     */
    getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') || 'light';
    }

    /**
     * Watch for system theme changes
     */
    watchSystemTheme() {
        if (window.matchMedia) {
            const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            darkModeQuery.addEventListener('change', (e) => {
                // Only auto-switch if user hasn't manually set a preference
                const hasManualPreference = localStorage.getItem(this.storageKey);
                if (!hasManualPreference) {
                    this.applyTheme(e.matches ? 'dark' : 'light');
                }
            });
        }
    }

    /**
     * Setup toggle buttons
     */
    setupToggleButtons() {
        // Find all theme toggle buttons
        const toggleButtons = document.querySelectorAll('[data-theme-toggle]');
        
        toggleButtons.forEach(button => {
            button.addEventListener('click', () => {
                this.toggleTheme();
            });
        });
    }

    /**
     * Update toggle button icons
     */
    updateToggleIcons(theme) {
        const lightIcons = document.querySelectorAll('[data-theme-icon="light"]');
        const darkIcons = document.querySelectorAll('[data-theme-icon="dark"]');
        
        if (theme === 'dark') {
            lightIcons.forEach(icon => icon.style.display = 'inline-block');
            darkIcons.forEach(icon => icon.style.display = 'none');
        } else {
            lightIcons.forEach(icon => icon.style.display = 'none');
            darkIcons.forEach(icon => icon.style.display = 'inline-block');
        }
    }

    /**
     * Reset to system theme
     */
    resetToSystemTheme() {
        localStorage.removeItem(this.storageKey);
        const systemTheme = this.getSystemTheme();
        this.applyTheme(systemTheme);
    }

    /**
     * Get theme colors for current role
     */
    getThemeColors() {
        const computedStyle = getComputedStyle(document.documentElement);
        
        return {
            primary: computedStyle.getPropertyValue('--primary').trim(),
            secondary: computedStyle.getPropertyValue('--secondary').trim(),
            accent: computedStyle.getPropertyValue('--accent').trim(),
            background: computedStyle.getPropertyValue('--background').trim(),
            card: computedStyle.getPropertyValue('--card').trim(),
            border: computedStyle.getPropertyValue('--border').trim(),
            textHeading: computedStyle.getPropertyValue('--text-heading').trim(),
            textBody: computedStyle.getPropertyValue('--text-body').trim(),
            textMuted: computedStyle.getPropertyValue('--text-muted').trim(),
            success: computedStyle.getPropertyValue('--success').trim(),
            error: computedStyle.getPropertyValue('--error').trim(),
            warning: computedStyle.getPropertyValue('--warning').trim(),
            info: computedStyle.getPropertyValue('--info').trim()
        };
    }
}

// Initialize theme manager when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.themeManager = new ThemeManager();
    });
} else {
    window.themeManager = new ThemeManager();
}

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ThemeManager;
}
