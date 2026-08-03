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
    bindCollectionSettingsButton();
    bindGallerySettingsButton();
    bindOpenGalleryButton();
    bindDownloadGalleryButton();
    bindAddGalleryButton();
    bindEditGalleryButton();
    bindAddCollectionButton();
    bindEditCollectionButton();
    bindRelinkGalleryButton();
    bindShareGalleryButton();
    bindLeaveShareButton();
    bindWipeGalleryButton();
    bindWipeAllGalleriesButton();
    bindDeleteGalleryButton();
    bindDeleteCollectionButton();
}

function bindSearchBox() {
    $(document).off('keyup', 'input#galleries-search-term').on('keyup', 'input#galleries-search-term', function (e) {
        const term = $('input#galleries-search-term').val();
        const type = $('select#galleries-type').val();

        const url = new URL(window.location.href);
        url.searchParams.set('term', term);
        url.searchParams.set('type', type);
        url.searchParams.set('page', '1');

        history.pushState({}, '', url);

        updateGalleryList();
    });

    $(document).off('change', 'select#galleries-type').on('change', 'select#galleries-type', function (e) {
        const term = $('input#galleries-search-term').val();
        const type = $('select#galleries-type').val();

        const url = new URL(window.location.href);
        url.searchParams.set('term', term);
        url.searchParams.set('type', type);
        url.searchParams.set('page', '1');

        history.pushState({}, '', url);

        updateGalleryList();
    });
}

