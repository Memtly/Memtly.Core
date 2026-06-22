import { displayMessage } from '@modules/message-box';
import { displayPopup, hidePopup } from '@modules/popups';
import { displayLoader, hideLoader } from '@modules/loader';
import { displayIdentityCheck } from '@modules/identity-check';
import { refreshGalleryPage } from '@pages/gallery/gallery';
import { bindCollectionItemSelection } from '@pages/account/partials/gallery';

class UploadBox {
    constructor() {
        this.maxRetries = 5;
        this.retryDelay = 2000;
    }

    init() {
        this.initializeDropZones();
        bindCollectionItemSelection();
    }

    isIdentityRequired() {
        return $('form.file-uploader-form').attr('data-identity-required') === 'true';
    }

    isCollection() {
        return $('form.file-uploader-form').attr('data-gallery-type') === 'collection';
    }

    triggerSelector(event) {
        if (this.isIdentityRequired()) {
            displayIdentityCheck(true, () => {
                this.triggerSelector(event);
            });
            return;
        }

        const zone = event.target.closest('fieldset.upload_drop');
        const input = $(zone.querySelector('input.upload-input'));

        const galleryId = input.attr('data-post-gallery-id');
        if (this.isCollection() && (galleryId === undefined || galleryId === '0')) {
            const collectionId = input.attr('data-post-collection-id');
            this.showGallerySelectorPopup(collectionId, (id) => {
                if (id !== undefined && id > 0) {
                    this.setGalleryId(input, id);
                }

                this.triggerSelector(event);
            });
        } else {
            if (input.data('post-allow-camera') === true) {
                this.showUploadMethodPopup(input);
            } else {
                this.setGalleryMode(input);
                input[0].click();
            }
        }
    }

    showUploadMethodPopup(input) {
        displayPopup({
            Title: localization.translate('Upload'),
            Message: localization.translate('Upload_Method'),
            Buttons: [
                {
                    Text: localization.translate('Gallery'),
                    Class: "btn-primary-2",
                    Callback: () => {
                        this.setGalleryMode(input);
                        input[0].click();
                        hidePopup();
                    }
                },
                {
                    Text: localization.translate('Camera'),
                    Class: "btn-primary-2",
                    Callback: () => {
                        this.setCameraMode(input);
                        input[0].click();
                        hidePopup();
                    }
                },
                {
                    Text: localization.translate('Close')
                }
            ]
        });
    }

    showGallerySelectorPopup(collectionId, callback) {
        $.ajax({
            url: '/Collection/Galleries',
            method: 'POST',
            data: {
                collectionId: collectionId
            }
        })
            .done(collection => {
                if (collection.items) {
                    displayPopup({
                        Title: localization.translate('Gallery_Selection'),
                        FooterHtml: `
                            <div class="row pb-3">
                                <div class="col-12">
                                    <div class="gallery-checklist-container" data-selection-type="single">
                                        ${collection.items.map(item => {
                                            return `<div class="gallery-checklist-item" data-gallery-id="${item.id}">${item.name}</div>`;
                                        }).join('\n')}
                                    </div>
                                </div>
                            </div>`,
                        Buttons: [{
                            Text: localization.translate('Select'),
                            Class: 'btn-primary-2',
                            Callback: () => {
                                const galleryId = $('.popup-modal .modal-body .gallery-checklist-item.selected').map((index, item) => { return $(item).data('gallery-id'); }).get()[0] ?? 0;
                                if (galleryId !== undefined && !isNaN(galleryId) && parseInt(galleryId) > 0) {
                                    callback(galleryId)
                                } else {
                                    displayMessage(localization.translate('Gallery_Selection'), localization.translate('Please_Select_Gallery'), null, () => {
                                        this.showGallerySelectorPopup(collectionId, callback);
                                    });
                                }
                            }
                        }, {
                            Text: localization.translate('Close')
                        }]
                    });
                } else {
                    displayMessage(localization.translate('Gallery_Selection'), localization.translate('Failed_Get_Gallery_List'));
                }
            })
            .fail((xhr, error) => {
                displayMessage(localization.translate('Gallery_Selection'), localization.translate('Failed_Get_Gallery_List'), [error]);
            });
    }

    setGalleryId(input, galleryId) {
        input.attr('data-post-gallery-id', galleryId);
    }

