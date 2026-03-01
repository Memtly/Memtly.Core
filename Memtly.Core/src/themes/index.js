import { displayLoader, hideLoader } from '@modules/loader';
import { displayPopup } from '@modules/popups';

function init() {
    bindEventHandlers();
}

export function getSelectedTheme() {
    return document.body.dataset.theme !== undefined ? document.body.dataset.theme.toLowerCase() : 'default';
}

function changeSelectedTheme(theme) {
    $.ajax({
        type: "POST",
        url: '/Themes/ChangeDisplayTheme',
        data: { theme: theme || 'AutoDetect' },
        success: function (data) {
            if (data.success) {
                window.location.reload();
            }
        }
    });
}

function bindEventHandlers() {
    $(document).off('click', '.change-theme').on('click', '.change-theme', function (e) {
        preventDefaults(e);

        displayLoader(localization.translate('Loading'));

        $.ajax({
            type: "GET",
            url: '/Themes',
            success: function (data) {
                hideLoader();

                if (data.themes && data.themes.length > 0) {
                    displayPopup({
                        Title: localization.translate('Change_Theme'),
                        Fields: [{
                            Id: 'theme-id',
                            Name: localization.translate('Theme'),
                            Hint: localization.translate('Theme_Name_Hint'),
                            Placeholder: localization.translate('Light'),
                            Type: 'select',
                            SelectOptions: data.themes.map(theme => ({
                                key: theme.name,
                                value: theme.name,
                                selected: theme.selected
                            }))
                        }],
                        Buttons: [{
                            Text: localization.translate('Switch'),
                            Class: 'btn-primary-2',
                            Callback: function () {
                                let theme = $('#popup-modal-field-theme-id').val().trim();
                                changeSelectedTheme(theme);
                            }
                        }, {
                            Text: localization.translate('Cancel')
                        }]
                    });
                }
            }
        });
    });
}

export default init;