import { displayMessage } from '@modules/message-box';
import { displayPopup, hidePopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import { getTimestamp } from '@utilities/datetime';
import { downloadBlob } from '@utilities/blobs';
import { getQueryParam } from '@utilities/urls';
import { default as initSettings } from '@pages/account/partials/settings';

function init() {
    initSettings();
    bindEventHandlers();
}

function bindEventHandlers() {
    bindSearchBox();
    bindGallerySettingsButton();
    bindOpenGalleryButton();
    bindDownloadGalleryButton();
    bindAddGalleryButton();
    bindEditGalleryButton();
    bindRelinkGalleryButton();
    bindWipeGalleryButton();
    bindWipeAllGalleriesButton();
    bindDeleteGalleryButton();
}

function bindSearchBox() {
    $(document).off('keyup', 'input#galleries-search-term').on('keyup', 'input#galleries-search-term', function (e) {
        const term = $('input#galleries-search-term').val();

        const url = new URL(window.location.href);
        url.searchParams.set('term', term);
        url.searchParams.set('page', '1');

        history.pushState({}, '', url);

        updateGalleryList();
    });
}

export function bindGallerySettingsButton() {
    $(document).off('click', '.btnGallerySettings').on('click', '.btnGallerySettings', function (e) {
        preventDefaults(e);

        let galleryId = $(this).data('gallery-id');

        $.ajax({
            type: 'GET',
            url: `/Account/Settings/${galleryId}`,
            success: function (data) {
                if (data !== undefined) {
                    data += `<div class="row"> \
                        <div class="col col-12"> \
                            <button class="btn btn-danger btn-wipe-gallery-settings w-100" data-gallery-id="${galleryId}">${localization.translate('Reset') }</button> \
                        </div> \
                    </div><hr/>`;

                    displayPopup({
                        Title: localization.translate('Gallery_Settings'),
                        CustomHtml: data,
                        Buttons: [{
                            Text: localization.translate('Save'),
                            Class: 'btn-primary-2',
                            Callback: function () {
                                let updatedFields = $('.setting-field[data-updated="true"]');
                                if (updatedFields.length > 0) {
                                    var settingsList = $.map(updatedFields, function (item) {
                                        let element = $(item);
                                        return { key: element.data('setting-name'), value: element.val() };
                                    });

                                    displayLoader(localization.translate('Loading'));
                                    $.ajax({
                                        url: '/Account/UpdateGallerySettings',
                                        method: 'PUT',
                                        data: { model: settingsList, galleryId: galleryId }
                                    })
                                        .done(data => {
                                            if (data.success === true) {
                                                displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Success'), null, function () {
                                                    window.location.reload();
                                                });
                                            } else if (data.message) {
                                                displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'), [data.message]);
                                            } else {
                                                displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'));
                                            }
                                        })
                                        .fail((xhr, error) => {
                                            displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'), [error]);
                                        });
                                } else {
                                    displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_No_Change'));
                                }
                            }
                        }, {
                            Text: localization.translate('Close')
                        }]
                    });
                } else {
                    displayMessage(localization.translate('Gallery_Settings'), localization.translate('Gallery_Settings_None'));
                }
            }
        });
    });

    $(document).off('click', '.btn-wipe-gallery-settings').on('click', '.btn-wipe-gallery-settings', function (e) {
        preventDefaults(e);

        let galleryId = $(this).data('gallery-id');

        hidePopup();
        displayLoader(localization.translate('Loading'));
        $.ajax({
            url: '/Account/ResetGallerySettings',
            method: 'DELETE',
            data: { galleryId: galleryId }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Success'), null, function () {
                        window.location.reload();
                    });
                } else if (data.message) {
                    displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Failed'), [error]);
            });
    });
}

function bindOpenGalleryButton() {
    $(document).off('click', '.btnOpenGallery').on('click', '.btnOpenGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        window.open($(this).data('url'), $(this).data('target'));
    });
}

