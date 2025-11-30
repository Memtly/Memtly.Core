import { displayLoader, hideLoader } from '../loader';
import { displayPopup } from '../popups';

export class Localization {
    constructor() {
    }

    translate(key) {
        return key;
    }
}

function bindEventHandlers() {
    $(document).off('click', '.change-language').on('click', '.change-language', function (e) {
        preventDefaults(e);

        displayLoader(localization.translate('Loading'));

        $.ajax({
            type: "GET",
            url: '/Language',
            success: function (data) {
                hideLoader();

                if (data.supported && data.supported.length > 0) {
                    displayPopup({
                        Title: localization.translate('Language_Change'),
                        Fields: [{
                            Id: 'language-id',
                            Name: localization.translate('Language'),
                            Hint: localization.translate('Language_Name_Hint'),
                            Placeholder: 'English (en-GB)',
                            Type: 'select',
                            SelectOptions: data.supported
                        }],
                        Buttons: [{
                            Text: localization.translate('Switch'),
                            Class: 'btn-success',
                            Callback: function () {
                                $.ajax({
                                    type: "POST",
                                    url: '/Language/ChangeDisplayLanguage',
                                    data: { culture: $('#popup-modal-field-language-id').val().trim() },
                                    success: function (data) {
                                        if (data.success) {
                                            try {
                                                window.location = window.location.toString().replace(/([&]*culture\=.+?)(\&|$)/g, '');
                                            } catch {
                                                window.location.reload();
                                            }
                                        }
                                    }
                                });
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

bindEventHandlers();