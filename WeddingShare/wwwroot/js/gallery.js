//function refreshGalleryPage(callback) {
//    $.ajax({
//        type: 'GET',
//        url: `${window.location.pathname}${window.location.search}&partial=true`,
//        success: function (data) {
//            $('#main-gallery').html(data);
//            if (callback !== undefined) {
//                callback();
//            }
//        }
//    });
//}

//(function () {
//    document.addEventListener('DOMContentLoaded', function () {

//        const triggerSelector = event => {
//            const identityReqiured = $('form.file-uploader-form').attr('data-identity-required') == 'true';
//            if (identityReqiured) {
//                displayIdentityCheck(true, function () {
//                    triggerSelector(event);
//                });
//                return;
//            }

//            const zone = event.target.closest('fieldset.upload_drop');
//            const input = $(zone.querySelector('input.upload-input'));
//            if (input.data('post-allow-camera') == true) {
//                displayPopup({
//                    Title: localization.translate('Upload'),
//                    Message: localization.translate('Upload_Method'),
//                    Buttons: [{
//                        Text: localization.translate('Gallery'),
//                        Class: "btn-primary",
//                        Callback: function () {
//                            input.attr('accept', 'image/*,video/*');
//                            input.attr('multiple', '');
//                            input.removeAttr('capture');
//                            input[0].click();
//                            hidePopup();
//                        }
//                    }, {
//                        Text: localization.translate('Camera'),
//                        Class: "btn-primary",
//                        Callback: function () {
//                            input.attr('accept', 'image/*');
//                            input.attr('capture', 'environment');
//                            input.removeAttr('multiple', '');
//                            input[0].click();
//                            hidePopup();
//                        }
//                    }, {
//                        Text: localization.translate('Close')
//                    }]
//                });
//            } else {
//                input.attr('accept', 'image/*,video/*');
//                input.attr('multiple', '');
//                input.removeAttr('capture');
//                input[0].click();
//            }
//        }

//        const highlight = (e) => {
//            $(e.target).closest('.upload_drop').addClass('highlight');
//        };

//        const unhighlight = (e) => {
//            $(e.target).closest('.upload_drop').removeClass('highlight');
//        };

//        const getInputAndGalleryRefs = element => {
//            const zone = element.closest('fieldset.upload_drop') || false;
//            const gallery = zone.querySelector('.upload_gallery') || false;
//            const input = zone.querySelector('input[type="file"]') || false;
//            return { input: input, gallery: gallery };
//        }

//        const handleDrop = event => {
//            const dataRefs = getInputAndGalleryRefs(event.target);
//            dataRefs.files = event.dataTransfer.files;

//            const identityReqiured = $('form.file-uploader-form').attr('data-identity-required') == 'true';
//            if (identityReqiured) {
//                displayIdentityCheck(true, function () {
//                    handleFiles(dataRefs);
//                });
//                return;
//            } else {
//                handleFiles(dataRefs);
//            }
//        }

//        const eventHandlers = zone => {
//            const dataRefs = getInputAndGalleryRefs(zone);

//            if (!dataRefs.input) return;

//            // Prevent default drag behaviors
//            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(event => {
//                zone.addEventListener(event, preventDefaults, false);
//                document.body.addEventListener(event, preventDefaults, false);
//            });

//            // Open file browser on drop area click
//            ['click', 'touch'].forEach(event => {
//                zone.addEventListener(event, triggerSelector, false);
//            });

//            // Highlighting drop area when item is dragged over it
//            ['dragenter', 'dragover'].forEach(event => {
//                zone.addEventListener(event, highlight, false);
//            });
//            ['dragleave', 'drop'].forEach(event => {
//                zone.addEventListener(event, unhighlight, false);
//            });

//            // Handle dropped files
//            zone.addEventListener('drop', handleDrop, false);

//            // Handle browse selected files
//            dataRefs.input.addEventListener('change', event => {
//                dataRefs.files = event.target.files;
//                handleFiles(dataRefs);
//            }, false);
//        }

//        // Initialise ALL dropzones
//        const dropZones = document.querySelectorAll('fieldset.upload_drop');
//        for (const zone of dropZones) {
//            eventHandlers(zone);
//        }

//        // No 'image/gif' or PDF or webp allowed here, but it's up to your use case.
//        // Double checks the input "accept" attribute
//        const isImageFile = file => file.type.toLowerCase().startsWith('image/');
//        const isVideoFile = file => file.type.toLowerCase().startsWith('video/');

//        // Based on: https://flaviocopes.com/how-to-upload-files-fetch/
//        const imageUpload = async dataRefs => {
//            const identityReqiured = $('form.file-uploader-form').attr('data-identity-required') == 'true';
//            if (identityReqiured) {
//                displayIdentityCheck(true, function () {
//                    dataRefs.input.click();
//                });
//                return;
//            }