function bindDownloadGalleryButton() {
    $(document).off('click', '.btnDownloadGallery').on('click', '.btnDownloadGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayLoader(localization.translate('Generating_Download'));

        let row = $(this).closest('tr');
        let id = row.data('gallery-id');
        let name = row.data('gallery-name');
        let secretKey = row.data('gallery-key');

        let nativeXhr;

        $.ajax({
            url: '/Gallery/DownloadGallery',
            method: 'POST',
            data: { Id: id, SecretKey: secretKey, FileFilter: [] },
            xhr: function () {
                nativeXhr = new XMLHttpRequest();
                return nativeXhr;
            },
            xhrFields: {
                responseType: 'blob'
            },
        })
            .done((data) => {
                hideLoader();
                downloadBlob(`${name}_${getTimestamp()}.zip`, 'application/zip', data, nativeXhr);
            })
            .fail(async function (jqXHR) {
                hideLoader();

                try {
                    if (nativeXhr.response instanceof Blob) {
                        const text = await nativeXhr.response.text();
                        const json = JSON.parse(text);

                        if (json.message !== undefined) {
                            displayMessage(
                                localization.translate('Download'),
                                localization.translate('Download_Failed'),
                                [json.message]
                            );
                        } else {
                            displayMessage(
                                localization.translate('Download'),
                                localization.translate('Download_Failed')
                            );
                        }
                    } else {
                        displayMessage(
                            localization.translate('Download'),
                            localization.translate('Download_Failed')
                        );
                    }
                } catch {
                    displayMessage(
                        localization.translate('Download'),
                        localization.translate('Download_Failed')
                    );
                }
            });
    });
}

function bindAddGalleryButton() {
    $(document).off('click', '.btnAddGallery').on('click', '.btnAddGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        $.ajax({
            url: '/Gallery/GenerateSecretKey',
            method: 'GET'
        })
            .done(secretKey => {
                displayAddGalleryPopup('', secretKey);
            });
    });
}

function bindEditGalleryButton() {
    $(document).off('click', '.btnEditGallery').on('click', '.btnEditGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        const row = $(this).closest('tr');
        const id = row.data('gallery-id');
        const identifier = row.data('gallery-identifier');
        const name = row.data('gallery-name');
        const secretKey = row.data('gallery-key');

        displayEditGalleryPopup(id, identifier, name, secretKey);
    });
}

function bindRelinkGalleryButton() {
    $(document).off('click', '.btnRelinkGallery').on('click', '.btnRelinkGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        const id = row.data('gallery-id');
        const username = row.data('gallery-username');

        displayRelinkGalleryPopup(id, username);
    });
}

