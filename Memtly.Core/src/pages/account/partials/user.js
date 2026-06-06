import { displayMessage } from '@modules/message-box';
import { displayPopup } from '@modules/popups';
import { displayLoader } from '@modules/loader';
import { generatePasswordValidationContainer, initPasswordValidation } from '@validation/password-validation';
import { getQueryParam } from '@utilities/urls';

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindSearchBox();
    bindAddUserButton();
    bindEditUserButton();
    bindChangePasswordButton();
    bindMultiFactorWipeButton();
    bindActivateUserButton();
    bindFreezeUserButton();
    bindUnfreezeUserButton();
    bindDeleteUserButton();
}

function bindSearchBox() {
    $(document).off('keyup', 'input#users-search-term').on('keyup', 'input#users-search-term', function (e) {
        const term = $('input#users-search-term').val();

        const url = new URL(window.location.href);
        url.searchParams.set('term', term);
        url.searchParams.set('page', '1');

        history.pushState({}, '', url);

        updateUsersList();
    });
}

function bindAddUserButton() {
    $(document).off('click', '.btnAddUser').on('click', '.btnAddUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayAddUserPopup();
    });
}

function bindEditUserButton() {
    $(document).off('click', '.btnEditUser').on('click', '.btnEditUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        const row = $(this).closest('tr');
        const id = row.data('user-id');
        const username = row.data('user-username');
        const firstname = row.data('user-firstname');
        const lastname = row.data('user-lastname');
        const email = row.data('user-email');
        const level = row.data('user-level');
        const tier = row.data('user-tier');
        const canModifyAccessLevel = row.data('modify-level');

        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
    });
}

function bindChangePasswordButton() {
    $(document).off('click', '.btnChangePassword').on('click', '.btnChangePassword', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('User_Edit'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }, {
                Id: 'user-password',
                Name: localization.translate('User_Password'),
                Value: row.data('user-password'),
                Hint: localization.translate('User_Password_Hint'),
                Type: 'password'
            }, {
                Id: 'user-cpassword',
                Name: localization.translate('User_Confirm_Password'),
                Value: row.data('user-cpassword'),
                Hint: localization.translate('User_Confirm_Password_Hint'),
                Type: 'password',
                Class: 'confirm-password'
            }],
            FooterHtml: `${generatePasswordValidationContainer('input#popup-modal-field-user-password')}`,
            Buttons: [{
                Text: localization.translate('Update'),
                Class: 'btn-primary-2',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    let password = $('#popup-modal-field-user-password').val();
                    if (password == undefined || password.length < 8) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Password'));
                        return;
                    }

                    let cpassword = $('#popup-modal-field-user-cpassword').val();
                    if (password == undefined || password !== cpassword) {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_CPassword'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/ChangeUserPassword',
                        method: 'PUT',
                        data: { Id: id, Password: password, CPassword: cpassword }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        }, () => {
            initPasswordValidation();
        });
    });
}

