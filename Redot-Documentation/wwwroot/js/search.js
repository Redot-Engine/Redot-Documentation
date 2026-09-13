let handler;
function updateViewport() {
    document.documentElement.style.setProperty('--search-viewport-height', `${window.visualViewport?.height ?? window.innerHeight}px`);
}
export function register(reference) {
    unregister();
    handler = event => {
        if (event.target.closest('.search-query') && ['ArrowDown','ArrowUp'].includes(event.key) && event.target.tagName === 'INPUT' && event.target.type === 'text') event.preventDefault();
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
            event.preventDefault();
            reference.invokeMethodAsync('Open');
        }
    };
    document.addEventListener('keydown', handler);
    updateViewport();
    window.visualViewport?.addEventListener('resize', updateViewport);
    window.addEventListener('resize', updateViewport);
}
export function unregister() { if (handler) document.removeEventListener('keydown', handler); handler = null; window.visualViewport?.removeEventListener('resize', updateViewport); window.removeEventListener('resize', updateViewport); document.documentElement.style.removeProperty('--search-viewport-height'); }
export function isMobile() { return window.matchMedia('(max-width: 600px)').matches; }
export function restoreFocus() { document.getElementById('documentation-search-trigger')?.focus(); }

export function scrollSelected(panel) {
    const region = panel.querySelector('.search-result-region');
    const selected = region?.querySelector('.search-result.selected');
    if (!selected) return;
    const outer = region.getBoundingClientRect(), inner = selected.getBoundingClientRect();
    if (inner.top < outer.top) region.scrollTop -= outer.top - inner.top;
    else if (inner.bottom > outer.bottom) region.scrollTop += inner.bottom - outer.bottom;
}
