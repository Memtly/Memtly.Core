import './media-viewer.css';
import { displayMessage } from '@modules/message-box';
import { displayLoader, hideLoader } from '@modules/loader';
import { loadGalleryPage } from '@pages/gallery/gallery';

class MediaViewer {
    constructor() {
        this.playButtonTimeout = null;
        this.resizePopupTimeout = null;
        this.touchStartPosX = null;
        this.touchStartPosY = null;
        this.lastSelected = null;
        this.page = 1;
        this.maxScroll = 0;
        this.maxCharacterCount = 2000;
    }

    init() {
        clearTimeout(this.playButtonTimeout);
        this.playButtonTimeout = setTimeout(() => {
            this.repositionPlayButton();
        }, 200);

        this.setItemIndexes();
        this.setMultiSelectBtnStates();
        this.bindEventHandlers();
    }

    bindEventHandlers() {
        this.bindOpenPopup();
        this.bindClosePopup();
        this.bindMultiSelectButtons();
        this.bindRightClick();
        this.bindPopupEventHandlers();
        this.bindPageEndEvent();
    }

    bindPopupEventHandlers() {
        this.bindCommentButtons();
        this.bindDownloadButton();
        this.bindSwipe();
        this.bindArrowKeys();
        this.bindLikeButton();
    }