    setGalleryMode(input) {
        input.attr('accept', 'image/*,video/*');
        input.attr('multiple', '');
        input.removeAttr('capture');
    }

    setCameraMode(input) {
        input.attr('accept', 'image/*');
        input.attr('capture', 'environment');
        input.removeAttr('multiple');
    }

    highlight(e) {
        $(e.target).closest('.upload_drop').addClass('highlight');
    }

    unhighlight(e) {
        $(e.target).closest('.upload_drop').removeClass('highlight');
    }

    getInputAndGalleryRefs(element) {
        const zone = element.closest('fieldset.upload_drop') || false;
        const gallery = zone ? zone.querySelector('.upload_gallery') : false;
        const input = zone ? zone.querySelector('input[type="file"]') : false;
        return { input, gallery };
    }

    handleDrop(event) {
        const dataRefs = this.getInputAndGalleryRefs(event.target);
        dataRefs.files = event.dataTransfer.files;

        if (this.isIdentityRequired()) {
            displayIdentityCheck(true, () => {
                this.handleFiles(dataRefs);
            });
        } else {
            const galleryId = dataRefs.input.getAttribute('data-post-gallery-id');
            if (this.isCollection() && (galleryId === undefined || galleryId === '0')) {
                const collectionId = dataRefs.input.getAttribute('data-post-collection-id');
                this.showGallerySelectorPopup(collectionId, (id) => {
                    if (id !== undefined && id > 0) {
                        this.setGalleryId($(dataRefs.input), id);
                    }

                    this.handleFiles(dataRefs);
                });
            } else {
                this.handleFiles(dataRefs);
            }
        }
    }

    initializeDropZones() {
        const dropZones = document.querySelectorAll('fieldset.upload_drop');

        dropZones.forEach(zone => {
            this.setupEventHandlers(zone);
        });
    }

