import { displayMessage } from '@modules/message-box';
import { displayLoader, hideLoader } from '@modules/loader';
import { initPasswordValidation } from '@validation/password-validation';

function init() {
    initPasswordValidation();
    bindEventHandlers();
}

function bindEventHandlers() {
    bindRegistrationForm();
}

function bindRegistrationForm() {
    $(document).off('submit', '#frmRegisterAccount').on('submit', '#frmRegisterAccount', function (e) {
        preventDefaults(e);

        var token = $('#frmRegisterAccount input[name=\'__RequestVerificationToken\']').val();

        var username = $('#frmRegisterAccount input.input-username').val();
        if (username === undefined || username.length < 5 || username.length > 50) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Username'));
            return;
        }

        var firstname = $('#frmRegisterAccount input.input-firstname').val();
        if (firstname === undefined || firstname.length < 1 || firstname.length > 50) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Firstname'));
            return;
        }

        var lastname = $('#frmRegisterAccount input.input-lastname').val();
        if (lastname === undefined || lastname.length < 1 || lastname.length > 50) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Lastname'));
            return;
        }

        var email = $('#frmRegisterAccount input.input-email').val();
        if (email === undefined || email.length === 0 || email.length > 100 || email.indexOf('@') === -1 || email.indexOf('.') === -1) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Email'));
            return;
        }

        var password = $('#frmRegisterAccount input.input-password').val();
        if (password === undefined || password.length < 8 || password.length > 100) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_Password'));
            return;
        }

        var cpassword = $('#frmRegisterAccount input.input-cpassword').val();
        if (cpassword === undefined || cpassword.length === 0 || cpassword !== password) {
            displayMessage(localization.translate('Registration'), localization.translate('Registration_Invalid_CPassword'));
            return;
        }

        displayLoader(localization.translate('Loading'));

        $.ajax({
            url: '/Account/Register',
            method: 'POST',
            data: { __RequestVerificationToken: token, Username: username, Firstname: firstname, Lastname: lastname, EmailAddress: email, Password: password, ConfirmPassword: cpassword }
        })
            .done(data => {
                hideLoader();

                if (data.success === true) {
                    if (data.validation === true) {
                        displayMessage(localization.translate('Registration'), localization.translate('Registration_Success_Validation'), null, function () {
                            window.location = `/Account`;
                        });
                    } else {
                        displayMessage(localization.translate('Registration'), localization.translate('Registration_Success'), null, function () {
                            displayLoader(localization.translate('Loading'));
                            $.ajax({
                                url: '/Account/Login',
                                method: 'POST',
                                data: { __RequestVerificationToken: token, Username: username, Password: password }
                            })
                                .done(data => {
                                    hideLoader();

                                    if (data.success === true) {
                                        window.location = `/Account`;
                                    } else if (data.message) {
                                        displayMessage(localization.translate('Registration'), localization.translate('Login_Failed'), [data.message]);
                                    } else {
                                        displayMessage(localization.translate('Registration'), localization.translate('Login_Invalid_Details'));
                                    }
                                })
                                .fail((xhr, error) => {
                                    hideLoader();
                                    displayMessage(localization.translate('Registration'), localization.translate('Login_Failed'), [error]);
                                });
                        });
                    }
                } else if (data.message) {
                    displayMessage(localization.translate('Registration'), localization.translate('Registration_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Registration'), localization.translate('Unexpected_Error_Occurred'));
                }
            })
            .fail((xhr, error) => {
                hideLoader();
                displayMessage(localization.translate('Registration'), localization.translate('Registration_Failed'), [error]);
            });
    });
}

export default init;