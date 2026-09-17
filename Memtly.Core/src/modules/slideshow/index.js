class Slideshow {
    constructor(selector, slideInterval = 10000, fadeInterval = 500) {
        this.selector = selector;
        this.slidetimer = null;
        this.transitionTimer = null;
        this.currentSlide = 0;
        this.slideInterval = slideInterval;
        this.fadeInterval = fadeInterval;
    }

    init() {
        this.elem = $(this.selector);
        if (this.elem !== undefined) {
            this.currentSlide = 0;

            const slideCount = this.elem.find('.slideshow-slide').length;
            if (slideCount > 0) {
                const windowWidth = $(window).outerWidth() - 40;
                const windowHeight = $(window).outerHeight();
                const headerHeight = $('.navbar').outerHeight();
                const footerHeight = $('footer').outerHeight();
                const creditsHeight = $('.credits').length > 0 ? 20 : 0;
                const reviewCounterHeight = $('.review-counter').length > 0 ? $('.review-counter').outerHeight() + 70 : 50;
                const totalHeight = headerHeight + footerHeight + reviewCounterHeight + creditsHeight;

                let slideHeight = windowHeight - totalHeight;
                if (slideHeight > windowWidth) {
                    slideHeight = windowWidth;
                }

                const qrCodeVal = this.elem.find('.slideshow-slide .share-slide').attr('data-value');
                this.elem.find('.slideshow-slide .share-slide').qrcode({ width: slideHeight, height: slideHeight, text: qrCodeVal });

                this.elem.height(slideHeight);
                const slides = this.elem.find('.slideshow-slide');
                slides.each(function (index) {
                    $(this).attr('data-slide-index', index);
                });
                this.elem.find('.slideshow-slide[data-slide-index="0"]').show();

                clearInterval(this.slidetimer);

                const advanceSlide = () => {
                    this.currentSlide++;

                    if (this.currentSlide >= slideCount) {
                        this.loadNewImages();
                    }

                    this.elem.find('.slideshow-slide').fadeOut(this.fadeInterval);
                    clearTimeout(this.transitionTimer);
                    this.transitionTimer = setTimeout(() => {
                        const slide = this.elem.find(`.slideshow-slide[data-slide-index="${this.currentSlide}"]`);
                        slide.fadeIn(this.fadeInterval);

                        const mediaType = slide.data('media-type');
                        let nextDelay = this.slideInterval;

                        if (mediaType === 'video') {
                            const videoEl = slide.find('.slideshow-slide-video')[0];
                            videoEl.currentTime = 0;
                            videoEl.play().catch(err => console.warn('Video play failed:', err));

                            const scheduleWithDuration = () => {
                                const maxVideoPlaybackDuration = 60;
                                nextDelay = (videoEl.duration < maxVideoPlaybackDuration ? videoEl.duration : maxVideoPlaybackDuration) * 1000;
                                this.slidetimer = setTimeout(advanceSlide, nextDelay);
                            };

                            if (videoEl.readyState >= 1) {
                                scheduleWithDuration();
                            } else {
                                videoEl.addEventListener('loadedmetadata', scheduleWithDuration, { once: true });
                            }
                        } else {
                            this.slidetimer = setTimeout(advanceSlide, nextDelay);
                        }
                    }, this.fadeInterval);
                };

                this.slidetimer = setTimeout(advanceSlide, this.slideInterval);
            }
        }
    }

    loadNewImages() {
        const params = new URLSearchParams(window.location.search);

        $.ajax({
            type: 'GET',
            url: `${window.location.pathname}${window.location.search}&mode=${params.get('mode')}&group=${params.get('group')}&order=${params.get('order')}&filter=${params.get('filter')}&culture=${params.get('culture')}&partial=true`,
            success: (data) => {
                clearInterval(this.slidetimer);
                clearTimeout(this.transitionTimer);
                $('#main-gallery').html(data);
                this.init(this.fadeInterval);
            }
        });
    }
}

export default Slideshow;