export function bindCollectionSettingsButton() {
    $(document).off('click', '.btnCollectionSettings').on('click', '.btnCollectionSettings', function (e) {
        preventDefaults(e);

        let collectionId = $(this).data('gallery-id');

        $.ajax({
            type: 'POST',
            url: `/Account/Settings`,
            data: { galleryId: collectionId, type: 2 },
            success: function (data) {
                if (data !== undefined) {
                    data += `<div class="row"> \
                        <div class="col col-12"> \
                            <button class="btn btn-danger btn-wipe-gallery-settings w-100" data-gallery-id="${collectionId}">${localization.translate('Reset')}</button> \
                        </div> \
                    </div><hr/>`;

                    displayPopup({
                        Title: localization.translate('Collection_Settings'),
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
                                        url: '/Account/UpdateCollectionSettings',
                                        method: 'PUT',
                                        data: { model: settingsList, collectionId: collectionId }
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
                    displayMessage(localization.translate('Collection_Settings'), localization.translate('Settings_Failed_To_Get'));
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

export function bindGallerySettingsButton() {
    $(document).off('click', '.btnGallerySettings').on('click', '.btnGallerySettings', function (e) {
        preventDefaults(e);

        let galleryId = $(this).data('gallery-id');

        $.ajax({
            type: 'POST',
            url: `/Account/Settings`,
            data: { galleryId: galleryId, type: 1 },
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
                    displayMessage(localization.translate('Gallery_Settings'), localization.translate('Settings_Failed_To_Get'));
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
                displayAddGalleryPopup('', 1, secretKey);
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
        const type = row.data('gallery-type');
        const secretKey = row.data('gallery-key');

        displayEditGalleryPopup(id, identifier, name, type, secretKey);
    });
}

function bindAddCollectionButton() {
    $(document).off('click', '.btnAddCollection').on('click', '.btnAddCollection', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        $.ajax({
            url: '/Gallery/GenerateSecretKey',
            method: 'GET'
        })
            .done(secretKey => {
                $.ajax({
                    url: '/Collection/Items',
                    method: 'POST',
                    data: {
                        collectionId: null
                    }
                })
                    .done(collection => {
                        if (collection.items) {
                            displayAddCollectionPopup('', secretKey, collection.items);
                        } else {
                            displayMessage(localization.translate('Collection_Create'), localization.translate('Failed_Get_Gallery_List'));
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Collection_Create'), localization.translate('Failed_Get_Gallery_List'), [error]);
                    });
            });
    });
}

function bindEditCollectionButton() {
    $(document).off('click', '.btnEditCollection').on('click', '.btnEditCollection', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        const row = $(this).closest('tr');
        const id = row.data('gallery-id');
        const identifier = row.data('gallery-identifier');
        const name = row.data('gallery-name');
        const secretKey = row.data('gallery-key');

        $.ajax({
            url: '/Collection/Items',
            method: 'POST',
            data: {
                collectionId: id
            }
        })
            .done(collection => {
                if (collection.items) {
                    displayEditCollectionPopup(id, identifier, name, secretKey, collection.items);
                } else {
                    displayMessage(localization.translate('Collection_Edit'), localization.translate('Failed_Get_Gallery_List'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Collection_Edit'), localization.translate('Failed_Get_Gallery_List'), [error]);
            });
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

        displayRelinkGalleryPopup(id, username, '');
    });
}

function bindShareGalleryButton() {
    $(document).off('click', '.btnShareGallery').on('click', '.btnShareGallery', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        const id = row.data('gallery-id');
        const username = row.data('gallery-username');
        const excludeUserIds = $(this).data('exclude').split(',').map(item => parseInt(item));

        displayShareGalleryPopup(id, username, '', excludeUserIds);
    });
}

function bindLeaveShareButton() {
    $(document).off('click', '.btnLeaveShare').on('click', '.btnLeaveShare', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('gallery-name');
        displayPopup({
            Title: localization.translate('Gallery_Share'),
            Message: localization.translate('Leave_Are_You_Sure'),
            Fields: [{
                Id: 'gallery-id',
                Value: row.data('gallery-id'),
                Type: 'hidden'
            }],
            Buttons: [{
                Text: localization.translate('Leave'),
                Class: 'btn-danger',
                Callback: function () {
                    displayLoader(localization.translate('Loading'));

                    let id = $('#popup-modal-field-gallery-id').val();
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Share'), localization.translate('Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/LeaveShare',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Share'), localization.translate('Leave_Share_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Share'), localization.translate('Leave_Share_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Gallery_Share'), localization.translate('Leave_Share_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Share'), localization.translate('Leave_Share_Failed'), [error]);
                        });
                }
            }, {
                    Text: localization.translate('Close')
            }]
        });
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
                        displayMessage(localization.translate('Gallery_Wipe'), localization.translate('Missing_Id'));
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
                        displayMessage(localization.translate('Gallery_Delete'), localization.translate('Missing_Id'));
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

function bindDeleteCollectionButton() {
    $(document).off('click', '.btnDeleteCollection').on('click', '.btnDeleteCollection', function (e) {
        preventDefaults(e);

        if ($(this).attr('disabled') == 'disabled') {
            return;
        }

        let row = $(this).closest('tr');
        let name = row.data('gallery-name');
        displayPopup({
            Title: localization.translate('Collection_Delete'),
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
                        displayMessage(localization.translate('Collection_Delete'), localization.translate('Missing_Id'));
                        return;
                    }

                    $.ajax({
                        url: '/Account/DeleteCollection',
                        method: 'DELETE',
                        data: { id }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Collection_Delete'), localization.translate('Collection_Delete_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Collection_Delete'), localization.translate('Collection_Delete_Failed'), [data.message]);
                            } else {
                                displayMessage(localization.translate('Collection_Delete'), localization.translate('Collection_Delete_Failed'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Collection_Delete'), localization.translate('Collection_Delete_Failed'), [error]);
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
    const type = getQueryParam('type') ?? '';
    const page = getQueryParam('page') ?? 1;
    const limit = getQueryParam('limit') ?? 50;

    $.ajax({
        type: 'GET',
        url: `/Account/GalleriesList?term=${term}&type=${type}&page=${page}&limit=${limit}`,
        success: function (data) {
            $('#galleries-list').html(data);
            bindEventHandlers();
        }
    });

    $.ajax({
        type: 'GET',
        url: `/Account/RecentGalleriesList?term=${term}&type=${type}&page=${page}&limit=${limit}`,
        success: function (data) {
            $('#recent-galleries-list').html(data);
            bindEventHandlers();
        }
    });

    $.ajax({
        type: 'GET',
        url: `/Account/SharedGalleriesList?term=${term}&type=${type}&page=${page}&limit=${limit}`,
        success: function (data) {
            $('#shared-galleries-list').html(data);
            bindEventHandlers();
        }
    });
}

function displayAddGalleryPopup(name, type, secretKey) {
    displayPopup({
        Title: localization.translate('Gallery_Create'),
        Fields: [{
            Id: 'gallery-name',
            Name: localization.translate('Name'),
            Hint: localization.translate('Gallery_Name_Hint'),
            Value: name
        }, {
            Id: 'gallery-type',
            Name: localization.translate('Type'),
            Hint: localization.translate('Gallery_Type_Hint'),
            Type: 'select',
            SelectOptions: [{
                key: 1,
                value: localization.translate('Basic'),
                selected: parseInt(type) === 1
            }, {
                key: 3,
                value: localization.translate('Drop'),
                selected: parseInt(type) === 3
            }]
        }, {
            Id: 'gallery-key',
            Name: localization.translate('Secret_Key'),
            Hint: localization.translate('Secret_Key_Hint'),
            Value: secretKey
        }],
        Buttons: [{
            Text: localization.translate('Create'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                name = $('#popup-modal-field-gallery-name').val();
                type = $('#popup-modal-field-gallery-type').val();
                secretKey = $('#popup-modal-field-gallery-key').val();

                if (name == undefined || name.length == 0) {
                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Missing_Name'), null, () => {
                        displayAddGalleryPopup(name, type, secretKey);
                    });
                    return;
                }

                const regex = /^[a-zA-Z0-9\-\s-_~]+$/;
                if (!regex.test(name)) {
                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Invalid_Name'), null, () => {
                        displayAddGalleryPopup(name, type, secretKey);
                    });
                    return;
                }

                if (type == undefined || isNaN(type) || parseInt(type) < 0) {
                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Invalid_Gallery_Type'), null, () => {
                        displayAddGalleryPopup(name, type, secretKey);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/AddGallery',
                    method: 'POST',
                    data: { Id: 0, Name: name, Type: parseInt(type), SecretKey: secretKey }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), [data.message], () => {
                                displayAddGalleryPopup(name, type, secretKey);
                            });
                        } else {
                            displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), null, () => {
                                displayAddGalleryPopup(name, type, secretKey);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Gallery_Create'), localization.translate('Gallery_Create_Failed'), [error], () => {
                            displayAddGalleryPopup(name, type, secretKey);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function displayEditGalleryPopup(id, identifier, name, type, secretKey) {
    displayPopup({
        Title: localization.translate('Gallery_Edit'),
        Fields: [{
            Id: 'gallery-id',
            Value: id,
            Type: 'hidden'
        }, {
            Id: 'gallery-identifier',
            Name: localization.translate('Identifier'),
            Value: identifier,
            Disabled: true
        }, {
            Id: 'gallery-name',
            Name: localization.translate('Name'),
            Value: name,
            Hint: localization.translate('Gallery_Name_Hint')
        }, {
            Id: 'gallery-type',
            Name: localization.translate('Type'),
            Hint: localization.translate('Gallery_Type_Hint'),
            Type: 'select',
            SelectOptions: [{
                key: 1,
                value: localization.translate('Basic'),
                selected: parseInt(type) === 1
            }, {
                key: 3,
                value: localization.translate('Drop'),
                selected: parseInt(type) === 3
            }]
        }, {
            Id: 'gallery-key',
            Name: localization.translate('Secret_Key'),
            Value: secretKey,
            Hint: localization.translate('Secret_Key_Hint')
        }],
        Buttons: [{
            Text: localization.translate('Update'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                id = $('#popup-modal-field-gallery-id').val();
                name = $('#popup-modal-field-gallery-name').val();
                type = $('#popup-modal-field-gallery-type').val();
                secretKey = $('#popup-modal-field-gallery-key').val();

                if (id == undefined || id.length == 0) {
                    displayMessage(localization.translate('Gallery_Edit'), localization.translate('Missing_Id'), null, () => {
                        displayEditGalleryPopup(id, identifier, name, type, secretKey);
                    });
                    return;
                }

                if (name == undefined || name.length == 0) {
                    displayMessage(localization.translate('Gallery_Edit'), localization.translate('Missing_Name'), null, () => {
                        displayEditGalleryPopup(id, identifier, name, type, secretKey);
                    });
                    return;
                }

                if (type == undefined || isNaN(type) || parseInt(type) < 0) {
                    displayMessage(localization.translate('Gallery_Create'), localization.translate('Invalid_Gallery_Type'), null, () => {
                        displayAddGalleryPopup(name, type, secretKey);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/EditGallery',
                    method: 'PUT',
                    data: { Id: id, Name: name, Type: parseInt(type), SecretKey: secretKey }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), [data.message], () => {
                                displayEditGalleryPopup(id, identifier, name, type, secretKey);
                            });
                        } else {
                            displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), null, () => {
                                displayEditGalleryPopup(id, identifier, name, type, secretKey);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Gallery_Edit'), localization.translate('Gallery_Edit_Failed'), [error], () => {
                            displayEditGalleryPopup(id, identifier, name, type, secretKey);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function displayAddCollectionPopup(name, secretKey, collectionItems) {
    if (collectionItems === undefined || collectionItems.length < 2) {
        displayMessage(localization.translate('Collection_Create'), localization.translate('Collection_Not_Enough_Galleries'));
        return;
    }

    displayPopup({
        Title: localization.translate('Collection_Create'),
        Fields: [{
            Id: 'collection-name',
            Name: localization.translate('Name'),
            Hint: localization.translate('Collection_Name_Hint'),
            Value: name
        }, {
            Id: 'collection-key',
            Name: localization.translate('Secret_Key'),
            Hint: localization.translate('Secret_Key_Hint'),
            Value: secretKey
        }],
        FooterHtml: `
            <div class="row pb-3">
                <div class="col-12">
                    <label>${localization.translate('Galleries')}</label>
                    <div id="collection-checklist" class="checklist-container" data-selection-type="multi">
                        ${collectionItems.map(item => {
                            return `<div class="checklist-item${item.selected ? ' selected' : ''}" data-gallery-id="${item.id}">${item.name}</div>`;
                        }).join('\n')}
                    </div>
                </div>
            </div>`,
        Buttons: [{
            Text: localization.translate('Create'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                name = $('#popup-modal-field-collection-name').val();
                secretKey = $('#popup-modal-field-collection-key').val();

                if (name == undefined || name.length == 0) {
                    displayMessage(localization.translate('Collection_Create'), localization.translate('Missing_Name'), null, () => {
                        displayAddCollectionPopup(name, secretKey, collectionItems);
                    });
                    return;
                }

                const regex = /^[a-zA-Z0-9\-\s-_~]+$/;
                if (!regex.test(name)) {
                    displayMessage(localization.translate('Collection_Create'), localization.translate('Invalid_Name'), null, () => {
                        displayAddCollectionPopup(name, secretKey, collectionItems);
                    });
                    return;
                }

                const selectedGalleries = $('#collection-checklist .checklist-item.selected').map((index, item) => { return $(item).data('gallery-id'); }).get();
                if (selectedGalleries === undefined || selectedGalleries.length < 2) {
                    displayMessage(localization.translate('Collection_Create'), localization.translate('Collection_Not_Enough_Galleries'), null, () => {
                        displayAddCollectionPopup(name, secretKey, collectionItems);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/AddCollection',
                    method: 'POST',
                    data: { Id: 0, Name: name, SecretKey: secretKey, CollectionItems: selectedGalleries }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Collection_Create'), localization.translate('Collection_Create_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Collection_Create'), localization.translate('Collection_Create_Failed'), [data.message], () => {
                                displayAddCollectionPopup(name, secretKey, collectionItems);
                            });
                        } else {
                            displayMessage(localization.translate('Collection_Create'), localization.translate('Collection_Create_Failed'), null, () => {
                                displayAddCollectionPopup(name, secretKey, collectionItems);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Collection_Create'), localization.translate('Collection_Create_Failed'), [error], () => {
                            displayAddCollectionPopup(name, secretKey, collectionItems);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems) {
    if (collectionItems === undefined || collectionItems.length < 2) {
        displayMessage(localization.translate('Collection_Edit'), localization.translate('Collection_Not_Enough_Galleries'));
        return;
    }

    displayPopup({
        Title: localization.translate('Collection_Edit'),
        Fields: [{
            Id: 'collection-id',
            Value: id,
            Type: 'hidden'
        }, {
            Id: 'collection-identifier',
            Name: localization.translate('Identifier'),
            Value: identifier,
            Disabled: true
        }, {
            Id: 'collection-name',
            Name: localization.translate('Name'),
            Value: name,
            Hint: localization.translate('Collection_Name_Hint')
        }, {
            Id: 'collection-key',
            Name: localization.translate('Secret_Key'),
            Value: secretKey,
            Hint: localization.translate('Secret_Key_Hint')
        }],
        FooterHtml: `
            <div class="row pb-3">
                <div class="col-12">
                    <label>${localization.translate('Galleries')}</label>
                    <div id="collection-checklist" class="checklist-container" data-selection-type="multi">
                        ${collectionItems.map(item => {
                            return `<div class="checklist-item${item.selected ? ' selected' : ''}" data-gallery-id="${item.id}">${item.name}</div>`;
                        }).join('\n')}
                    </div>
                </div>
            </div>`,
        Buttons: [{
            Text: localization.translate('Update'),
            Class: 'btn-primary-2',
            Callback: function () {
                displayLoader(localization.translate('Loading'));

                id = $('#popup-modal-field-collection-id').val();
                name = $('#popup-modal-field-collection-name').val();
                secretKey = $('#popup-modal-field-collection-key').val();

                if (id == undefined || id.length == 0) {
                    displayMessage(localization.translate('Collection_Edit'), localization.translate('Missing_Id'), null, () => {
                        displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems);
                    });
                    return;
                }

                if (name == undefined || name.length == 0) {
                    displayMessage(localization.translate('Collection_Edit'), localization.translate('Missing_Name'), null, () => {
                        displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems);
                    });
                    return;
                }

                const selectedGalleries = $('#collection-checklist .checklist-item.selected').map((index, item) => { return $(item).data('gallery-id'); }).get();
                if (selectedGalleries === undefined || selectedGalleries.length < 2) {
                    displayMessage(localization.translate('Collection_Edit'), localization.translate('Collection_Not_Enough_Galleries'), null, () => {
                        displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems);
                    });
                    return;
                }

                $.ajax({
                    url: '/Account/EditCollection',
                    method: 'PUT',
                    data: { Id: id, Name: name, SecretKey: secretKey, CollectionItems: selectedGalleries }
                })
                    .done(data => {
                        if (data.success === true) {
                            updateGalleryList();
                            displayMessage(localization.translate('Collection_Edit'), localization.translate('Collection_Edit_Success'));
                        } else if (data.message) {
                            displayMessage(localization.translate('Collection_Edit'), localization.translate('Collection_Edit_Failed'), [data.message], () => {
                                displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems);
                            });
                        } else {
                            displayMessage(localization.translate('Collection_Edit'), localization.translate('Collection_Edit_Failed'), null, () => {
                                displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems);
                            });
                        }
                    })
                    .fail((xhr, error) => {
                        displayMessage(localization.translate('Collection_Edit'), localization.translate('Collection_Edit_Failed'), [error], () => {
                            displayEditCollectionPopup(id, identifier, name, secretKey, collectionItems);
                        });
                    });
            }
        }, {
            Text: localization.translate('Close')
        }]
    });
}

function displayRelinkGalleryPopup(id, username, term) {
    displayPopup({
        Title: localization.translate('Gallery_Relink'),
        Fields: [{
            Id: 'gallery-id',
            Value: id,
            Type: 'hidden'
        }, {
            Id: 'search-term',
            Name: localization.translate('SearchTerm_Label'),
            Value: term,
            Placeholder: username,
            Hint: localization.translate('SearchTerm_Help')
        }],
        FooterHtml: `
            <div class="row pb-3">
                <div class="col-12">
                    <div id="gallery-relink-checklist" class="checklist-container" data-selection-type="single"></div>
                </div>
            </div>`,
        Buttons: [{
            Text: localization.translate('Select'),
            Class: 'btn-primary-2',
            Callback: () => {
                term = $('#popup-modal-field-search-term').val();

                const userId = $('#gallery-relink-checklist .checklist-item.selected').map((index, item) => { return $(item).data('user-id'); }).get()[0] ?? 0;
                if (userId !== undefined && !isNaN(userId) && parseInt(userId) > 0) {
                    displayLoader(localization.translate('Loading'));

                    id = $('#popup-modal-field-gallery-id').val();
                    
                    if (id == undefined || id.length == 0) {
                        displayMessage(localization.translate('Gallery_Relink'), localization.translate('Missing_Id'), null, () => {
                            displayRelinkGalleryPopup(id, username, term);
                        });
                        return;
                    }

                    const newOwner = $('#gallery-relink-checklist .checklist-item.selected').map((index, item) => { return $(item).data('user-name'); }).get()[0] ?? '';
                    if (newOwner == undefined || newOwner.length == 0) {
                        displayMessage(localization.translate('Gallery_Relink'), localization.translate('Missing_Username'), null, () => {
                            displayRelinkGalleryPopup(id, username, term);
                        });
                        return;
                    }

                    $.ajax({
                        url: '/Account/RelinkGallery',
                        method: 'PUT',
                        data: { Id: id, OwnerName: newOwner }
                    })
                        .done(data => {
                            if (data.success === true) {
                                updateGalleryList();
                                displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Success'));
                            } else if (data.message) {
                                displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Failed'), [data.message], () => {
                                    displayRelinkGalleryPopup(id, username, term);
                                });
                            } else {
                                displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Failed'), null, () => {
                                    displayRelinkGalleryPopup(id, username, term);
                                });
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Relink'), localization.translate('Gallery_Relink_Failed'), [error], () => {
                                displayRelinkGalleryPopup(id, username, term);
                            });
                        });
                } else {
                    displayMessage(localization.translate('Gallery_Relink'), localization.translate('Please_Select_User'), null, () => {
                        displayRelinkGalleryPopup(id, username, term);
                    });
                }
            }
        }, {
            Text: localization.translate('Close')
            }]
    }, () => {
        let searchTimeout = null;

        function updateList(items) {
            if (items !== undefined && items.length > 0) {
                $('#gallery-relink-checklist').html(`${items.map(item => {
                    return `<div class="checklist-item${item.selected ? ' selected' : ''}" data-user-id="${item.id}" data-user-name="${item.name}">${item.name}</div>`;
                }).join('\n')}`);
            }
        }

        function search(term) {
            clearTimeout(searchTimeout);
            if (term !== undefined && term.length > 0) {
                searchTimeout = setTimeout(() => {
                    $.ajax({
                        url: '/User/Search',
                        method: 'POST',
                        data: {
                            term: term
                        }
                    })
                        .done(users => {
                            if (users.items) {
                                if (users.items.length > 0) {
                                    updateList(users.items.map(user => {
                                        return { id: user.id, name: user.username, selected: user.username.toLowerCase() === username.toLowerCase() };
                                    }));
                                } else {
                                    $('#gallery-relink-checklist').html(`<div class="checklist-message">${localization.translate('No_Users_Found_Matching')} '${term}'</div>`);
                                }
                            } else {
                                displayMessage(localization.translate('Gallery_Relink'), localization.translate('Failed_Get_User_List'));
                            }
                        })
                        .fail((xhr, error) => {
                            displayMessage(localization.translate('Gallery_Relink'), localization.translate('Failed_Get_User_List'), [error]);
                        });
                }, 500);
            } else {
                updateList([{ id: username, name: username, selected: true }]);
            }
        }

        search(term);

        $(document).off('keyup', '#popup-modal-field-search-term').on('keyup', '#popup-modal-field-search-term', (e) => {
            search($(e.currentTarget).val());
        });
    });
}

function displayShareGalleryPopup(id, username, term, excludeUserIds) {
    $.ajax({
        url: '/Gallery/Shares',
        method: 'POST',
        data: {
            galleryId: id
        }
    })
        .done(shares => {
            displayPopup({
                Title: localization.translate('Gallery_Share'),
                Fields: [{
                    Id: 'gallery-id',
                    Value: id,
                    Type: 'hidden'
                }, {
                    Id: 'search-term',
                    Name: localization.translate('SearchTerm_Label'),
                    Value: term,
                    Placeholder: username,
                    Hint: localization.translate('SearchTerm_Help')
                }],
                FooterHtml: `
                    <div class="row pb-3 d-none">
                        <div class="col-12">
                            <div id="gallery-share-search-checklist" class="checklist-container checklist-container-xs" data-selection-type="multi"></div>
                        </div>
                    </div>
                    <div class="row pb-3">
                        <label>${localization.translate('Users_With_Access') }</labl>
                        <div class="col-12">
                            <div id="gallery-share-checklist" class="checklist-container" data-selection-type="read-only"></div>
                        </div>
                    </div>`,
                Buttons: [{
                    Text: localization.translate('Update'),
                    Class: 'btn-primary-2',
                    Callback: () => {
                        displayLoader(localization.translate('Loading'));

                        id = $('#popup-modal-field-gallery-id').val();
                        term = $('#popup-modal-field-search-term').val();

                        if (id == undefined || id.length == 0) {
                            displayMessage(localization.translate('Gallery_Share'), localization.translate('Missing_Id'), null, () => {
                                displayShareGalleryPopup(id, username, term, excludeUserIds);
                            });
                            return;
                        }

                        const users = $('#gallery-share-checklist .checklist-item').map((index, item) => {
                            const elem = $(item);
                            return { GalleryId: id, UserId: elem.data('user-id'), UserName: elem.data('user-name') }
                        }).get();

                        if (users == undefined) {
                            displayMessage(localization.translate('Gallery_Share'), localization.translate('Missing_Username'), null, () => {
                                displayShareGalleryPopup(id, username, term, excludeUserIds);
                            });
                            return;
                        }

                        $.ajax({
                            url: '/Account/ShareGallery',
                            method: 'PUT',
                            data: { GalleryId: id, Users: users }
                        })
                            .done(data => {
                                if (data.success === true) {
                                    if (data.added > 0 && data.removed == 0) {
                                        displayMessage(localization.translate('Gallery_Share'), localization.translate('Gallery_Share_Success'));
                                    } else if (data.added == 0 && data.removed > 0) {
                                        displayMessage(localization.translate('Gallery_Share'), localization.translate('Gallery_UnShare_Success'));
                                    } else {
                                        displayMessage(localization.translate('Gallery_Share'), localization.translate('Gallery_Share_Update_Success'));
                                    }
                                } else if (data.message) {
                                    displayMessage(localization.translate('Gallery_Share'), localization.translate('Gallery_Share_Failed'), [data.message], () => {
                                        displayShareGalleryPopup(id, username, term, excludeUserIds);
                                    });
                                } else {
                                    displayMessage(localization.translate('Gallery_Share'), localization.translate('Gallery_Share_Failed'), null, () => {
                                        displayShareGalleryPopup(id, username, term, excludeUserIds);
                                    });
                                }
                            })
                            .fail((xhr, error) => {
                                displayMessage(localization.translate('Gallery_Share'), localization.translate('Gallery_Share_Failed'), [error], () => {
                                    displayShareGalleryPopup(id, username, term, excludeUserIds);
                                });
                            });
                    }
                }, {
                    Text: localization.translate('Close')
                    }]
            }, () => {
                let searchTimeout = null;

                function updateSearchList(items, term = '') {
                    if (items !== undefined && items.length > 0) {
                        $('#gallery-share-search-checklist').html(`${items.filter(item => !item.selected).map(item => {
                            return `<div class="checklist-item${item.selected ? ' selected' : ''}" data-user-id="${item.id}" data-user-name="${item.name}">${item.name}</div>`;
                        }).join('\n')}`);
                    } else if (term != undefined && term.length > 0) {
                        $('#gallery-share-search-checklist').html(`<div class="checklist-message">${localization.translate('No_Users_Found_Matching')} '${term}'</div>`);
                    }
                }

                function updateShareList(items) {
                    const results = items?.filter(item => item.selected) ?? [];
                    if (results !== undefined) {
                        $('#gallery-share-checklist').html(`${results.map(item => {
                            return `<div class="checklist-item" data-user-id="${item.id}" data-user-name="${item.name}">${item.name}</div>`;
                        }).join('\n')}`);
                    }
                }

                function search(term, delay = 500) {
                    clearTimeout(searchTimeout);

                    term = term?.trim();

                    if (term !== undefined && term.length > 0) {
                        searchTimeout = setTimeout(() => {
                            $.ajax({
                                url: '/User/Search',
                                method: 'POST',
                                data: {
                                    term: term,
                                    excludeUserIds: [...new Set(excludeUserIds.concat(shares.items.map(item => parseInt(item.id))))].join(',')
                                }
                            })
                                .done(users => {
                                    if (users.items) {
                                        const elem = $('#gallery-share-search-checklist').closest('.row');
                                        if (elem.hasClass('d-none')) {
                                            elem.removeClass('d-none').hide();
                                        }

                                        updateSearchList(users?.items?.map(user => {
                                            return { id: user.id, name: user.username, selected: shares.items.some(share => share.id == user.id && share.selected) };
                                        }), term);

                                        elem.slideDown(200);
                                    } else {
                                        displayMessage(localization.translate('Gallery_Share'), localization.translate('Failed_Get_User_List'));
                                    }
                                })
                                .fail((xhr, error) => {
                                    displayMessage(localization.translate('Gallery_Share'), localization.translate('Failed_Get_User_List'), [error]);
                                });
                        }, delay);
                    } else {
                        updateSearchList([], term);

                        const elem = $('#gallery-share-search-checklist').closest('.row');
                        elem.slideUp(200);
                    }
                }

                updateShareList(shares.items);

                $(document).off('keyup', '#popup-modal-field-search-term').on('keyup', '#popup-modal-field-search-term', (e) => {
                    search($(e.currentTarget).val());
                });

                $(document).off('click', '#gallery-share-search-checklist .checklist-item').on('click', '#gallery-share-search-checklist .checklist-item', (e) => {
                    const id = $(e.currentTarget).data('user-id');
                    const name = $(e.currentTarget).data('user-name');
                    const exists = shares.items.some(share => share.id == id);

                    if (exists) {
                        shares.items = shares.items.filter(share => share.id !== id);
                    } else {
                        shares.items.push({ id, name, selected: true });
                    }

                    search($('#popup-modal-field-search-term').val(), 0);
                    updateShareList(shares.items);
                });

                $(document).off('click', '#gallery-share-checklist .checklist-item').on('click', '#gallery-share-checklist .checklist-item', (e) => {
                    const id = $(e.currentTarget).data('user-id');
                    const name = $(e.currentTarget).data('user-name');
                    
                    shares.items = shares.items.filter(share => share.id !== id);

                    search($('#popup-modal-field-search-term').val(), 0);
                    updateShareList(shares.items);
                });
            });
        })
        .fail((xhr, error) => {
            displayMessage(localization.translate('Gallery_Share'), localization.translate('Failed_Get_Share_List'), [error]);
        });
}

export default init;