//            // Multiple source routes, so double check validity
//            if (!dataRefs.files || !dataRefs.input) {
//                displayMessage(localization.translate('Upload'), localization.translate('Upload_No_Files_Detected'));
//                return;
//            }

//            const token = $('form.file-uploader-form input[name=\'__RequestVerificationToken\']').val();

//            const galleryId = dataRefs.input.getAttribute('data-post-gallery-id');
//            if (!galleryId) {
//                displayMessage(localization.translate('Upload'), localization.translate('Upload_Invalid_Gallery_Detected'));
//                return;
//            }

//            const url = dataRefs.input.getAttribute('data-post-url');
//            if (!url) {
//                displayMessage(localization.translate('Upload'), localization.translate('Upload_Invalid_Upload_Url'));
//                return;
//            }

//            const secretKey = dataRefs.input.getAttribute('data-post-key');

//            let uploadedCount = 0;
//            let requiresReview = true;
//            let errors = [];
//            let retries = 0;

//            function processFileUpload(i) {
//                if (i < dataRefs.files.length) {
//                    const formData = new FormData();
//                    formData.append('__RequestVerificationToken', token);
//                    formData.append('Id', galleryId);
//                    formData.append('SecretKey', secretKey);
//                    formData.append(dataRefs.files[i].name, dataRefs.files[i]);

//                    displayLoader(`${localization.translate('Upload_Progress', { index: i + 1, count: dataRefs.files.length })} <br/><br/><span id="file-upload-progress">0%</span>`);

//                    $.ajax({
//                        url: url,
//                        type: 'POST',
//                        data: formData,
//                        async: true,
//                        cache: false,
//                        contentType: false,
//                        dataType: 'json',
//                        processData: false,
//                        success: function (response) {
//                            if (response !== undefined && response.success === true) {
//                                requiresReview = response.requiresReview;
//                                uploadedCount++;
//                            } else if (response.errors !== undefined && response.errors.length > 0) {
//                                errors.push(response.errors);
//                            }

//                            processFileUpload(i + 1);
//                        },
//                        xhr: function () {
//                            var xhr = new window.XMLHttpRequest();

//                            xhr.upload.addEventListener("progress", function (evt) {
//                                if (evt.lengthComputable) {
//                                    var percentComplete = evt.loaded / evt.total;
//                                    percentComplete = parseInt(percentComplete * 100);

//                                    if ($('span#file-upload-progress').length > 0) {
//                                        $('span#file-upload-progress').text(`(${percentComplete}%)`);
//                                    }
//                                }
//                            }, false);

//                            xhr.upload.addEventListener("error", function (evt) {
//                                console.log(evt);
//                                if (retries < 5) {
//                                    setTimeout(function () {
//                                        retries++;
//                                        processFileUpload(i);
//                                    }, 2000);
//                                } else {
//                                    displayMessage(localization.translate('Upload'), localization.translate('Upload_Failed'), errors);
//                                }
//                            }, false);

//                            return xhr;
//                        },
//                    });
//                } else {
//                    hideLoader();

//                    if (uploadedCount <= 0) {
//                        displayMessage(localization.translate('Upload'), localization.translate('Upload_Failed'), errors);
//                    } else if (requiresReview) {
//                        displayMessage(localization.translate('Upload'), localization.translate('Upload_Success_Pending_Review', { count: uploadedCount }), errors);

//                        const formData = new FormData();
//                        formData.append('Id', galleryId);
//                        formData.append('SecretKey', secretKey);
//                        formData.append('Count', uploadedCount);

//                        setTimeout(function () {
//                            $.ajax({
//                                url: '/Gallery/UploadCompleted',
//                                type: 'POST',
//                                data: formData,
//                                async: true,
//                                cache: false,
//                                contentType: false,
//                                dataType: 'json',
//                                processData: false,
//                                success: function (response) {
//                                    dataRefs.input.value = '';

//                                    let counter = $('.review-counter');
//                                    if (counter.length > 0) {
//                                        counter.find('.review-counter-total').text(response.counters.total);
//                                        counter.find('.review-counter-approved').text(response.counters.approved);
//                                        counter.find('.review-counter-pending').text(response.counters.pending);
//                                    }
//                                },
//                                error: function (response) {
//                                    console.log(response);
//                                    displayMessage(localization.translate('Upload'), localization.translate('Upload_Failed'), errors);
//                                }
//                            });
//                        }, 500);
//                    } else {
//                        displayMessage(localization.translate('Upload'), localization.translate('Upload_Success', { count: uploadedCount }), errors, function () {
//                            refreshGalleryPage();
//                        });
//                    }
//                }
//            }

//            processFileUpload(0);
//        }

//        // Handle both selected and dropped files
//        const handleFiles = async dataRefs => {
//            let files = [...dataRefs.files];

//            // Remove unaccepted file types
//            files = files.filter(item => {
//                var isAllowed = isImageFile(item) || isVideoFile(item);
//                if (!isAllowed) {
//                    console.log(`File type '${item.type}' is not allowed. Filename: '${item.name}'`);
//                }

