export function generateChecklistItem(identifier, type, text, hidden = false, padded = true) {
    let icon;
    switch (type.toLowerCase()) {
        case 'success':
            icon = `<i class="fa fa-square-check mx-2"></i>`;
            break;
        case 'error':
            icon = `<i class="fa fa-square-xmark mx-2"></i>`;
            break;
        case 'default':
            icon = `<i class="fa fa-regular fa-square mx-2"></i>`;
            break;
        default:
            icon = '';
            break;
    }

    return `
        <span class="${identifier} ${hidden === true ? 'visually-hidden' : ''} border rounded ${padded === false ? 'mx-0' : 'mx-3 mx-lg-5'} my-2 px-3 py-1 d-block checklist-${type}">
            ${icon}${text}
        </span>
    `;
}