function bindMultiFactorWipeButton() {
    $(document).off('click', '.btnWipe2FA').on('click', '.btnWipe2FA', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('2FA_Setup'),
            Message: localization.translate('2FA_Wipe_Message'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Wipe'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('2FA_Setup'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/MultiFactor/ResetForUser',
                        method: 'DELETE',
                        data: { userId: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Wipe'));
                            } else if (data.message) {
                                displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('2FA_Setup'), localization.translate('2FA_Set_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindActivateUserButton() {
    $(document).off('click', '.btnActivateUser').on('click', '.btnActivateUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Activate_User'),
            Message: `${localization.translate('Activate_User_Message')} '${row.data('user-username')}'`,
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Activate'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Activate_User'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/ActivateUser',
                        method: 'PUT',
                        data: { Id: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Successfully'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Activate_User'), localization.translate('Activate_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindFreezeUserButton() {
    $(document).off('click', '.btnFreezeUser').on('click', '.btnFreezeUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Freeze_User'),
            Message: `${localization.translate('Freeze_User_Message')} '${row.data('user-username')}'`,
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Freeze'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Freeze_User'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/FreezeUser',
                        method: 'PUT',
                        data: { Id: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Successfully'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Freeze_User'), localization.translate('Freeze_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindUnfreezeUserButton() {
    $(document).off('click', '.btnUnfreezeUser').on('click', '.btnUnfreezeUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('Unfreeze_User'),
            Message: `${localization.translate('Unfreeze_User_Message')} '${row.data('user-username')}'`,
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Unfreeze'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Unfreeze_User'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/UnfreezeUser',
                        method: 'PUT',
                        data: { Id: id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Successfully'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Unfreeze_User'), localization.translate('Unfreeze_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindDeleteUserButton() {
    $(document).off('click', '.btnDeleteUser').on('click', '.btnDeleteUser', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        displayPopup({
            Title: localization.translate('User_Delete'),
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'user-id',
                Value: row.data('user-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Delete'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-user-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('User_Delete'), localization.translate('User_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/DeleteUser',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateUsersList();
                                displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('User_Delete'), localization.translate('User_Delete_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier) {
    displayPopup({
        Title: localization.translate('User_Create'),
        Fields: [{
            Id: 'user-username',
            Name: localization.translate('User_Username'),
            Hint: localization.translate('User_Username_Hint'),
            Value: username
        },
        {
            Id: 'user-firstname',
            Name: localization.translate('User_Firstname'),
            Hint: localization.translate('User_Firstname_Hint'),
            Value: firstname
        },
        {
            Id: 'user-lastname',
            Name: localization.translate('User_Lastname'),
            Hint: localization.translate('User_Lastname_Hint'),
            Value: lastname
        },
        {
            Id: 'user-email',
            Name: localization.translate('User_Email'),
            Hint: localization.translate('User_Email_Hint'),
            Value: email
        },
        {
            Id: 'user-password',
            Name: localization.translate('User_Password'),
            Hint: localization.translate('User_Password_Hint'),
            Value: password,
            Type: "password"
        },
        {
            Id: 'user-cpassword',
            Name: localization.translate('User_Confirm_Password'),
            Hint: localization.translate('User_Confirm_Password_Hint'),
            Value: cpassword,
            Type: "password",
            Class: 'confirm-password'
        },
        {
            Id: 'user-level',
            Name: localization.translate('User_Level'),
            Hint: localization.translate('User_Level_Hint'),
            Type: 'select',
            SelectOptions: [
                {
                    key: '1',
                    selected: level === undefined || level === '' || isNaN(level) || parseInt(level) == 1,
                    value: 'Basic'
                },
                {
                    key: '3',
                    selected: parseInt(level) === 3,
                    value: 'Reviewer'
                },
                {
                    key: '4',
                    selected: parseInt(level) === 4,
                    value: 'Moderator'
                },
                {
                    key: '5',
                    selected: parseInt(level) === 5,
                    value: 'Admin'
                }
            ]
        },
        {
            Id: 'user-tier',
            Name: localization.translate('User_Tier'),
            Hint: localization.translate('User_Tier_Hint'),
            Type: 'select',
            SelectOptions: [
                {
                    key: '1',
                    selected: tier === undefined || tier === '' || isNaN(tier) || parseInt(tier) === 1,
                    value: 'None'
                },
                {
                    key: '2',
                    selected: parseInt(tier) === 2,
                    value: 'Basic'
                },
                {
                    key: '3',
                    selected: parseInt(tier) === 3,
                    value: 'Advanced'
                },
                {
                    key: '4',
                    selected: parseInt(tier) === 4,
                    value: 'Premium'
                }
            ]
        }],
        FooterHtml: `${generatePasswordValidationContainer('input#popup-modal-field-user-password')}`,
        Buttons: [{
            Text: localization.translate('Add'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                username = $('#popup-modal-field-user-username').val();
                firstname = $('#popup-modal-field-user-firstname').val();
                lastname = $('#popup-modal-field-user-lastname').val();
                email = $('#popup-modal-field-user-email').val();
                password = $('#popup-modal-field-user-password').val();
                cpassword = $('#popup-modal-field-user-cpassword').val();
                level = $('#popup-modal-field-user-level').val();
                tier = $('#popup-modal-field-user-tier').val();

                const usernameRegex = /^[a-zA-Z0-9\-\s-_~]+$/;
                if (username == undefined || username.length == 0 || !usernameRegex.test(username)) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Username'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                if (firstname == undefined || firstname.length < 1 || firstname.length > 50) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Firstname'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                if (lastname == undefined || lastname.length < 1 || lastname.length > 50) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Lastname'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                const emailRegex = /^((?!\.)[\w\-_.]*[^.])(@[\w\-_]+)(\.\w+(\.\w+)?[^.\W])$/;
                if (email != undefined && email.length > 0 && !emailRegex.test(email)) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Email'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                if (password == undefined || password.length < 8) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Password'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                if (password !== cpassword) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_CPassword'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                if (level == undefined || level.length == 0) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Level'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                if (tier == undefined || tier.length == 0) {
                    displayMessage(localization.translate('User_Create'), localization.translate('User_Invalid_Tier'), null, () => {
                        displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/AddUser',
                    method: 'POST',
                    data: { Username: username, Firstname: firstname, Lastname: lastname, Email: email, Password: password, CPassword: cpassword, Level: level, Tier: tier }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateUsersList();
                            displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Failed'), [data.message], () => {
                                displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                            });
                        } else {
                            displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Failed'), null, () => {
                                displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('User_Create'), localization.translate('User_Create_Failed'), [error], () => {
                            displayAddUserPopup(username, firstname, lastname, email, password, cpassword, level, tier);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    }, () => {
        initPasswordValidation();
    });
}

function displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel) {
    displayPopup({
        Title: localization.translate('User_Edit'),
        Fields: [{
            Id: 'user-id',
            Value: id,
            Type: 'hidden'
        }, {
            Id: 'user-username',
            Name: localization.translate('User_Username'),
            Value: username,
            Hint: localization.translate('User_Username_Hint'),
            Disabled: true
        }, {
            Id: 'user-firstname',
            Name: localization.translate('User_Firstname'),
            Value: firstname,
            Hint: localization.translate('User_Firstname_Hint')
        }, {
            Id: 'user-lastname',
            Name: localization.translate('User_Lastname'),
            Value: lastname,
            Hint: localization.translate('User_Lastname_Hint')
        }, {
            Id: 'user-email',
            Name: localization.translate('User_Email'),
            Value: email,
            Hint: localization.translate('User_Email_Hint')
        }, {
            Id: 'user-level',
            Name: localization.translate('User_Level'),
            Hint: localization.translate('User_Level_Hint'),
            Type: 'select',
            SelectOptions: canModifyAccessLevel ? [
                {
                    key: '1',
                    selected: level === undefined || level === '' || isNaN(level) || parseInt(level) == 1,
                    value: 'Basic'
                },
                {
                    key: '3',
                    selected: parseInt(level) === 3,
                    value: 'Reviewer'
                },
                {
                    key: '4',
                    selected: parseInt(level) === 4,
                    value: 'Moderator'
                },
                {
                    key: '5',
                    selected: parseInt(level) === 5,
                    value: 'Admin'
                }
            ] : []
        }, {
            Id: 'user-tier',
            Name: localization.translate('User_Tier'),
            Hint: localization.translate('User_Tier_Hint'),
            Type: 'select',
            SelectOptions: canModifyAccessLevel ? [
                {
                    key: '1',
                    selected: tier === undefined || tier === '' || isNaN(tier) || parseInt(tier) === 1,
                    value: 'None'
                },
                {
                    key: '2',
                    selected: parseInt(tier) === 2,
                    value: 'Basic'
                },
                {
                    key: '3',
                    selected: parseInt(tier) === 3,
                    value: 'Advanced'
                },
                {
                    key: '4',
                    selected: parseInt(tier) === 4,
                    value: 'Premium'
                }
            ] : []
        }],
        Buttons: [{
            Text: localization.translate('Update'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                username = $('#popup-modal-field-user-username').val();
                firstname = $('#popup-modal-field-user-firstname').val();
                lastname = $('#popup-modal-field-user-lastname').val();
                email = $('#popup-modal-field-user-email').val();
                level = $('#popup-modal-field-user-level').val();
                tier = $('#popup-modal-field-user-tier').val();

                if (id == undefined || id.length == 0) {
                    displayMessage(localization.translate('User_Edit'), localization.translate('User_Missing_Id'), null, () => {
                        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                    });
                    return;
                }

                if (firstname != undefined && (firstname.length < 1 || firstname.length > 50)) {
                    displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Firstname'), null, () => {
                        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                    });
                    return;
                }

                if (lastname != undefined && (lastname.length < 1 || lastname.length > 50)) {
                    displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Lastname'), null, () => {
                        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                    });
                    return;
                }

                const emailRegex = /^((?!\.)[\w\-_.]*[^.])(@[\w\-_]+)(\.\w+(\.\w+)?[^.\W])$/;
                if (email != undefined && email.length > 0 && !emailRegex.test(email)) {
                    displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Email'), null, () => {
                        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                    });
                    return;
                }

                if (canModifyAccessLevel && (level == undefined || level.length == 0)) {
                    displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Level'), null, () => {
                        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                    });
                    return;
                }

                if (canModifyAccessLevel && (tier == undefined || tier.length == 0)) {
                    displayMessage(localization.translate('User_Edit'), localization.translate('User_Invalid_Tier'), null, () => {
                        displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/EditUser',
                    method: 'PUT',
                    data: { Id: id, Firstname: firstname, Lastname: lastname, Email: email, Level: level, Tier: tier }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateUsersList();
                            displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [data.message], () => {
                                displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                            });
                        } else {
                            displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), null, () => {
                                displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('User_Edit'), localization.translate('User_Edit_Failed'), [error], () => {
                            displayEditUserPopup(id, username, firstname, lastname, email, level, tier, canModifyAccessLevel);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

export function updateUsersList() {
    const term = getQueryParam('term') ?? '';
    const page = getQueryParam('page') ?? 1;
    const limit = getQueryParam('limit') ?? 50;

    $.ajax({
        type: 'GET',
        url: `/Account/UsersList?term=${term}&page=${page}&limit=${limit}`,
        success: function (data) {
            $('#users-list').html(data);
            bindEventHandlers();

            const url = new URL(window.location.href);
            url.searchParams.set('term', term);

            history.pushState({}, '', url);
        }
    });
}

export default init;