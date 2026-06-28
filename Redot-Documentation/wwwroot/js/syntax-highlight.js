(() => {
    const MAX_RETRY_FRAMES = 30;
    const OBSERVER_CONFIG = {
        childList: true,
        subtree: true
    };
    let observer;

    const highlightCode = () => {
        if (!window.Prism || typeof window.Prism.highlightElement !== 'function') {
            return false;
        }

        const docContent = document.querySelector('.doc-viewer-content');
        if (!docContent) {
            return false;
        }

        const codeBlocks = docContent.querySelectorAll('pre > code, code[class*="language-"]');
        if (codeBlocks.length === 0) {
            return false;
        }

        codeBlocks.forEach(codeBlock => {
            const languageClass = Array.from(codeBlock.classList)
                .find(cssClass => cssClass.startsWith('language-'));

            if (!languageClass) {
                return;
            }

            const parentPre = codeBlock.parentElement;
            if (parentPre?.tagName === 'PRE') {
                parentPre.classList.add(languageClass);
            }

            window.Prism.highlightElement(codeBlock);
        });

        return true;
    };

    window.highlightCode = highlightCode;

    const scheduleHighlight = (retriesLeft = MAX_RETRY_FRAMES) => {
        requestAnimationFrame(() => {
            const didHighlight = highlightCode();

            if (!didHighlight && retriesLeft > 0) {
                scheduleHighlight(retriesLeft - 1);
            }
        });
    };

    const ensureObserver = () => {
        if (observer || typeof MutationObserver !== 'function') {
            return;
        }

        observer = new MutationObserver(() => {
            scheduleHighlight();
        });

        observer.observe(document.body, OBSERVER_CONFIG);
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            ensureObserver();
            scheduleHighlight();
        });
    } else {
        ensureObserver();
        scheduleHighlight();
    }

    document.addEventListener('enhancedload', () => {
        ensureObserver();
        scheduleHighlight();
    });
})();