    bindOpenPopup() {
        $(document).off('click', '.media-viewer-item').on('click', '.media-viewer-item', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const elem = $(e.currentTarget);
            const checkbox = elem.find('.btn-multi-select');

            if (e.ctrlKey) {
                this.toggleMultiSelectOption(checkbox);
            } else if (e.shiftKey) {
                this.shiftMultiSelectOption(checkbox);
            } else {
                this.openMediaViewer(elem);
            }
        });
    }

    bindLoadEvent() {
        $('.media-viewer-image').on('load', (e) => {
            const element = $(e.currentTarget).closest('.media-viewer');
            const type = element.data('type');
            const source = element.data('source');
            this.initMediaViewImage(type, source);
        });
    }

    bindRightClick() {
        $(document).off('contextmenu', '.image-tile').on('contextmenu', '.image-tile', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });
    }

    bindMultiSelectButtons() {
        $(document).off('click', '.btn-multi-select').on('click', '.btn-multi-select', (e) => {
            preventDefaults(e);
            this.toggleMultiSelectOption($(e.currentTarget));
        });

        $(document).off('click', '.btn-multi-select-all').on('click', '.btn-multi-select-all', (e) => {
            preventDefaults(e);
            $('.btn-multi-select').each((_, elem) => {
                this.setMultiSelectOption($(elem), true);
            });
        });

        $(document).off('keydown.selectAll').on('keydown.selectAll', (e) => {
            if (!$('#media-viewer-post').is(':focus')) {
                if (e.ctrlKey && e.key.toLowerCase() === 'a') {
                    preventDefaults(e);

                    const deselectedCount = $('.btn-multi-select.fa-square').length;
                    $('.btn-multi-select').each((_, elem) => {
                        this.setMultiSelectOption($(elem), deselectedCount > 0);
                    });
                }
            }
        });

        $(document).off('click', '.btn-multi-deselect-all').on('click', '.btn-multi-deselect-all', (e) => {
            preventDefaults(e);
            $('.btn-multi-select').each((_, elem) => {
                this.setMultiSelectOption($(elem), false);
            });
        });

        $(document).off('click', '.media-viewer-card').on('click', '.media-viewer-card', (e) => {
            const checkbox = $(e.currentTarget).find('.btn-multi-select');
            if (e.ctrlKey) {
                this.toggleMultiSelectOption(checkbox);
            } else if (e.shiftKey) {
                this.shiftMultiSelectOption(checkbox);
            }
        });
    }

    bindSwipe() {
        $(document).off('click touchstart touchend mousedown mouseup', '.media-viewer .media-viewer-content .media-viewer-image').on('click touchstart touchend mousedown mouseup', '.media-viewer .media-viewer-content .media-viewer-image', (e) => {
            //e.preventDefault();
            e.stopPropagation();

            try {
                const element = $(e.currentTarget);

                if (e.originalEvent.type === 'click') {
                    const position = e.pageX - element.offset().left;
                    if (position <= (element.width() / 2)) {
                        this.moveSlide(-1);
                    } else {
                        this.moveSlide(1);
                    }
                } else if (e.originalEvent.type === 'touchstart' || e.originalEvent.type === 'mousedown') {
                    this.touchStartPosX = e.touches ? e.touches[0].screenX : e.screenX;
                    this.touchStartPosY = e.touches ? e.touches[0].screenY : e.screenY;
                } else if (e.originalEvent.type === 'touchend' || e.originalEvent.type === 'mouseup') {
                    const touchEndPosX = e.changedTouches ? e.changedTouches[0].screenX : e.screenX;
                    const touchEndPosY = e.changedTouches ? e.changedTouches[0].screenY : e.screenY;

                    const touchDiffX = Math.abs(this.touchStartPosX - touchEndPosX);
                    const touchDiffY = Math.abs(this.touchStartPosY - touchEndPosY);

                    if (touchDiffX > 100) {
                        if (touchEndPosX < this.touchStartPosX) {
                            this.moveSlide(1);
                        } else if (touchEndPosX > this.touchStartPosX) {
                            this.moveSlide(-1);
                        }
                    } else if (touchDiffY > 100) {
                        if ($('.media-viewer-comments').hasClass('d-none')) {
                            if (touchEndPosY < this.touchStartPosY) {
                                this.moveSlide(1);
                            } else if (touchEndPosY > this.touchStartPosY) {
                                this.moveSlide(-1);
                            }
                        }
                    } else {
                        const pageX = e.changedTouches ? e.changedTouches[0].pageX : e.pageX;
                        const position = pageX - element.offset().left;
                        if (position <= (element.width() / 2)) {
                            this.moveSlide(-1);
                        } else {
                            this.moveSlide(1);
                        }
                    }
                }
            } catch (ex) {
                console.log(ex);
            }
        });
    }

    bindArrowKeys() {
        $(document).on('keyup', (e) => {
            if (!$('#media-viewer-post').is(':focus')) {
                if ($('.media-viewer .media-viewer-content').is(':visible') && $('.media-viewer-comments').hasClass('d-none')) {
                    if (e.key === 'Escape') {
                        this.hideMediaViewer();
                    } else if (e.key === 'ArrowLeft') {
                        this.moveSlide(-1);
                    } else if (e.key === 'ArrowRight') {
                        this.moveSlide(1);
                    }
                }
            }
        });
    }

    bindClosePopup() {
        $(document).off('click', 'div#media-viewer-wrapper').on('click', 'div#media-viewer-wrapper', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.hideMediaViewer();
        });

        $(document).off('click', '.media-viewer-close').on('click', '.media-viewer-close', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.hideMediaViewer();
        });

        $(document).off('click', 'div.media-viewer').on('click', 'div.media-viewer', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });
    }

    bindLikeButton() {
        $(document).off('click', '.like-button').on('click', '.like-button', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const id = $('.media-viewer-like-button button').attr('data-like-id');
            const action = $('.media-viewer-like-button button').attr('data-action');
            this.like(id, action);
        });
    }

    bindCommentButtons() {
        $(document).off('click', '.comment-button').on('click', '.comment-button', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const element = $(e.currentTarget);
            const popup = element.closest('.media-viewer-popup');
            const viewer = popup.find('.media-viewer');

            if ($('.media-viewer-comments').hasClass('d-none')) {
                viewer.css({ 'margin-bottom': popup[0].style.top });
                $('.media-viewer-comments').removeClass('d-none');
            } else {
                viewer.css({ 'margin-bottom': '0px' });
                $('.media-viewer-comments').addClass('d-none');
            }
        });

        $(document).off('click', '#btn-post-media-comment').on('click', '#btn-post-media-comment', (e) => {
            const element = $(e.currentTarget);
            if (!element.hasClass('btn-disabled')) {
                const id = element.attr('data-gallery-item-id');
                const comment = $('#media-viewer-post').val()?.trim();

                if (comment !== undefined && comment.length > 0) {
                    this.comment(id, comment);
                }
            }
        });

        $(document).off('keyup', '#media-viewer-post').on('keyup', '#media-viewer-post', (e) => {
            let comment = $(e.currentTarget).val()?.trim();
            if (comment.length > this.maxCharacterCount) {
                comment = comment.substring(0, this.maxCharacterCount);
                $('#media-viewer-post').val(comment);
            }

            if (comment !== undefined && comment.length > 0) {
                $('#btn-post-media-comment').removeClass('btn-disabled');
                $('#btn-post-media-comment').addClass('btn-primary-2');
            } else {
                $('#btn-post-media-comment').removeClass('btn-primary-2');
                $('#btn-post-media-comment').addClass('btn-disabled');
            }

            $('.media-viewer-post-character-count').text(`${comment.length} / ${this.maxCharacterCount}`);
        });

        $(document).off('click', '.btn-delete-comment').on('click', '.btn-delete-comment', (e) => {
            const element = $(e.currentTarget);
            if (!element.hasClass('btn-disabled')) {
                const id = element.closest('.media-viewer-comment').attr('data-comment-id');
                this.deleteComment(id);
            }
        });
    }

    bindDownloadButton() {
        $(document).off('click', '.media-viewer-download').on('click', '.media-viewer-download', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const element = $(e.currentTarget).closest('.media-viewer');
            const source = element.data('source');
            this.download(source);
        });
    }

    bindPageEndEvent() {
        const path = window.location.pathname.toLowerCase();
        if (path.startsWith('/gallery')) {
            const paginationStyle = $('.pagination').data('pagination-style');
            if (paginationStyle !== undefined && paginationStyle === 'scrollloading') {
                $('#main-wrapper').off('scroll').on('scroll', (e) => {
                    const el = e.currentTarget;
                    const wrapperHeight = $(el).outerHeight();
                    const diff = el.scrollHeight - el.scrollTop - el.clientHeight;

                    if (diff < 200 && el.scrollHeight > this.maxScroll) {
                        this.maxScroll = el.scrollHeight;
                        const nextPage = this.page + 1;
                        loadGalleryPage(nextPage, true, () => {
                            this.page = nextPage;
                        });
                    }
                });
            }
        }
    }

    openMediaViewer(e) {
        let id = $(e).data('media-viewer-id');
        let index = $(e).data('media-viewer-index');
        let type = $(e).data('media-viewer-type');
        let collection = $(e).data('media-viewer-collection');

        this.displayMediaViewer(id, index, type, collection);
    }

    displayMediaViewer(id, index, type, collection) {
        this.hideMediaViewer();

        displayLoader(localization.translate('Loading'));

        let url;
        if (type !== undefined && type.length > 0) {
            if (type.toLowerCase() === 'pending_review') {
                url = '/MediaViewer/ReviewItem';
            } else if (type.toLowerCase() === 'custom_resource') {
                url = '/MediaViewer/CustomResource';
            } else if (type.toLowerCase() === 'gallery_item') {
                url = '/MediaViewer/GalleryItem';
            }
        }

        if (url !== undefined && url.length > 0) {
            $.ajax({
                url: url,
                type: 'GET',
                data: { id },
                success: (response) => {
                    hideLoader();
                    $('body').append(response);
                    $('#media-viewer-wrapper .media-viewer-popup .media-viewer').attr('data-media-viewer-index', `${index}`);
                    $('#media-viewer-wrapper .media-viewer-popup .media-viewer').attr('data-media-viewer-collection', `${collection}`);

                    this.bindLoadEvent();
                },
                error: (response) => {
                    hideLoader();
                    console.log(response);
                }
            });
        }
    }

    hideMediaViewer() {
        $('div#media-viewer-wrapper').hide();
        $('div#media-viewer-wrapper').remove();
    }

    initMediaViewImage(type, source) {
        this.resizeMediaViewer(1, $('#media-viewer-wrapper'), type, source);
        this.bindPopupEventHandlers();
    }

    resizeMediaViewer(iteration, popup, type, source) {
        let container = popup.find('.media-viewer-popup');
        let mediaContainer = container.find('.media-viewer-content');
        let media = mediaContainer.find('img');

        let margin = window.innerWidth > 900 ? 50 : 20;
        let targetWidth = popup.innerWidth() - (margin * 2);
        let targetHeight = popup.innerHeight() - (margin * 2);

        if (iteration == 1) {
            media.width(10);
        }

        if (container.outerWidth() < targetWidth && container.outerHeight() < targetHeight) {
            media.width(media.width() + 10);

            clearTimeout(this.resizePopupTimeout);
            this.resizePopupTimeout = setTimeout(() => {
                this.resizeMediaViewer(iteration + 1, popup, type, source);
            }, 5);
        } else {
            container.css({
                'top': `${(popup.innerHeight() - container.outerHeight()) / 2}px`,
                'left': `${(popup.innerWidth() - container.outerWidth()) / 2}px`
            });

            let width = $('.media-viewer-content img').innerWidth();
            if (type === 'video') {
                let height = $('.media-viewer-content img').innerHeight();
                $('.media-viewer-content').prepend(`
                    <video width="${width}" height="${height}" controls autoplay>
                        <source src="${source}" type="video/mp4">
                        ${localization.translate('Browser_Does_Not_Support')}
                    </video>
                `);
                $('.media-viewer-content img').remove();
                $('.media-viewer-content .media-viewer-actions').css({
                    'bottom': '40px'
                });
            }

            $('.media-viewer-row').css({
                'max-width': `${width}px`
            });

            popup.fadeTo(500, 1.0);
        }
    }

    like(id, action) {
        $.ajax({
            url: '/MediaViewer/Like',
            type: 'POST',
            data: { id, action },
            success: function (response) {
                if (response !== undefined && response.success) {
                    $('.media-viewer-like-button .lbl-like-count').text(response.value);
                    if (action.toLowerCase() === 'like') {
                        $('.media-viewer-like-button button').addClass('like-button-active');
                        $('.media-viewer-like-button button').attr('data-action', 'unlike')
                    } else {
                        $('.media-viewer-like-button button').removeClass('like-button-active');
                        $('.media-viewer-like-button button').attr('data-action', 'like')
                    }
                }
            }
        });
    }

    comment(id, value) {
        if (value === undefined || value.length <= 0) {
            displayMessage(localization.translate('Post_Comment'), localization.translate('Comment_Empty_Value'));
        } else if (value.length > this.maxCharacterCount) {
            displayMessage(localization.translate('Post_Comment'), localization.translate('Comment_Value_Too_Long'));
        } else {
            const maxCharacterCount = this.maxCharacterCount;
            $.ajax({
                url: '/MediaViewer/Comment',
                type: 'POST',
                data: { id, value },
                success: function (response) {
                    if (response !== undefined && response.success) {
                        const comments = $('.media-viewer-comments');
                        comments.append(`
                            <div class="media-viewer-comment" data-comment-id="${response.id}">
                                <h6 class="text-truncate">${response.username}</h6>
                                <p class="comment-message">${response.value}</p>
                                <p class="comment-timestamp" title="${response.timestamp_full}">${localization.translate(response.timestamp)}</p>
                                <i class="btn-delete-comment fa-solid fa-trash-can"></i>
                            </div>
                        `);
                        $('#media-viewer-post').val('');
                        $('.media-viewer-post-character-count').text(`0 / ${maxCharacterCount}`);
                        $('#btn-post-media-comment').removeClass('btn-primary-2');
                        $('#btn-post-media-comment').addClass('btn-disabled');
                    } else if (response.message !== undefined) {
                        displayMessage(localization.translate('Post_Comment'), response.message);
                    } else {
                        displayMessage(localization.translate('Post_Comment'), localization.translate('Unexpected_Error_Occurred'));
                    }
                }
            });
        }
    }

    deleteComment(id) {
        $.ajax({
            url: '/MediaViewer/DeleteComment',
            type: 'DELETE',
            data: { id },
            success: function (response) {
                if (response !== undefined && response.success) {
                    $(`.media-viewer-comment[data-comment-id='${id}']`).remove();
                } else if (response.message !== undefined) {
                    displayMessage(localization.translate('Delete_Comment'), response.message);
                } else {
                    displayMessage(localization.translate('Delete_Comment'), localization.translate('Unexpected_Error_Occurred'));
                }
            }
        });
    }

    download(source) {
        let parts = source.split('/');

        let a = document.createElement('a');
        a.href = source;
        a.download = parts[parts.length - 1];
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }

    getOrientation(item) {
        let width = item.width();
        let height = item.height();

        let orientation = 'unkown';
        if (width > height) {
            orientation = 'horizontal';
        } else if (width < height) {
            orientation = 'vertical';
        } else {
            orientation = 'square';
        }

        return orientation;
    }

    moveSlide(direction) {
        let viewer = $('.media-viewer .media-viewer-content').closest('.media-viewer');
        let currentIndex = viewer.data('media-viewer-index');
        let nextIndex = currentIndex + direction;
        let collection = viewer.data('media-viewer-collection');
        let items = $(`a[data-media-viewer-collection='${collection}']`);

        if (nextIndex < 0) {
            nextIndex = items.length - 1;
        } else if (nextIndex >= items.length) {
            nextIndex = 0;
        }

        let slide = $(`a[data-media-viewer-index='${nextIndex}']`);

        this.openMediaViewer(slide);
    }

    toggleMultiSelectOption(elem) {
        this.setMultiSelectOption(elem, elem.hasClass('fa-square'));
    }

    shiftMultiSelectOption(elem) {
        if (this.lastSelected) {
            const cards = $('.media-viewer-card');

            const currentSelectedId = parseInt(elem.closest('.media-viewer-card').find('.media-viewer-item').attr('data-media-viewer-index'));
            const lastSelectedId = parseInt(this.lastSelected.find('.media-viewer-item').attr('data-media-viewer-index'));
            
            const startItemId = Math.min(currentSelectedId, lastSelectedId);
            const endItemId = Math.max(currentSelectedId, lastSelectedId);

            cards.each((_, card) => {
                const currentItemId = parseInt($(card).find('.media-viewer-item').attr('data-media-viewer-index'));
                if (currentItemId >= startItemId && currentItemId <= endItemId) {
                    this.setMultiSelectOption($(card).find('.btn-multi-select'), true);
                }
            });
        }
    }

    setMultiSelectOption(elem, selected) {
        if (selected) {
            this.lastSelected = elem.closest('.media-viewer-card');
            elem.removeClass('fa-square').addClass('fa-square-check');
        } else {
            this.lastSelected = null;
            elem.removeClass('fa-square-check').addClass('fa-square');
        }

        this.setMultiSelectBtnStates();
    }

    setMultiSelectBtnStates() {
        const selectedCount = $('.btn-multi-select.fa-square-check').length;
        if (selectedCount === 0) {
            $('.btn-multi-select-all').removeClass('d-none').addClass('link-primary-2');
            $('.btn-multi-deselect-all').removeClass('link-primary-2').addClass('d-none');
            $('.btn-bulk-delete-resources').removeClass('link-danger').addClass('btn-faded');
        } else {
            $('.btn-multi-select-all').removeClass('link-primary-2').addClass('d-none');
            $('.btn-multi-deselect-all').removeClass('d-none').addClass('link-primary-2');
            $('.btn-bulk-delete-resources').removeClass('btn-faded').addClass('link-danger');
        }
    }

    setItemIndexes() {
        $('.image-container').each(function () {
            $(this).find('.media-viewer-item').each(function (index, item) {
                $(item).attr('data-media-viewer-index', index);
            });
        });
    }

    repositionPlayButton() {
        $('.media-viewer-item .media-viewer-play').each(function () {
            const element = $(this);
            const preview = element.parent();
            let thumbnail = $(preview.find('img')[0]);

            let adjustSizeFn = function () {
                const playButtonWidth = element.width();
                const playButtonHeight = element.height();

                const thumbnailWidth = thumbnail.outerWidth();
                const thumbnailHeight = thumbnail.outerHeight();

                preview.css('height', `${thumbnailHeight}px`);

                element.css({
                    'top': `-${(thumbnailHeight / 2)}px`,
                    'left': `${(thumbnailWidth / 2)}px`,
                    'margin-top': `-${playButtonHeight / 2}px`,
                    'margin-left': `-${playButtonWidth / 2}px`
                });

                element.fadeTo(200, 1.0);
            }

            thumbnail.on('load', adjustSizeFn);
            element.on('load', adjustSizeFn);

            adjustSizeFn();
        });
    }
}

export default MediaViewer;