function bindWipeGalleryButton() {
    $(document).off('click', '.btnWipeGallery').on('click', '.btnWipeGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('gallery-name');
        displayPopup({
            Title: localization.translate('Gallery_Wipe'),
            Message: `${name} - ${localization.translate('Gallery_Wipe_Message')}`,
            Fields: [{
                Id: 'gallery-id',
                Value: row.data('gallery-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Wipe'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-gallery-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/WipeGallery',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Gallery_Wipe_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindWipeAllGalleriesButton() {
    $(document).off('click', '.btnWipeAllGalleries').on('click', '.btnWipeAllGalleries', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        displayPopup({
            Title: localization.translate('Wipe_Data'),
            Message: localization.translate('Wipe_Data_Message'),
            Buttons: [{
                Text: localization.translate('Wipe'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    $.ajax({
                        url: '/Account/WipeAllGalleries',
                        method: 'DELETE'
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Wipe_Data'), localization.translate('Wipe_Data_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

function bindDeleteGalleryButton() {
    $(document).off('click', '.btnDeleteGallery').on('click', '.btnDeleteGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('gallery-name');
        displayPopup({
            Title: localization.translate('Gallery_Delete'),
            Message: localization.translate('Delete_Are_You_Sure'),
            Fields: [{
                Id: 'gallery-id',
                Value: row.data('gallery-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Delete'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-gallery-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/DeleteGallery',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Delete'), localization.translate('Gallery_Delete_Failed'), [error]);
                        });
                }
            }, {
                Text: localization.translate('Close')
            }]
        });
    });
}

export function updateGalleryList() {
    const term = getQueryParam('term') ?? '';
    const page = getQueryParam('page') ?? 1;
    const limit = getQueryParam('limit') ?? 50;
    
    $.ajax({
        type: 'GET',
        url: `/Account/GalleriesList?term=${term}&page=${page}&limit=${limit}`,
        success: function (data) {
            $('#galleries-list').html(data);
            bindEventHandlers();
        }
    });
}

function displayAddGalleryPopup(name, secretKey) {
    displayPopup({
        Title: localization.translate('Gallery_Create'),
        Fields: [{
            Id: 'gallery-name',
            Name: localization.translate('Gallery_Name'),
            Hint: localization.translate('Gallery_Name_Hint'),
            Value: name
        }, {
            Id: 'gallery-key',
            Name: localization.translate('Gallery_Secret_Key'),
            Hint: localization.translate('Gallery_Secret_Key_Hint'),
            Value: secretKey
        }],
        Buttons: [{
            Text: localization.translate('Create'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                name = $('#popup-modal-field-gallery-name').val();
                secretKey = $('#popup-modal-field-gallery-key').val();

                if (name == undefined || name.length == 0) {
                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Missing_Name'), null, () => {
                        displayAddGalleryPopup(name, secretKey);
                    });
                    return;
                }

                const regex = /^[a-zA-Z0-9\-\s-_~]+$/;
                if (!regex.test(name)) {
                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Invalid_Name'), null, () => {
                        displayAddGalleryPopup(name, secretKey);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/AddGallery',
                    method: 'POST',
                    data: { Id: 0, Name: name, SecretKey: secretKey }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), [data.message], () => {
                                displayAddGalleryPopup(name, secretKey);
                            });
                        } else {
                            displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), null, () => {
                                displayAddGalleryPopup(name, secretKey);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), [error], () => {
                            displayAddGalleryPopup(name, secretKey);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function displayEditGalleryPopup(id, identifier, name, secretKey) {
    displayPopup({
        Title: localization.translate('Gallery_Edit'),
        Fields: [{
            Id: 'gallery-id',
            Value: id,
            Type: 'hidden'
        }, {
            Id: 'gallery-identifier',
            Name: localization.translate('Gallery_Identifier'),
            Value: identifier,
            Disabled: true
        }, {
            Id: 'gallery-name',
            Name: localization.translate('Gallery_Name'),
            Value: name,
            Hint: localization.translate('Gallery_Name_Hint')
        }, {
            Id: 'gallery-key',
            Name: localization.translate('Gallery_Secret_Key'),
            Value: secretKey,
            Hint: localization.translate('Gallery_Secret_Key_Hint')
        }],
        Buttons: [{
            Text: localization.translate('Update'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                id = $('#popup-modal-field-gallery-id').val();
                name = $('#popup-modal-field-gallery-name').val();
                secretKey = $('#popup-modal-field-gallery-key').val();

                if (id == undefined || id.length == 0) {
                    displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Missing_Id'), null, () => {
                        displayEditGalleryPopup(id, identifier, name, secretKey);
                    });
                    return;
                }

                if (name == undefined || name.length == 0) {
                    displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Missing_Name'), null, () => {
                        displayEditGalleryPopup(id, identifier, name, secretKey);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/EditGallery',
                    method: 'PUT',
                    data: { Id: id, Name: name, SecretKey: secretKey }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), [data.message], () => {
                                displayEditGalleryPopup(id, identifier, name, secretKey);
                            });
                        } else {
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), null, () => {
                                displayEditGalleryPopup(id, identifier, name, secretKey);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), [error], () => {
                            displayEditGalleryPopup(id, identifier, name, secretKey);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function displayRelinkGalleryPopup(id, username) {
    displayPopup({
        Title: localization.translate('Gallery_Relink'),
        Fields: [{
            Id: 'gallery-id',
            Value: id,
            Type: 'hidden'
        }, {
            Id: 'gallery-username',
            Name: localization.translate('Username'),
            Value: username,
            Hint: localization.translate('Relink_Username_Hint')
        }],
        Buttons: [{
            Text: localization.translate('Update'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                id = $('#popup-modal-field-gallery-id').val();
                username = $('#popup-modal-field-gallery-username').val();

                if (id == undefined || id.length == 0) {
                    displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Missing_Id'), null, () => {
                        displayRelinkGalleryPopup(id, username);
                    });
                    return;
                }

                if (username == undefined || username.length == 0) {
                    displayMessage(localization.translate('Gallery_Relink'), localization.translate('Missing_Username'), null, () => {
                        displayRelinkGalleryPopup(id, username);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/RelinkGallery',
                    method: 'PUT',
                    data: { Id: id, OwnerName: username }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Failed'), [data.message], () => {
                                displayRelinkGalleryPopup(id, username);
                            });
                        } else {
                            displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Failed'), null, () => {
                                displayRelinkGalleryPopup(id, username);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Failed'), [error], () => {
                            displayRelinkGalleryPopup(id, username);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

export default init;