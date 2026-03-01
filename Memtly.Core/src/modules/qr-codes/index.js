function init() {
    generateQrCodes();
}

function generateQrCodes() {
    $('.qrcode-wrapper').each(function () {
        const text = $(this).attr('data-value');
        if (text !== undefined) {
            $(this).html('<div class="qrcode"></div><div class="qrcode-download d-none"></div>');
            $(this).find('.qrcode').qrcode({ width: 150, height: 150, text });
            $(this).find('.qrcode-download').qrcode({ width: 1000, height: 1000, text });
        }
    });
}

export default init;