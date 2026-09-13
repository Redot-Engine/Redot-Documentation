(() => {
    // Highlight each DOM revision once. Prism inserts spans, so observing its
    // own changes without this cache would create an endless mutation loop.
    const highlighted = new WeakMap();
    let scheduled = false;

    const highlightCode = () => {
        if (!window.Prism?.highlightElement) return;
        document.querySelectorAll('.doc-viewer-content pre > code, .doc-viewer-content code[class*="language-"]').forEach(code => {
            const language = Array.from(code.classList).find(name => name.startsWith('language-'));
            if (!language) return;
            const revision = `${language}\n${code.textContent}`;
            if (highlighted.get(code) === revision) return;
            highlighted.set(code, revision);
            if (code.parentElement?.tagName === 'PRE') code.parentElement.classList.add(language);
            window.Prism.highlightElement(code);
        });
    };
    const schedule = () => {
        if (scheduled) return;
        scheduled = true;
        requestAnimationFrame(() => { scheduled = false; highlightCode(); });
    };
    window.highlightCode = highlightCode;
    const start = () => {
        new MutationObserver(schedule).observe(document.body, { childList: true, subtree: true, characterData: true });
        schedule();
    };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
    else start();
    document.addEventListener('enhancedload', schedule);
})();