//                return isAllowed ? item : null;
//            });

//            if (!files.length) return;
//            dataRefs.files = files;

//            await imageUpload(dataRefs);
//        }

//        $(document).off('click', 'button.btnSaveQRCode').on('click', 'button.btnSaveQRCode', function (e) {
//            preventDefaults(e);

//            if ($(this).attr('disabled') == 'disabled') {
//                return;
//            }

//            let galleryName = $(this).data('gallery-name');

//            let link = document.createElement('a');
//            link.download = `${galleryName}-qrcode.png`;
//            link.href = $('#qrcode-download canvas')[0].toDataURL('image/png', 1.0).replace('image/png', 'image/octet-stream');
//            link.click();
//        });

//        $(document).off('click', 'i.btnDownloadGroup').on('click', 'i.btnDownloadGroup', function (e) {
//            preventDefaults(e);

//            if ($(this).attr('disabled') == 'disabled') {
//                return;
//            }

//            displayLoader(localization.translate('Generating_Download'));

//            let id = $(this).data('gallery-id');
//            let name = $(this).data('gallery-name');
//            let secretKey = $(this).data('gallery-key');
//            let group = $(this).data('group-name');

//            $.ajax({
//                url: '/Gallery/DownloadGallery',
//                method: 'POST',
//                data: { Id: id, SecretKey: secretKey, Group: group },
//                xhrFields: {
//                    responseType: 'blob'
//                },
//            })
//                .done((data, status, xhr) => {
//                    hideLoader();

//                    try {
//                        downloadBlob(`${name}_${getTimestamp()}.zip`, 'application/zip', data, xhr);
//                    } catch {
//                        displayMessage(localization.translate('Download'), localization.translate('Download_Failed'));
//                    }
//                })
//                .fail((xhr, error) => {
//                    hideLoader();
//                    displayMessage(localization.translate('Download'), localization.translate('Download_Failed'), [error]);
//                });
//        });

//        $(document).off('click', 'button.btnDownloadGallery').on('click', 'button.btnDownloadGallery', function (e) {
//            preventDefaults(e);

//            if ($(this).attr('disabled') == 'disabled') {
//                return;
//            }

//            displayLoader(localization.translate('Generating_Download'));

//            let id = $(this).data('gallery-id');
//            let name = $(this).data('gallery-name');
//            let secretKey = $(this).data('gallery-key');

//            $.ajax({
//                url: '/Gallery/DownloadGallery',
//                method: 'POST',
//                data: { Id: id, SecretKey: secretKey },
//                xhrFields: {
//                    responseType: 'blob'
//                },
//            })
//                .done((data, status, xhr) => {
//                    hideLoader();

//                    try {
//                        downloadBlob(`${name}_${getTimestamp()}.zip`, 'application/zip', data, xhr);
//                    } catch {
//                        displayMessage(localization.translate('Download'), localization.translate('Download_Failed'));
//                    }
//                })
//                .fail((xhr, error) => {
//                    hideLoader();
//                    displayMessage(localization.translate('Download'), localization.translate('Download_Failed'), [error]);
//                });
//        });

//        $(document).off('click', 'i.btnDeletePhoto').on('click', 'i.btnDeletePhoto', function (e) {
//            preventDefaults(e);

//            if ($(this).attr('disabled') == 'disabled') {
//                return;
//            }

//            var id = $(this).data('photo-id');
//            var name = $(this).data('photo-name');
//            var tile = $(this).closest('.image-tile');

//            displayPopup({
//                Title: localization.translate('Delete_Item'),
//                Message: localization.translate('Delete_Item_Message', { name }),
//                Fields: [{
//                    Id: 'photo-id',
//                    Value: id,
//                    Type: 'hidden'
//                }],
//                Buttons: [{
//                    Text: localization.translate('Delete'),
//                    Class: 'btn-danger',
//                    Callback: function () {
//                        displayLoader(localization.translate('Loading'));

//                        let id = $('#popup-modal-field-photo-id').val();
//                        if (id == undefined || id.length == 0) {
//                            displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Id_Missing'));
//                            return;
//                        }

//                        $.ajax({
//                            url: '/Account/DeletePhoto',
//                            method: 'DELETE',
//                            data: { id }
//                        })
//                            .done(data => {
//                                if (data.success === true) {
//                                    tile.remove();
//                                    displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Success'), null, function () {
//                                        refreshGalleryPage();
//                                    });
//                                } else if (data.message) {
//                                    displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Failed'), [data.message]);
//                                } else {
//                                    displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Failed'));
//                                }
//                            })
//                            .fail((xhr, error) => {
//                                displayMessage(localization.translate('Delete_Item'), localization.translate('Delete_Item_Failed'), [error]);
//                            });
//                    }
//                }, {
//                    Text: localization.translate('Close')
//                }]
//            });
//        });

//    });
//})();