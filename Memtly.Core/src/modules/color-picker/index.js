function init() {
    bindColorPickers();
}

function bindColorPickers() {
    $('.form-color-picker-group').each(function () {
        const elem = $(this);

        const textInput = elem.find('.form-control-text');
        const colorInput = elem.find('.form-control-color');
        const defaultColor = textInput.attr('placeholder');

        textInput.off('change').on('change', (e) => {
            try {
                let colour = e.currentTarget.value.toUpperCase();

                const regex = /\#[0-9A-F]{6}/;
                if (!regex.test(colour)) {
                    colour = defaultColor;
                }

                colorInput.val(colour);
                textInput.val(colour);
                textInput.attr('data-updated', 'true');
            } catch { }
        });

        colorInput.off('change').on('change', (e) => {
            try {
                let colour = e.currentTarget.value.toUpperCase();

                const regex = /\#[0-9A-F]{6}/;
                if (!regex.test(colour)) {
                    colour = defaultColor;
                }

                colorInput.val(colour);
                textInput.val(colour);
                textInput.attr('data-updated', 'true');
            } catch { }
        });
    });
}

export default init;