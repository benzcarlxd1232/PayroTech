/**
 * PayroTech Design System JavaScript Module
 * Provides interactive components: Command Palette, Toasts, Skeletons, Animations, etc.
 */

(function (window) {
    'use strict';

    // PayroTech Design System Namespace
    const PT = {
        version: '1.0.0',
        config: {
            animationDuration: 250,
            toastDuration: 5000,
        }
    };

    // ================================
    // 1. TOAST NOTIFICATION SYSTEM
    // ================================

    PT.Toast = {
        container: null,

        init() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.className = 'pt-toast-container';
                document.body.appendChild(this.container);
            }
        },

        show(options) {
            this.init();

            const defaults = {
                type: 'info', // success, warning, error, info
                title: '',
                message: '',
                duration: PT.config.toastDuration,
                closable: true
            };

            const settings = { ...defaults, ...options };

            const icons = {
                success: 'bi-check-circle-fill',
                warning: 'bi-exclamation-triangle-fill',
                error: 'bi-x-circle-fill',
                info: 'bi-info-circle-fill'
            };

            const toast = document.createElement('div');
            toast.className = `pt-toast ${settings.type}`;
            toast.innerHTML = `
                <div class="pt-toast-icon">
                    <i class="bi ${icons[settings.type]}"></i>
                </div>
                <div class="pt-toast-content">
                    ${settings.title ? `<div class="pt-toast-title">${settings.title}</div>` : ''}
                    <div class="pt-toast-message">${settings.message}</div>
                </div>
                ${settings.closable ? '<button class="pt-toast-close"><i class="bi bi-x"></i></button>' : ''}
            `;

            this.container.appendChild(toast);

            // Close button handler
            if (settings.closable) {
                toast.querySelector('.pt-toast-close').addEventListener('click', () => {
                    this.dismiss(toast);
                });
            }

            // Auto dismiss
            if (settings.duration > 0) {
                setTimeout(() => this.dismiss(toast), settings.duration);
            }

            return toast;
        },

        dismiss(toast) {
            toast.style.animation = 'pt-fadeOut 0.2s ease-out forwards';
            setTimeout(() => toast.remove(), 200);
        },

        success(message, title = 'Success') {
            return this.show({ type: 'success', title, message });
        },

        error(message, title = 'Error') {
            return this.show({ type: 'error', title, message });
        },

        warning(message, title = 'Warning') {
            return this.show({ type: 'warning', title, message });
        },

        info(message, title = 'Info') {
            return this.show({ type: 'info', title, message });
        }
    };

    // ================================
    // 2. COMMAND PALETTE (Cmd+K)
    // ================================

    PT.CommandPalette = {
        isOpen: false,
        backdrop: null,
        palette: null,
        items: [],
        selectedIndex: 0,
        filteredItems: [],

        init(items = []) {
            this.items = items.length > 0 ? items : this.getDefaultItems();
            this.createElements();
            this.bindKeyboardShortcuts();
        },

        getDefaultItems() {
            return [
                { id: 'dashboard', icon: 'bi-speedometer2', title: 'Dashboard', description: 'Go to main dashboard', action: () => window.location.href = '/' },
                { id: 'employees', icon: 'bi-people', title: 'Employees', description: 'View employee list', action: () => window.location.href = '/Employee', shortcut: ['E'] },
                { id: 'new-employee', icon: 'bi-person-plus', title: 'Add Employee', description: 'Create new employee', action: () => window.location.href = '/Employee/Create', shortcut: ['N'] },
                { id: 'departments', icon: 'bi-diagram-3', title: 'Departments', description: 'Manage departments', action: () => window.location.href = '/Department' },
                { id: 'attendance', icon: 'bi-clock', title: 'Attendance', description: 'View attendance records', action: () => window.location.href = '/Attendance' },
                { id: 'time-in', icon: 'bi-box-arrow-in-right', title: 'Time In', description: 'Record time in', action: () => window.location.href = '/Attendance/TimeIn' },
                { id: 'time-out', icon: 'bi-box-arrow-right', title: 'Time Out', description: 'Record time out', action: () => window.location.href = '/Attendance/TimeOut' },
                { id: 'leave', icon: 'bi-calendar-check', title: 'Leave Management', description: 'File and manage leaves', action: () => window.location.href = '/Leave' },
                { id: 'overtime', icon: 'bi-clock-history', title: 'Overtime', description: 'Manage overtime requests', action: () => window.location.href = '/Overtime' },
                { id: 'payroll', icon: 'bi-cash-stack', title: 'Payroll', description: 'Process payroll', action: () => window.location.href = '/Payroll' },
                { id: 'toggle-theme', icon: 'bi-moon-fill', title: 'Toggle Dark Mode', description: 'Switch theme', action: () => PT.Theme.toggle(), shortcut: ['D'] },
            ];
        },

        createElements() {
            // Backdrop
            this.backdrop = document.createElement('div');
            this.backdrop.className = 'pt-command-palette-backdrop';
            this.backdrop.style.display = 'none';
            this.backdrop.addEventListener('click', () => this.close());

            // Palette
            this.palette = document.createElement('div');
            this.palette.className = 'pt-command-palette';
            this.palette.style.display = 'none';
            this.palette.innerHTML = `
                <input type="text" class="pt-command-palette-input" placeholder="Search commands or navigate..." autofocus>
                <div class="pt-command-palette-results"></div>
            `;

            document.body.appendChild(this.backdrop);
            document.body.appendChild(this.palette);

            // Input handlers
            const input = this.palette.querySelector('.pt-command-palette-input');
            input.addEventListener('input', (e) => this.filter(e.target.value));
            input.addEventListener('keydown', (e) => this.handleKeydown(e));
        },

        bindKeyboardShortcuts() {
            document.addEventListener('keydown', (e) => {
                // Cmd/Ctrl + K to open
                if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
                    e.preventDefault();
                    this.toggle();
                }

                // Escape to close
                if (e.key === 'Escape' && this.isOpen) {
                    this.close();
                }
            });
        },

        open() {
            this.isOpen = true;
            this.backdrop.style.display = 'block';
            this.palette.style.display = 'block';
            this.selectedIndex = 0;
            this.filter('');
            
            const input = this.palette.querySelector('.pt-command-palette-input');
            input.value = '';
            setTimeout(() => input.focus(), 50);
        },

        close() {
            this.isOpen = false;
            this.palette.style.animation = 'pt-scaleOut 0.15s ease-out forwards';
            this.backdrop.style.animation = 'pt-fadeOut 0.15s ease-out forwards';
            
            setTimeout(() => {
                this.backdrop.style.display = 'none';
                this.palette.style.display = 'none';
                this.palette.style.animation = '';
                this.backdrop.style.animation = '';
            }, 150);
        },

        toggle() {
            this.isOpen ? this.close() : this.open();
        },

        filter(query) {
            const q = query.toLowerCase().trim();
            this.filteredItems = q
                ? this.items.filter(item => 
                    item.title.toLowerCase().includes(q) || 
                    item.description.toLowerCase().includes(q))
                : this.items;

            this.selectedIndex = 0;
            this.render();
        },

        render() {
            const results = this.palette.querySelector('.pt-command-palette-results');
            
            if (this.filteredItems.length === 0) {
                results.innerHTML = `
                    <div style="padding: 2rem; text-align: center; color: var(--pt-text-secondary);">
                        <i class="bi bi-search" style="font-size: 2rem; margin-bottom: 0.5rem; display: block;"></i>
                        No results found
                    </div>
                `;
                return;
            }

            results.innerHTML = this.filteredItems.map((item, index) => `
                <div class="pt-command-item ${index === this.selectedIndex ? 'selected' : ''}" data-index="${index}">
                    <div class="pt-command-item-icon">
                        <i class="bi ${item.icon}"></i>
                    </div>
                    <div>
                        <div class="pt-command-item-title">${item.title}</div>
                        <div class="pt-command-item-description">${item.description}</div>
                    </div>
                    ${item.shortcut ? `
                        <div class="pt-command-shortcut">
                            ${item.shortcut.map(k => `<kbd>${k}</kbd>`).join('')}
                        </div>
                    ` : ''}
                </div>
            `).join('');

            // Click handlers
            results.querySelectorAll('.pt-command-item').forEach((el, index) => {
                el.addEventListener('click', () => this.execute(index));
                el.addEventListener('mouseenter', () => {
                    this.selectedIndex = index;
                    this.render();
                });
            });
        },

        handleKeydown(e) {
            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    this.selectedIndex = Math.min(this.selectedIndex + 1, this.filteredItems.length - 1);
                    this.render();
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    this.selectedIndex = Math.max(this.selectedIndex - 1, 0);
                    this.render();
                    break;
                case 'Enter':
                    e.preventDefault();
                    this.execute(this.selectedIndex);
                    break;
            }
        },

        execute(index) {
            const item = this.filteredItems[index];
            if (item && item.action) {
                this.close();
                item.action();
            }
        }
    };

    // ================================
    // 3. THEME MANAGEMENT
    // ================================

    PT.Theme = {
        currentTheme: 'light',

        init() {
            const saved = localStorage.getItem('pt-theme');
            if (saved) {
                this.set(saved);
            } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
                this.set('dark');
            }

            // Listen for system preference changes
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (!localStorage.getItem('pt-theme')) {
                    this.set(e.matches ? 'dark' : 'light');
                }
            });
        },

        set(theme) {
            this.currentTheme = theme;
            document.documentElement.classList.toggle('dark-mode', theme === 'dark');
            document.body.classList.toggle('dark-mode', theme === 'dark');
            localStorage.setItem('pt-theme', theme);

            // Update theme toggle icon if present
            const toggleIcon = document.querySelector('.theme-toggle i');
            if (toggleIcon) {
                toggleIcon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
            }
        },

        toggle() {
            this.set(this.currentTheme === 'dark' ? 'light' : 'dark');
            PT.Toast.info(
                `Theme switched to ${this.currentTheme} mode`,
                'Theme Changed'
            );
        },

        get() {
            return this.currentTheme;
        }
    };

    // ================================
    // 4. SKELETON LOADERS
    // ================================

    PT.Skeleton = {
        show(container, type = 'card', count = 3) {
            const el = typeof container === 'string' ? document.querySelector(container) : container;
            if (!el) return;

            let html = '';
            for (let i = 0; i < count; i++) {
                switch (type) {
                    case 'card':
                        html += '<div class="pt-skeleton pt-skeleton-card pt-stagger-' + (i + 1) + '"></div>';
                        break;
                    case 'stat':
                        html += '<div class="pt-skeleton pt-skeleton-stat pt-stagger-' + (i + 1) + '"></div>';
                        break;
                    case 'table':
                        html += `
                            <div class="pt-skeleton-table-row pt-stagger-${i + 1}">
                                <div class="pt-skeleton"></div>
                                <div class="pt-skeleton"></div>
                                <div class="pt-skeleton"></div>
                                <div class="pt-skeleton"></div>
                            </div>
                        `;
                        break;
                    case 'text':
                        html += `
                            <div class="pt-skeleton pt-skeleton-text full pt-stagger-${i + 1}"></div>
                            <div class="pt-skeleton pt-skeleton-text medium pt-stagger-${i + 1}"></div>
                            <div class="pt-skeleton pt-skeleton-text short pt-stagger-${i + 1}"></div>
                        `;
                        break;
                }
            }

            el.innerHTML = html;
        },

        hide(container) {
            const el = typeof container === 'string' ? document.querySelector(container) : container;
            if (el) {
                el.innerHTML = '';
            }
        }
    };

    // ================================
    // 5. SCROLL ANIMATIONS
    // ================================

    PT.ScrollAnimation = {
        observer: null,

        init(selector = '[data-animate]') {
            if (!('IntersectionObserver' in window)) return;

            this.observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const animation = entry.target.dataset.animate || 'fadeInUp';
                        entry.target.classList.add('pt-animate-' + animation);
                        this.observer.unobserve(entry.target);
                    }
                });
            }, {
                threshold: 0.1,
                rootMargin: '0px 0px -50px 0px'
            });

            document.querySelectorAll(selector).forEach(el => {
                el.style.opacity = '0';
                this.observer.observe(el);
            });
        }
    };

    // ================================
    // 6. FLOATING ACTION BUTTON
    // ================================

    PT.FAB = {
        fabElement: null,
        menuElement: null,
        isOpen: false,

        init(actions = []) {
            if (actions.length === 0) {
                actions = this.getDefaultActions();
            }

            this.createElements(actions);
            this.bindEvents();
        },

        getDefaultActions() {
            return [
                { icon: 'bi-person-plus', label: 'Add Employee', action: () => window.location.href = '/Employee/Create' },
                { icon: 'bi-calendar-plus', label: 'File Leave', action: () => window.location.href = '/Leave/Create' },
                { icon: 'bi-clock-history', label: 'File Overtime', action: () => window.location.href = '/Overtime/Create' },
            ];
        },

        createElements(actions) {
            // FAB Menu
            this.menuElement = document.createElement('div');
            this.menuElement.className = 'pt-fab-menu';
            this.menuElement.innerHTML = actions.map(action => `
                <div class="pt-fab-menu-item">
                    <span class="pt-fab-label">${action.label}</span>
                    <button class="pt-fab-mini" data-action="${action.label}">
                        <i class="bi ${action.icon}"></i>
                    </button>
                </div>
            `).join('');

            // Main FAB
            this.fabElement = document.createElement('button');
            this.fabElement.className = 'pt-fab';
            this.fabElement.innerHTML = '<i class="bi bi-plus-lg"></i>';

            document.body.appendChild(this.menuElement);
            document.body.appendChild(this.fabElement);

            // Action handlers
            actions.forEach(action => {
                const btn = this.menuElement.querySelector(`[data-action="${action.label}"]`);
                if (btn) {
                    btn.addEventListener('click', () => {
                        this.close();
                        action.action();
                    });
                }
            });
        },

        bindEvents() {
            this.fabElement.addEventListener('click', () => this.toggle());

            // Close on outside click
            document.addEventListener('click', (e) => {
                if (this.isOpen && 
                    !this.fabElement.contains(e.target) && 
                    !this.menuElement.contains(e.target)) {
                    this.close();
                }
            });
        },

        toggle() {
            this.isOpen ? this.close() : this.open();
        },

        open() {
            this.isOpen = true;
            this.menuElement.classList.add('open');
            this.fabElement.innerHTML = '<i class="bi bi-x-lg"></i>';
            this.fabElement.style.transform = 'rotate(90deg)';
        },

        close() {
            this.isOpen = false;
            this.menuElement.classList.remove('open');
            this.fabElement.innerHTML = '<i class="bi bi-plus-lg"></i>';
            this.fabElement.style.transform = 'rotate(0deg)';
        }
    };

    // ================================
    // 7. PROGRESS COMPONENTS
    // ================================

    PT.Progress = {
        createBar(container, value = 0, options = {}) {
            const el = typeof container === 'string' ? document.querySelector(container) : container;
            if (!el) return;

            const { variant = '', animated = true } = options;

            el.innerHTML = `
                <div class="pt-progress">
                    <div class="pt-progress-bar ${variant}" style="--progress-width: ${value}%; width: ${animated ? '0' : value + '%'}"></div>
                </div>
            `;

            if (animated) {
                setTimeout(() => {
                    el.querySelector('.pt-progress-bar').style.width = value + '%';
                }, 100);
            }

            return {
                update: (newValue) => {
                    el.querySelector('.pt-progress-bar').style.width = newValue + '%';
                }
            };
        },

        createCircle(container, value = 0, options = {}) {
            const el = typeof container === 'string' ? document.querySelector(container) : container;
            if (!el) return;

            const { size = 60, strokeWidth = 6, showValue = true } = options;
            const radius = (size - strokeWidth) / 2;
            const circumference = radius * 2 * Math.PI;
            const offset = circumference - (value / 100) * circumference;

            el.innerHTML = `
                <div class="pt-progress-circle" style="--size: ${size}px; --stroke-width: ${strokeWidth}px;">
                    <svg viewBox="0 0 ${size} ${size}">
                        <circle class="track" cx="${size/2}" cy="${size/2}" r="${radius}"/>
                        <circle class="progress" cx="${size/2}" cy="${size/2}" r="${radius}"
                            style="stroke-dasharray: ${circumference}; stroke-dashoffset: ${circumference}"/>
                    </svg>
                    ${showValue ? `<div class="value">${value}%</div>` : ''}
                </div>
            `;

            // Animate
            setTimeout(() => {
                el.querySelector('.progress').style.strokeDashoffset = offset;
            }, 100);

            return {
                update: (newValue) => {
                    const newOffset = circumference - (newValue / 100) * circumference;
                    el.querySelector('.progress').style.strokeDashoffset = newOffset;
                    if (showValue) {
                        el.querySelector('.value').textContent = newValue + '%';
                    }
                }
            };
        }
    };

    // ================================
    // 8. DATA EXPORT
    // ================================

    PT.Export = {
        toCSV(data, filename = 'export.csv') {
            if (!data || data.length === 0) return;

            const headers = Object.keys(data[0]);
            const csvContent = [
                headers.join(','),
                ...data.map(row => headers.map(h => `"${row[h] || ''}"`).join(','))
            ].join('\n');

            this.download(csvContent, filename, 'text/csv');
        },

        toJSON(data, filename = 'export.json') {
            const content = JSON.stringify(data, null, 2);
            this.download(content, filename, 'application/json');
        },

        download(content, filename, mimeType) {
            const blob = new Blob([content], { type: mimeType });
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }
    };

    // ================================
    // 9. FORM VALIDATION
    // ================================

    PT.Validate = {
        init(formSelector) {
            const form = document.querySelector(formSelector);
            if (!form) return;

            form.addEventListener('submit', (e) => {
                if (!this.validateForm(form)) {
                    e.preventDefault();
                }
            });

            // Real-time validation
            form.querySelectorAll('.pt-input[required], input[required]').forEach(input => {
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', () => {
                    if (input.classList.contains('error')) {
                        this.validateField(input);
                    }
                });
            });
        },

        validateForm(form) {
            let isValid = true;
            form.querySelectorAll('.pt-input[required], input[required]').forEach(input => {
                if (!this.validateField(input)) {
                    isValid = false;
                }
            });
            return isValid;
        },

        validateField(input) {
            const value = input.value.trim();
            let isValid = true;
            let message = '';

            // Required check
            if (input.hasAttribute('required') && !value) {
                isValid = false;
                message = 'This field is required';
            }

            // Email check
            if (isValid && input.type === 'email' && value) {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(value)) {
                    isValid = false;
                    message = 'Please enter a valid email';
                }
            }

            // Min length check
            if (isValid && input.minLength > 0 && value.length < input.minLength) {
                isValid = false;
                message = `Minimum ${input.minLength} characters required`;
            }

            // Update UI
            input.classList.toggle('error', !isValid);
            
            // Error message
            let errorEl = input.parentElement.querySelector('.pt-error-message');
            if (!isValid) {
                if (!errorEl) {
                    errorEl = document.createElement('span');
                    errorEl.className = 'pt-error-message pt-text-danger pt-text-sm';
                    input.parentElement.appendChild(errorEl);
                }
                errorEl.textContent = message;
            } else if (errorEl) {
                errorEl.remove();
            }

            return isValid;
        }
    };

    // ================================
    // 10. BREADCRUMB MANAGER
    // ================================

    PT.Breadcrumb = {
        init(container, items = []) {
            const el = typeof container === 'string' ? document.querySelector(container) : container;
            if (!el) return;

            if (items.length === 0) {
                items = this.getFromUrl();
            }

            el.innerHTML = items.map((item, index) => `
                ${index > 0 ? '<span class="pt-breadcrumb-separator"><i class="bi bi-chevron-right"></i></span>' : ''}
                <a href="${item.url || '#'}" class="pt-breadcrumb-item ${index === items.length - 1 ? 'active' : ''}">
                    ${item.icon ? `<i class="bi ${item.icon}"></i>` : ''}
                    ${item.label}
                </a>
            `).join('');
        },

        getFromUrl() {
            const path = window.location.pathname;
            const segments = path.split('/').filter(s => s);
            
            const items = [{ label: 'Home', url: '/', icon: 'bi-house' }];
            
            let currentPath = '';
            segments.forEach((segment, index) => {
                currentPath += '/' + segment;
                items.push({
                    label: this.formatLabel(segment),
                    url: index === segments.length - 1 ? null : currentPath
                });
            });

            return items;
        },

        formatLabel(segment) {
            return segment
                .replace(/([A-Z])/g, ' $1')
                .replace(/-/g, ' ')
                .replace(/^\w/, c => c.toUpperCase())
                .trim();
        }
    };

    // ================================
    // 11. STEPPER COMPONENT
    // ================================

    PT.Stepper = {
        create(container, steps = [], currentStep = 0) {
            const el = typeof container === 'string' ? document.querySelector(container) : container;
            if (!el) return;

            el.innerHTML = `
                <div class="pt-stepper">
                    ${steps.map((step, index) => `
                        ${index > 0 ? `<div class="pt-step-connector ${index <= currentStep ? 'completed' : ''}"></div>` : ''}
                        <div class="pt-step ${index < currentStep ? 'completed' : index === currentStep ? 'active' : 'pending'}">
                            <div class="pt-step-indicator">
                                ${index < currentStep ? '<i class="bi bi-check"></i>' : index + 1}
                            </div>
                            <span class="pt-step-label">${step}</span>
                        </div>
                    `).join('')}
                </div>
            `;

            return {
                setStep: (newStep) => this.create(el, steps, newStep),
                next: () => this.create(el, steps, Math.min(currentStep + 1, steps.length - 1)),
                prev: () => this.create(el, steps, Math.max(currentStep - 1, 0))
            };
        }
    };

    // ================================
    // 12. COUNT UP ANIMATION
    // ================================

    PT.CountUp = {
        animate(element, endValue, options = {}) {
            const el = typeof element === 'string' ? document.querySelector(element) : element;
            if (!el) return;

            const {
                duration = 1500,
                startValue = 0,
                prefix = '',
                suffix = '',
                decimals = 0,
                separator = ','
            } = options;

            const startTime = performance.now();
            const diff = endValue - startValue;

            const format = (num) => {
                const fixed = num.toFixed(decimals);
                const parts = fixed.split('.');
                parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, separator);
                return prefix + parts.join('.') + suffix;
            };

            const step = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                
                // Easing function (ease-out)
                const eased = 1 - Math.pow(1 - progress, 3);
                const currentValue = startValue + (diff * eased);

                el.textContent = format(currentValue);

                if (progress < 1) {
                    requestAnimationFrame(step);
                }
            };

            requestAnimationFrame(step);
        },

        initAll(selector = '[data-countup]') {
            document.querySelectorAll(selector).forEach(el => {
                const value = parseFloat(el.dataset.countup) || 0;
                const prefix = el.dataset.prefix || '';
                const suffix = el.dataset.suffix || '';
                const decimals = parseInt(el.dataset.decimals) || 0;

                // Use IntersectionObserver to trigger on scroll
                const observer = new IntersectionObserver((entries) => {
                    entries.forEach(entry => {
                        if (entry.isIntersecting) {
                            this.animate(el, value, { prefix, suffix, decimals });
                            observer.unobserve(el);
                        }
                    });
                }, { threshold: 0.5 });

                observer.observe(el);
            });
        }
    };

    // ================================
    // 13. KEYBOARD SHORTCUTS
    // ================================

    PT.Shortcuts = {
        bindings: {},

        init() {
            document.addEventListener('keydown', (e) => {
                // Skip if in input/textarea
                if (['INPUT', 'TEXTAREA', 'SELECT'].includes(document.activeElement.tagName)) {
                    return;
                }

                const key = e.key.toLowerCase();
                const binding = this.bindings[key];

                if (binding && !e.metaKey && !e.ctrlKey && !e.altKey) {
                    e.preventDefault();
                    binding();
                }
            });

            // Register default shortcuts
            this.register('n', () => window.location.href = '/Employee/Create'); // New employee
            this.register('e', () => window.location.href = '/Employee'); // Employees
            this.register('d', () => PT.Theme.toggle()); // Toggle dark mode
            this.register('?', () => this.showHelp()); // Show shortcuts
        },

        register(key, callback) {
            this.bindings[key.toLowerCase()] = callback;
        },

        unregister(key) {
            delete this.bindings[key.toLowerCase()];
        },

        showHelp() {
            const shortcuts = [
                { key: 'Cmd/Ctrl + K', description: 'Open command palette' },
                { key: 'N', description: 'New employee' },
                { key: 'E', description: 'Go to employees' },
                { key: 'D', description: 'Toggle dark mode' },
                { key: '?', description: 'Show this help' },
            ];

            PT.Toast.info(
                shortcuts.map(s => `<strong>${s.key}</strong>: ${s.description}`).join('<br>'),
                'Keyboard Shortcuts'
            );
        }
    };

    // ================================
    // 14. AUTO INIT
    // ================================

    PT.init = function () {
        // Initialize theme
        PT.Theme.init();

        // Initialize command palette
        PT.CommandPalette.init();

        // Initialize keyboard shortcuts
        PT.Shortcuts.init();

        // Initialize scroll animations
        PT.ScrollAnimation.init();

        // Initialize count up animations
        PT.CountUp.initAll();

        // Log initialization
        console.log(`%c PayroTech Design System v${PT.version} initialized`, 
            'color: #2563eb; font-weight: bold; font-size: 12px;');
    };

    // Auto-init on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', PT.init);
    } else {
        PT.init();
    }

    // Export to window
    window.PT = PT;

})(window);
