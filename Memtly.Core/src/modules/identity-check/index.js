import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';

function init() {
    bindEventHandlers();

    const pageLoadEnabled = $('body').data('identity-check');
    if (pageLoadEnabled) {
        displayIdentityCheck(false);
    }
}

function bindEventHandlers() {
    bindChangeIdentityButton();
}

function bindChangeIdentityButton() {
    $(document).off('click', '.change-identity').on('click', '.change-identity', function (e) {
        preventDefaults(e);

        const elem = $(this);
        const name = elem.data('identity-name');
        const email = elem.data('identity-email');
        const emailRequired = elem.attr('data-identity-email') !== undefined;

        displayIdentityCheckChangePopup(name, email, emailRequired);
    });
}

export function displayIdentityCheck(required, callbackFn) {
    const elem = $('.change-identity');
    const emailRequired = elem.attr('data-identity-email') !== undefined;

    displayIdentityCheckPopup('', '', required, emailRequired, callbackFn);
}

export function displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn) {
    let buttons = [{
        Text: localization.translate('Identity_Check_Tell_Us'),
        Class: 'btn-primary-2',
        Callback: function () {
            name = $('#popup-modal-field-identity-name').val().trim();
            if (name !== undefined && name.length > 0) {
                email = emailRequired ? $('#popup-modal-field-identity-email').val().trim() : '';

                $.ajax({
                    url: '/Home/SetIdentity',
                    method: 'POST',
                    data: { name, email }
                })
                    .done(data => {
                        if (data == undefined || data.success == undefined) {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        } else if (data.success) {
                            $('.file-uploader-form').attr('data-identity-required', 'false');

                            if (callbackFn !== undefined && callbackFn !== null) {
                                callbackFn();
                            } else {
                                window.location.reload();
                            }
                        } else if (data.reason == 1) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        } else if (data.reason == 2) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Email'), localization.translate('Identity_Check_Invalid_Email_Msg'), null, () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        } else {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                            displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                        });
                    });
            } else {
                displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                    displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                });
            }
        }
    }];

    if (!nameRequired) {
        buttons.push({
            Text: localization.translate('Identity_Check_Stay_Anonymous'),
            Callback: function () {
                $.ajax({
                    url: '/Home/SetIdentity',
                    method: 'POST',
                    data: { name: 'Anonymous', email: '' }
                })
                    .done(data => {
                        if (data == undefined || data.success == undefined) {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        } else if (data.success) {
                            window.location.reload();
                        } else if (data.reason == 1) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        } else if (data.reason == 2) {
                            displayMessage(localization.translate('Identity_Check_Invalid_Email'), localization.translate('Identity_Check_Invalid_Email_Msg'), null, () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        } else {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                displayIdentityCheckPopup(name, email, nameRequired, emailRequired, callbackFn);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                            displayIdentityCheckPopup(name, email, nameRequired, callbackFn);
                        });
                    });
            }
        });
    }

    let identityCheckFields = [{
        Id: 'identity-name',
        Name: localization.translate('Identity_Check_Name'),
        Value: name,
        Hint: localization.translate('Identity_Check_Name_Hint'),
        Placeholder: localization.translate('Identity_Check_Name_Placeholder')
    }];

    if (emailRequired) {
        identityCheckFields.push({
            Id: 'identity-email',
            Name: localization.translate('Identity_Check_Email'),
            Value: email,
            Hint: localization.translate('Identity_Check_Email_Hint'),
            Placeholder: localization.translate('Identity_Check_Email_Placeholder')
        });
    }

    displayPopup({
        Title: localization.translate('Identity_Check'),
        Fields: identityCheckFields,
        Buttons: buttons
    });
}

function displayIdentityCheckChangePopup(name, email, emailRequired) {
    let fields = [{
        Id: 'identity-name',
        Name: localization.translate('Identity_Check_Name'),
        Value: name,
        Hint: localization.translate('Identity_Check_Name_Hint'),
        Placeholder: localization.translate('Identity_Check_Name_Placeholder')
    }];

    if (emailRequired) {
        fields.push({
            Id: 'identity-email',
            Name: localization.translate('Identity_Check_Email'),
            Value: email,
            Hint: localization.translate('Identity_Check_Email_Hint'),
            Placeholder: localization.translate('Identity_Check_Email_Placeholder')
        });
    }

    displayPopup({
        Title: localization.translate('Identity_Check_Change_Identity'),
        Fields: fields,
        Buttons: [{
            Text: localization.translate('Identity_Check_Change'),
            Class: 'btn-primary-2',
            Callback: function () {
                name = $('#popup-modal-field-identity-name').val().trim();
                email = $('#popup-modal-field-identity-email').length > 0 ? $('#popup-modal-field-identity-email').val().trim() : '';

                if (name !== undefined && name.length > 0) {
                    $.ajax({
                        url: '/Home/SetIdentity',
                        method: 'POST',
                        data: { name, email }
                    })
                        .done(data => {
                            if (data == undefined || data.success == undefined) {
                                displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                    displayIdentityCheckChangePopup(name, email, emailRequired);
                                });
                            } else if (data.success) {
                                window.location.reload();
                            } else if (data.reason == 1) {
                                displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                                    displayIdentityCheckChangePopup(name, email, emailRequired);
                                });
                            } else if (data.reason == 2) {
                                displayMessage(localization.translate('Identity_Check_Invalid_Email'), localization.translate('Identity_Check_Invalid_Email_Msg'), null, () => {
                                    displayIdentityCheckChangePopup(name, email, emailRequired);
                                });
                            } else {
                                displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                    displayIdentityCheckChangePopup(name, email, emailRequired);
                                });
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Identity_Check'), localization.translate('Identity_Check_Set_Failed'), [error], () => {
                                displayIdentityCheckChangePopup(name, email, emailRequired);
                            });
                        });
                } else {
                    displayMessage(localization.translate('Identity_Check_Invalid_Name'), localization.translate('Identity_Check_Invalid_Name_Msg'), null, () => {
                        displayIdentityCheckChangePopup(name, email, emailRequired);
                    });
                }
            }
        }, {
            Text: localization.translate('Cancel')
        }]
    });
}

export default init;