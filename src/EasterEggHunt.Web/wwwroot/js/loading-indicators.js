/**
 * Loading Indicators JavaScript
 * Verwaltet Loading-Indikatoren für asynchrone Operationen
 */

(function() {
    'use strict';

    /**
     * Zeigt einen Loading-Spinner an
     * @param {string} containerId - ID des Containers, in dem der Spinner angezeigt wird
     * @param {object} options - Optionale Konfiguration
     * @param {string} options.text - Text neben dem Spinner
     * @param {string} options.size - Größe: 'small', 'normal', 'large'
     * @param {boolean} options.overlay - Overlay-Modus (blockiert den gesamten Bildschirm)
     */
    window.showLoadingSpinner = function(containerId, options = {}) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.warn(`Container mit ID '${containerId}' nicht gefunden`);
            return;
        }

        const text = options.text || 'Lädt...';
        const size = options.size || 'normal';
        const overlay = options.overlay || false;

        // Erstelle Spinner-HTML
        let spinnerHtml = '';
        if (overlay) {
            spinnerHtml = `
                <div class="loading-overlay" role="status" aria-live="polite" aria-label="${text}">
                    <div class="loading-spinner-container">
                        <div class="spinner-border text-primary ${size === 'small' ? 'spinner-border-sm' : size === 'large' ? 'spinner-border-lg' : ''}" role="status">
                            <span class="visually-hidden">${text}</span>
                        </div>
                        ${text ? `<p class="mt-3 text-white">${text}</p>` : ''}
                    </div>
                </div>
            `;
        } else {
            spinnerHtml = `
                <div class="d-inline-flex align-items-center" role="status" aria-live="polite" aria-label="${text}">
                    <div class="spinner-border ${size === 'small' ? 'spinner-border-sm' : size === 'large' ? 'spinner-border-lg' : ''} text-primary me-2" role="status">
                        <span class="visually-hidden">${text}</span>
                    </div>
                    ${text ? `<span>${text}</span>` : ''}
                </div>
            `;
        }

        // Speichere ursprünglichen Inhalt und füge Spinner hinzu
        if (!container.dataset.originalContent) {
            container.dataset.originalContent = container.innerHTML;
        }
        container.innerHTML = spinnerHtml;
        container.classList.add('loading-active');
    };

    /**
     * Versteckt den Loading-Spinner und stellt den ursprünglichen Inhalt wieder her
     * @param {string} containerId - ID des Containers
     */
    window.hideLoadingSpinner = function(containerId) {
        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        if (container.dataset.originalContent) {
            container.innerHTML = container.dataset.originalContent;
            delete container.dataset.originalContent;
        }
        container.classList.remove('loading-active');
    };

    /**
     * Deaktiviert einen Button während einer Operation
     * @param {HTMLElement|string} button - Button-Element oder Button-ID
     * @param {boolean} showSpinner - Ob ein Spinner im Button angezeigt werden soll
     */
    window.disableButton = function(button, showSpinner = true) {
        const buttonElement = typeof button === 'string' ? document.getElementById(button) : button;
        if (!buttonElement) {
            return;
        }

        // Speichere ursprünglichen Zustand
        if (!buttonElement.dataset.originalDisabled) {
            buttonElement.dataset.originalDisabled = buttonElement.disabled;
        }
        if (!buttonElement.dataset.originalHTML) {
            buttonElement.dataset.originalHTML = buttonElement.innerHTML;
        }

        buttonElement.disabled = true;
        buttonElement.classList.add('disabled');

        if (showSpinner) {
            const spinnerHtml = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            `;
            buttonElement.innerHTML = spinnerHtml + buttonElement.textContent.trim();
        }
    };

    /**
     * Aktiviert einen Button wieder
     * @param {HTMLElement|string} button - Button-Element oder Button-ID
     */
    window.enableButton = function(button) {
        const buttonElement = typeof button === 'string' ? document.getElementById(button) : button;
        if (!buttonElement) {
            return;
        }

        if (buttonElement.dataset.originalDisabled !== undefined) {
            buttonElement.disabled = buttonElement.dataset.originalDisabled === 'true';
        } else {
            buttonElement.disabled = false;
        }

        buttonElement.classList.remove('disabled');

        if (buttonElement.dataset.originalHTML) {
            buttonElement.innerHTML = buttonElement.dataset.originalHTML;
            delete buttonElement.dataset.originalHTML;
        }
    };

    /**
     * Wrapper für Formular-Submission mit Loading-Indikatoren
     * @param {HTMLFormElement|string} form - Formular-Element oder Form-ID
     * @param {Function} submitCallback - Callback-Funktion für Submit
     */
    window.submitFormWithLoading = async function(form, submitCallback) {
        const formElement = typeof form === 'string' ? document.getElementById(form) : form;
        if (!formElement) {
            return;
        }

        const submitButton = formElement.querySelector('button[type="submit"]');
        if (submitButton) {
            disableButton(submitButton, true);
        }

        try {
            await submitCallback();
        } finally {
            if (submitButton) {
                enableButton(submitButton);
            }
        }
    };

    /**
     * Initialisiert Loading-Indikatoren für alle Formulare mit data-loading="true"
     */
    function initializeFormLoading() {
        const forms = document.querySelectorAll('form[data-loading="true"]');
        forms.forEach(form => {
            form.addEventListener('submit', function(e) {
                const submitButton = form.querySelector('button[type="submit"]');
                if (submitButton) {
                    disableButton(submitButton, true);
                }
            });
        });
    }

    /**
     * Initialisiert Button-Loading für alle Buttons mit data-loading="true"
     */
    function initializeButtonLoading() {
        const buttons = document.querySelectorAll('button[data-loading="true"], a.btn[data-loading="true"]');
        buttons.forEach(button => {
            button.addEventListener('click', function(e) {
                if (button.tagName === 'A' && !button.hasAttribute('data-prevent-loading')) {
                    // Für Links: Zeige Spinner im Link
                    disableButton(button, true);
                }
            });
        });
    }

    // Initialisiere beim DOM-Ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initializeFormLoading();
            initializeButtonLoading();
        });
    } else {
        initializeFormLoading();
        initializeButtonLoading();
    }
})();









