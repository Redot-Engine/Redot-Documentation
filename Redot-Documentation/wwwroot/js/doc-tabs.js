(() => {
    const owned = (container, selector) => Array.from(container.querySelectorAll(selector))
        .filter(element => element.closest('.doc-tabs') === container);

    function activateTab(container, selected) {
        owned(container, '.doc-tab-button').forEach(button => {
            const active = button === selected;
            button.classList.toggle('active', active);
            button.setAttribute('aria-selected', String(active));
            button.tabIndex = active ? 0 : -1;
        });
        owned(container, '.doc-tab-panel').forEach(panel => {
            panel.classList.toggle('active', `#${panel.id}` === selected.dataset.tabTarget);
        });
    }

    document.addEventListener('click', event => {
        const button = event.target.closest('.doc-tab-button');
        const container = button?.closest('.doc-tabs');
        if (container) activateTab(container, button);
    });

    document.addEventListener('keydown', event => {
        const button = event.target.closest('.doc-tab-button');
        const container = button?.closest('.doc-tabs');
        if (!container || !['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
        const buttons = owned(container, '.doc-tab-button');
        let index = buttons.indexOf(button);
        if (event.key === 'Home') index = 0;
        else if (event.key === 'End') index = buttons.length - 1;
        else index = (index + (event.key === 'ArrowRight' ? 1 : -1) + buttons.length) % buttons.length;
        event.preventDefault();
        activateTab(container, buttons[index]);
        buttons[index].focus();
    });
})();
