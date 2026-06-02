import { displayMessage } from '@modules/message-box';
import { displayLoader } from '@modules/loader';
import initColorPickers from '@modules/color-picker';

let settingsSearchTimeout = null;

function init() {
    bindEventHandlers();
    initColorPickers();
}

function bindEventHandlers() {
    bindUpdateSettingActions();
    bindSaveSettingsButton();
    bindTestNotificationButtons();
    bindAdvancedSettingsButtons();
    bindSettingsSearchBox();
    bindBackgroundWorkerButtons();
}

function bindUpdateSettingActions() {
    $(document).off('change', 'input.setting-field,select.setting-field,textarea.setting-field').on('change', 'input.setting-field,select.setting-field,textarea.setting-field', function (e) {
        $(this).attr('data-updated', 'true');
    });
}

function bindSaveSettingsButton() {
    $(document).off('click', 'button#btnSaveSettings').on('click', 'button#btnSaveSettings', function (e) {
        let updatedFields = $('.setting-field[data-updated="true"]');
        if (updatedFields.length > 0) {
            var settingsList = $.map(updatedFields, function (item) {
                let element = $(item);
                return { key: element.data('setting-name'), value: element.val() };
            });

            displayLoader(localization.translate('Loading'));
            $.ajax({
                url: '/Account/UpdateSettings',
                method: 'PUT',
                data: { model: settingsList }
            })
                .done(data => {
                    if (data.success === true) {
                        displayMessage(localization.translate('Update_Settings'), localization.translate('Update_Settings_Success'), null, () => {
                            const hasThemeChanges = settingsList.some(item => item.key?.toLowerCase()?.startsWith("memtly:themes:"));
                            if (hasThemeChanges) {
                                window.location.reload();
                            }
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
    });
}

function bindTestNotificationButtons() {
    $(document).off('click', '.btn-send-smtp-test').on('click', '.btn-send-smtp-test', function (e) {
        displayLoader(localization.translate('Sending'));
        $.ajax({
            url: '/Notification/SendTestEmailNotification',
            method: 'POST',
            data: {
                recipients: $('input[data-setting-name="Memtly:Notifications:Smtp:Recipient"]').val(),
                host: $('input[data-setting-name="Memtly:Notifications:Smtp:Host"]').val(),
                port: $('input[data-setting-name="Memtly:Notifications:Smtp:Port"]').val(),
                username: $('input[data-setting-name="Memtly:Notifications:Smtp:Username"]').val(),
                password: $('input[data-setting-name="Memtly:Notifications:Smtp:Password"]').val(),
                from: $('input[data-setting-name="Memtly:Notifications:Smtp:From"]').val(),
                display_name: $('input[data-setting-name="Memtly:Notifications:Smtp:DisplayName"]').val(),
                enable_ssl: $('input[data-setting-name="Memtly:Notifications:Smtp:Use_SSL"]').val(),
            }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Success_Send_Test_Notification'));
                } else if (data.message) {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'), [data.message]);
                } else {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'), [error]);
            });
    });

    $(document).off('click', '.btn-send-ntfy-test').on('click', '.btn-send-ntfy-test', function (e) {
        displayLoader(localization.translate('Sending'));
        $.ajax({
            url: '/Notification/SendTestNtfyNotification',
            method: 'POST',
            data: {
                endpoint: $('input[data-setting-name="Memtly:Notifications:Ntfy:Endpoint"]').val(),
                token: $('input[data-setting-name="Memtly:Notifications:Ntfy:Token"]').val(),
                topic: $('input[data-setting-name="Memtly:Notifications:Ntfy:Topic"]').val(),
                priority: $('input[data-setting-name="Memtly:Notifications:Ntfy:Priority"]').val(),
            }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Success_Send_Test_Notification'));
                } else if (data.message) {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'), [data.message]);
                } else {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'), [error]);
            });
    });

    $(document).off('click', '.btn-send-gotify-test').on('click', '.btn-send-gotify-test', function (e) {
        displayLoader(localization.translate('Sending'));
        $.ajax({
            url: '/Notification/SendTestGotifyNotification',
            method: 'POST',
            data: {
                endpoint: $('input[data-setting-name="Memtly:Notifications:Gotify:Endpoint"]').val(),
                token: $('input[data-setting-name="Memtly:Notifications:Gotify:Token"]').val(),
                priority: $('input[data-setting-name="Memtly:Notifications:Ntfy:Priority"]').val(),
            }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Success_Send_Test_Notification'));
                } else if (data.message) {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'), [data.message]);
                } else {
                    displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Notification_Test'), localization.translate('Failed_Send_Test_Notification'), [error]);
            });
    });
}

function bindAdvancedSettingsButtons() {
    $(document).off('click', '.btnShowAdvancedSettings').on('click', '.btnShowAdvancedSettings', function (e) {
        showAdvancedSettings();
        searchSettings();
    });

    $(document).off('click', '.btnHideAdvancedSettings').on('click', '.btnHideAdvancedSettings', function (e) {
        hideAdvancedSettings();
        searchSettings();
    });
}

function bindSettingsSearchBox() {
    $(document).off('keyup', 'input#settings-search-term').on('keyup', 'input#settings-search-term', function (e) {
        searchSettings();
    });
}

function bindBackgroundWorkerButtons() {
    $(document).off('click', '.btnRequestDirectoryScan').on('click', '.btnRequestDirectoryScan', function (e) {
        displayLoader(localization.translate('Loading'));
        $.ajax({
            url: '/Account/RequestBackgroundWorker',
            method: 'POST',
            data: {
                type: 'DirectoryScanner'
            }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Success'));
                    $('.btnRequestDirectoryScan').addClass('disabled');
                } else if (data.message) {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'), [error]);
            });
    });

    $(document).off('click', '.btnRequestEmailReport').on('click', '.btnRequestEmailReport', function (e) {
        displayLoader(localization.translate('Loading'));
        $.ajax({
            url: '/Account/RequestBackgroundWorker',
            method: 'POST',
            data: {
                type: 'NotificationReport'
            }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Success'));
                    $('.btnRequestEmailReport').addClass('disabled');
                } else if (data.message) {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'), [error]);
            });
    });

    $(document).off('click', '.btnRequestCleanup').on('click', '.btnRequestCleanup', function (e) {
        displayLoader(localization.translate('Loading'));
        $.ajax({
            url: '/Account/RequestBackgroundWorker',
            method: 'POST',
            data: {
                type: 'Cleanup'
            }
        })
            .done(data => {
                if (data.success === true) {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Success'));
                    $('.btnRequestCleanup').addClass('disabled');
                } else if (data.message) {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'), [data.message]);
                } else {
                    displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Settings_BackgroundServices'), localization.translate('Request_Background_Worker_Failed'), [error]);
            });
    });
}

function showAdvancedSettings() {
    $('.setting-advanced').removeClass('d-none');
    $('.btnShowAdvancedSettings').addClass('d-none');
    $('.btnHideAdvancedSettings').removeClass('d-none');
}

function hideAdvancedSettings() {
    $('.setting-advanced').addClass('d-none');
    $('.btnShowAdvancedSettings').removeClass('d-none');
    $('.btnHideAdvancedSettings').addClass('d-none');
}

export function searchSettings() {
    clearTimeout(settingsSearchTimeout);
    settingsSearchTimeout = setTimeout(() => {
        let term = $('input#settings-search-term').val();

        $('#settings-accordion .accordion-item, #settings-accordion .accordion-item .setting-section, #settings-accordion .accordion-item .setting-section .setting-container').removeClass('d-none');
        $('#settings-accordion .accordion-item .setting-section').removeClass('no-border');

        if (term !== undefined && term.length > 0) {
            $('.setting-container').each(function () {
                const label = $(this).find('.setting-label').text();
                const hint = $(this).find('.setting-hint').text();

                if ((label === undefined && hint === undefined) || (label.toLowerCase().indexOf(term.toLowerCase()) === -1 && hint.toLowerCase().indexOf(term.toLowerCase()) === -1)) {
                    $(this).addClass('d-none');
                } else {
                    $(this).removeClass('d-none');
                }
            });
        }

        if ($('.btnShowAdvancedSettings:not(.d-none)').length > 0) {
            hideAdvancedSettings();
        }

        $('#settings-accordion .accordion-item').each(function () {
            const settingAccordion = $(this);

            let settingSections = settingAccordion.find('.setting-section:not(.d-none)');
            if (settingSections !== undefined && settingSections.length > 0) {
                settingSections.each(function () {
                    const settingSection = $(this);

                    let settingContainers = settingSection.find('.setting-container:not(.d-none)');
                    if (settingContainers !== undefined && settingContainers.length === 0) {
                        settingSection.addClass('d-none');
                    }
                });

                settingSections = settingAccordion.find('.setting-section:not(.d-none)');
                if (settingSections !== undefined && settingSections.length === 0) {
                    settingAccordion.addClass('d-none');
                }

                settingSections.last().addClass('no-border');
            }
        });

    }, 500);
}

export function updateSettings() {
    $.ajax({
        type: 'GET',
        url: `/Account/SettingsPartial`,
        success: function (data) {
            $('#settings-list').html(data);
            bindEventHandlers();
        }
    });
}

export default init;