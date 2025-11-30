import '../css/index.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap';
import '@fortawesome/fontawesome-free/css/all.min.css';
import '@fortawesome/fontawesome-free/js/all.js';
import 'jquery-loading';
import 'jquery-qrcode';
import 'jquery-validation';
import 'jquery-validation-unobtrusive';

import { Localization } from '../components/localization';
import '../components/presentation';
import '../components/cookies';
import '../components/sponsors';

const app = {
    initialized: false,
    config: {
        theme: 'default',
        debug: true
    }
};

function init() {
    if (app.initialized) return;

    app.config.theme = document.body.dataset.theme.toLowerCase();
    app.initialized = true;

    let culture = window.navigator.language;
    //if (culture !== undefined && culture.length > 0) {
    //    $.ajax({
    //        type: "POST",
    //        url: '/Language/ChangeDisplayLanguage',
    //        data: { culture: culture },
    //        success: function (data) {
    //            if (data.success) {
    //                window.location.reload();
    //            }
    //        }
    //    });
    //}

    resizeLayout();
    bindEventHandlers();
}

function bindEventHandlers() {
    if ($('div.navbar-options').length == 0) {
        var presentationTimeout = setTimeout(function () {
            $('.presentation-hidden').fadeOut(500);
            $('body').css('cursor', 'none');
        }, 1000);

        $(document).off('mousemove').on('mousemove', function () {
            $('.presentation-hidden').fadeIn(200);
            $('body').css('cursor', 'default');

            clearTimeout(presentationTimeout);
            presentationTimeout = setTimeout(function () {
                $('.presentation-hidden').fadeOut(500);
                $('body').css('cursor', 'none');
            }, 1000);
        });
    }
}

function resizeLayout() {
    if ($('div#main-wrapper').length > 0) {
        let windowWidth = $(window).width();
        let windowHeight = $(window).height();
        let navHeight = $('nav.navbar').outerHeight();
        let alertHeight = $('.header-alert').length > 0 ? $('.header-alert').outerHeight() : 0;
        let footerHeight = $('footer').outerHeight();
        let bodyHeight = windowHeight - (navHeight + footerHeight + alertHeight);

        $('div#main-wrapper').css({
            'height': `${bodyHeight + alertHeight}px`,
            'max-height': `${bodyHeight + alertHeight}px`,
            'top': `${navHeight}px`
        });

        if ($('div#main-content').length > 0) {
            let contentHeight = $('div#main-content').outerHeight();
            let padding = (bodyHeight - contentHeight) / 2;

            if (windowWidth >= 700 && padding < 30) {
                padding = 30;
            } else if (windowWidth < 700 && padding < 20) {
                padding = 20;
            }

            $('div#main-content').css({
                'padding-top': `${padding}px`,
                'padding-bottom': `50px`
            });
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    init();

    window.localization = new Localization();

    window.preventDefaults = event => {
        event.preventDefault();
        event.stopPropagation();
    };
});


export { app };