function init() {
    bindCollectionItemSelection();
}

function bindCollectionItemSelection() {
    $(document).off('click', '.checklist-item').on('click', '.checklist-item', function (e) {
        preventDefaults(e);

        const elem = $(this);
        const container = elem.closest('.checklist-container');
        const selecttionType = getSelectionType(container);
        if (selecttionType === 'single') {
            container.find('.checklist-item').removeClass('selected');
        }

        elem.toggleClass('selected');
    });
}

function getSelectionType(container) {
    if (container !== undefined) {
        try {
            const selectionType = container.attr('data-selection-type')?.trim()?.toLowerCase();
            if (selectionType !== undefined && selectionType !== '') {
                return selectionType;
            }
        } catch { }
    }

    return 'multi';
}

export default init;