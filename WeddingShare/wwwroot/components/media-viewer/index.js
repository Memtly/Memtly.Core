import './media-viewer.css';
import { displayLoader, hideLoader } from '../loader';

export class MediaViewer {
    constructor() {
        this.playButtonTimeout = null;
        this.resizePopupTimeout = null;
        this.touchStartPosX = null;
        this.touchStartPosY = null;

        init();
    }

    init() {
        clearTimeout(this.playButtonTimeout);
        this.playButtonTimeout = setTimeout(function () {
            $('.media-viewer-item .media-viewer-play').each(function () {
                let element = $(this);
                let preview = element.parent();
                let thumbnail = $(preview.find('img')[0]);

                let adjustSizeFn = function () {
                    let size = element.height();
                    preview.css('height', `${thumbnail.outerHeight()}px`);

                    element.css({
                        'top': `-${(thumbnail.outerHeight() / 2)}px`,
                        'left': `${(thumbnail.outerWidth() / 2)}px`,
                        'margin-top': `-${size / 2}px`,
                        'margin-left': `-${size / 2}px`
                    });

                    element.fadeTo(200, 1.0);
                }

                thumbnail.on('load', adjustSizeFn);
                element.on('load', adjustSizeFn);

                adjustSizeFn();
            });
        }, 200);

        bindEventHandlers();
    }

    bindEventHandlers() {
        $(document).off('click', '.media-viewer-item').on('click', '.media-viewer-item', function (e) {
            e.preventDefault();
            e.stopPropagation();

            openMediaViewer(this);
        });

        $(document).off('contextmenu', '.image-tile').on('contextmenu', '.image-tile', function (e) {
            e.preventDefault();
            e.stopPropagation();
        });

        $(document).off('click touchstart touchend', '.media-viewer .media-viewer-content').on('click touchstart touchend', '.media-viewer .media-viewer-content', function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (e.originalEvent.type === 'click') {
                let position = e.pageX - $(this).offset().left;
                if (position <= ($(this).width() / 2)) {
                    moveSlide(-1);
                } else {
                    moveSlide(1);
                }
            } else if (e.originalEvent.type === 'touchstart') {
                touchStartPosX = e.touches[0].screenX;
                touchStartPosY = e.touches[0].screenY;
            } else if (e.originalEvent.type === 'touchend') {
                let touchEndPosX = e.changedTouches[0].screenX;
                let touchEndPosY = e.changedTouches[0].screenY;

                let touchDiffX = Math.abs(touchStartPosX - touchEndPosX);
                let touchDiffY = Math.abs(touchStartPosY - touchEndPosY);

                if (touchDiffX > 100) {
                    if (touchEndPosX < touchStartPosX) {
                        moveSlide(1);
                    } else if (touchEndPosX > touchStartPosX) {
                        moveSlide(-1);
                    }
                } else if (touchDiffY > 100) {
                    if (touchEndPosY < touchStartPosY) {
                        moveSlide(1);
                    } else if (touchEndPosY > touchStartPosY) {
                        moveSlide(-1);
                    }
                } else {
                    let position = e.changedTouches[0].pageX - $(this).offset().left;
                    if (position <= ($(this).width() / 2)) {
                        moveSlide(-1);
                    } else {
                        moveSlide(1);
                    }
                }
            }
        });

        $(document).on('keyup', function (e) {
            if ($('.media-viewer .media-viewer-content').is(':visible')) {
                if (e.key === 'Escape') {
                    hideMediaViewer();
                } else if (e.key === 'ArrowLeft') {
                    moveSlide(-1);
                } else if (e.key === 'ArrowRight') {
                    moveSlide(1);
                } else if (e.key === 'd') {
                    download();
                }
            }
        });

        $(document).off('click', 'div#media-viewer-popup').on('click', 'div#media-viewer-popup', function (e) {
            e.preventDefault();
            e.stopPropagation();
            hideMediaViewer();
        });

        $(document).off('click', 'div.media-viewer').on('click', 'div.media-viewer', function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
    }

    openMediaViewer(e) {
        let id = $(e).data('media-viewer-id');
        let index = $(e).data('media-viewer-index');
        let type = $(e).data('media-viewer-type');

        displayMediaViewer(id, index, type);
    }

    displayMediaViewer(id, index, type) {
        hideMediaViewer();

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
                success: function (response) {
                    hideLoader();
                    $('body').append(response);
                    $('#media-viewer-popup .media-viewer').attr('data-media-viewer-index', `${index}`);
                },
                error: function (response) {
                    hideLoader();
                    console.log(response);
                }
            });
        }
    }

    hideMediaViewer() {
        $('div#media-viewer-popup').hide();
        $('div#media-viewer-popup').remove();
    }

    initMediaViewImage(type, source) {
        resizeMediaViewer(1, $('#media-viewer-popup'), type, source);
    }

    resizeMediaViewer(iteration, popup, type, source) {
        let container = popup.find('.media-viewer');
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

            clearTimeout(resizePopupTimeout);
            resizePopupTimeout = setTimeout(function () {
                resizeMediaViewer(iteration + 1, popup, type, source);
            }, 5);
        } else {
            container.css({
                'top': `${(popup.innerHeight() - container.outerHeight()) / 2}px`,
                'left': `${(popup.innerWidth() - container.outerWidth()) / 2}px`
            });

            if (type === 'video') {
                let width = $('.media-viewer-content img').innerWidth();
                let height = $('.media-viewer-content img').innerHeight();
                $('.media-viewer-content').html(`
                <video width="${width}" height="${height}" controls autoplay>
                    <source src="${source}" type="video/mp4">
                    ${localization.translate('Browser_Does_Not_Support')}
                </video>
            `);
            }

            popup.fadeTo(500, 1.0);
        }
    }

    loginPrompt() {
        displayMessage(localization.translate('Login'), localization.translate('Login_To_Complete_Action'));
    }

    upgradeToUnlock() {
        displayMessage(localization.translate('Unavailable'), localization.translate('Paywall_Feature'));
    }

    like(id) {
        let action = $('#like-button button').attr('data-action');
        $.ajax({
            url: '/MediaViewer/Like',
            type: 'POST',
            data: { id, action },
            success: function (response) {
                if (response !== undefined && response.success) {
                    $('#like-button .lbl-like-count').text(response.value);
                    if (action.toLowerCase() === 'like') {
                        $('#like-button button').addClass('like-button-active');
                        $('#like-button button').attr('data-action', 'unlike')
                    } else {
                        $('#like-button button').removeClass('like-button-active');
                        $('#like-button button').attr('data-action', 'like')
                    }
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
        let index = viewer.data('media-viewer-index') + direction;
        let collection = viewer.data('media-viewer-collection');
        let items = $(`a[data-media-viewer-collection='${collection}']`);

        if (index < 0) {
            index = items.length - 1;
        } else if (index >= items.length) {
            index = 0;
        }

        let slide = $(`a[data-media-viewer-index='${index}']`);

        openMediaViewer(slide);
    }
}

export default MediaViewer;