    setupEventHandlers(zone) {
        const dataRefs = this.getInputAndGalleryRefs(zone);

        if (!dataRefs.input) return;

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            zone.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        // Open file browser on drop area click
        ['click', 'touch'].forEach(eventName => {
            zone.addEventListener(eventName, (e) => this.triggerSelector(e), false);
        });

        // Highlighting drop area when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            zone.addEventListener(eventName, (e) => this.highlight(e), false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            zone.addEventListener(eventName, (e) => this.unhighlight(e), false);
        });

        // Handle dropped files
        zone.addEventListener('drop', (e) => this.handleDrop(e), false);

        // Handle browse selected files
        dataRefs.input.addEventListener('change', (event) => {
            dataRefs.files = event.target.files;
            this.handleFiles(dataRefs);
        }, false);
    }

    isImageFile(file) {
        return file.type.toLowerCase().startsWith('image/');
    }

    isVideoFile(file) {
        return file.type.toLowerCase().startsWith('video/');
    }

    async handleFiles(dataRefs) {
        let files = [...dataRefs.files];

        // Remove unaccepted file types
        files = files.filter(item => {
            const isAllowed = this.isImageFile(item) || this.isVideoFile(item);
            if (!isAllowed) {
                console.log(`File type '${item.type}' is not allowed. Filename: '${item.name}'`);
            }
            return isAllowed;
        });

        if (!files.length) return;

        dataRefs.files = files;
        await this.imageUpload(dataRefs);
    }

    async imageUpload(dataRefs) {
        if (this.isIdentityRequired()) {
            displayIdentityCheck(true, () => {
                dataRefs.input.click();
            });
            return;
        }

        // Multiple source routes, so double check validity
        if (!dataRefs.files || !dataRefs.input) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_No_Files_Detected')
            );
            return;
        }

        const token = $('form.file-uploader-form input[name=\'__RequestVerificationToken\']').val();
        const collectionId = dataRefs.input.getAttribute('data-post-collection-id');
        const galleryId = dataRefs.input.getAttribute('data-post-gallery-id');
        const url = dataRefs.input.getAttribute('data-post-url');
        const secretKey = dataRefs.input.getAttribute('data-post-key');

        if (!galleryId) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Invalid_Gallery_Detected')
            );
            return;
        }

        if (!url) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Invalid_Upload_Url')
            );
            return;
        }

        if (this.isCollection() && (galleryId === undefined || galleryId === '0')) {
            this.showGallerySelectorPopup(collectionId, (id) => {
                if (id !== undefined && id > 0) {
                    this.setGalleryId($(dataRefs.input), id);
                }

                dataRefs.input.click();
            });
            return;
        }

        let uploadedCount = 0;
        let requiresReview = true;
        let errors = [];

        const processFileUpload = (i, retries = 0) => {
            if (i < dataRefs.files.length) {
                const formData = new FormData();
                formData.append('__RequestVerificationToken', token);
                formData.append('CollectionId', collectionId);
                formData.append('GalleryId', galleryId);
                formData.append('SecretKey', secretKey);
                formData.append(dataRefs.files[i].name, dataRefs.files[i]);

                displayLoader(
                    `${localization.translate('Upload_Progress')} ${i + 1}/${dataRefs.files.length}...<br/><br/><span id="file-upload-progress">0%</span>`
                );

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    async: true,
                    cache: false,
                    contentType: false,
                    dataType: 'json',
                    processData: false,
                    success: (response) => {
                        if (response?.success === true) {
                            requiresReview = response.requiresReview;
                            uploadedCount++;
                        } else if (response?.errors?.length > 0) {
                            errors.push(response.errors);
                        }
                        processFileUpload(i + 1);
                    },
                    xhr: () => {
                        const xhr = new window.XMLHttpRequest();

                        xhr.upload.addEventListener("progress", (evt) => {
                            if (evt.lengthComputable) {
                                const percentComplete = Math.floor((evt.loaded / evt.total) * 100);
                                const progressElement = $('span#file-upload-progress');
                                if (progressElement.length > 0) {
                                    progressElement.text(`(${percentComplete}%)`);
                                }
                            }
                        }, false);

                        xhr.upload.addEventListener("error", (evt) => {
                            console.error(evt);
                            if (retries < this.maxRetries) {
                                setTimeout(() => {
                                    processFileUpload(i, retries + 1);
                                }, this.retryDelay);
                            } else {
                                displayMessage(
                                    localization.translate('Upload'),
                                    localization.translate('Upload_Failed'),
                                    errors
                                );
                            }
                        }, false);

                        return xhr;
                    },
                });
            } else {
                this.handleUploadComplete(uploadedCount, requiresReview, errors, collectionId, galleryId, secretKey, dataRefs);
            }
        };

        processFileUpload(0);
    }

    handleUploadComplete(uploadedCount, requiresReview, errors, collectionId, galleryId, secretKey, dataRefs) {
        hideLoader();

        if (this.isCollection()) {
            this.setGalleryId($(dataRefs.input), '0');
        }

        if (uploadedCount <= 0) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Failed'),
                errors
            );
        } else if (requiresReview) {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Success_Pending_Review'),
                errors
            );

            this.notifyUploadCompleted(collectionId, galleryId, secretKey, uploadedCount, dataRefs);
        } else {
            displayMessage(
                localization.translate('Upload'),
                localization.translate('Upload_Success'),
                errors,
                () => refreshGalleryPage()
            );
        }
    }

    notifyUploadCompleted(collectionId, galleryId, secretKey, uploadedCount, dataRefs) {
        const formData = new FormData();
        formData.append('CollectionId', collectionId);
        formData.append('GalleryId', galleryId);
        formData.append('SecretKey', secretKey);
        formData.append('Count', uploadedCount);

        setTimeout(() => {
            $.ajax({
                url: '/Gallery/UploadCompleted',
                type: 'POST',
                data: formData,
                async: true,
                cache: false,
                contentType: false,
                dataType: 'json',
                processData: false,
                success: (response) => {
                    dataRefs.input.value = '';

                    const counter = $('.review-counter');
                    if (counter.length > 0) {
                        counter.find('.review-counter-total').text(response.counters.total);
                        counter.find('.review-counter-approved').text(response.counters.approved);
                        counter.find('.review-counter-pending').text(response.counters.pending);
                    }
                },
                error: (response) => {
                    console.error(response);
                    displayMessage(
                        localization.translate('Upload'),
                        localization.translate('Upload_Failed'),
                        [response]
                    );
                }
            });
        }, 500);
    }
}

const galleryUpload = new UploadBox();

export default galleryUpload;