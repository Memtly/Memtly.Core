import { generateChecklistItem } from '@modules/html-elements';

function init() {
    initPasswordValidation();
}

export function initPasswordValidation() {
    if ($('div.password-validator-container').length > 0) {
        $('div.password-validator-container').each(function () {
            $(this).html(generatePasswordValidationContainer($(this).data('input')));
        });
    }

    if ($('.password-validator').length > 0) {
        $('.password-validator').each(function () {
            const validator = $(this);
            initPasswordValidationField(validator);
        });
    }
}

function initPasswordValidationField(validator) {
    let input = $(validator.data('input'));
    if (input !== undefined && input.length > 0) {
        let confirmField = input.parent().parent().parent().find('input.confirm-password');
        if (confirmField !== undefined && confirmField.length === 1) {
            validator.find('.lbl-confirm').removeClass('visually-hidden');
            confirmField.off('keyup').on('keyup', function () {
                var value = $(input).val();
                setPasswordValidationField(validator.find('.lbl-confirm'), confirmField.val() === value && value.length > 0);
                //setPasswordValidationField(validator, validator.find('li[class^=\'lbl-\']:not([class*=\'hidden\'])').length === 0);
            });
        }

        $(input).off('keyup').on('keyup', function () {
            var value = $(this).val();
            setPasswordValidationField(validator.find('.lbl-lower'), /[a-z]+?/.test(value));
            setPasswordValidationField(validator.find('.lbl-upper'), /[A-Z]+?/.test(value));
            setPasswordValidationField(validator.find('.lbl-number'), /[0-9]+?/.test(value));
            setPasswordValidationField(validator.find('.lbl-special'), /[^A-Za-z0-9]+?/.test(value));
            setPasswordValidationField(validator.find('.lbl-length'), value.length >= 8);

            if (confirmField !== undefined && confirmField.length === 1) {
                setPasswordValidationField(validator.find('.lbl-confirm'), confirmField.val() === value && value.length > 0);
            }

            //setPasswordValidationField(validator, validator.find('li[class^=\'lbl-\']:not([class*=\'hidden\'])').length === 0);
        });
    }
}

export function generatePasswordValidationContainer(field) {
    return `
        <div class="checklist password-validator" data-input="${field}">
            ${generateChecklistItem('lbl-lower', 'default', localization.translate('Password_Validation_Lower'))}
            ${generateChecklistItem('lbl-upper', 'default', localization.translate('Password_Validation_Upper'))}
            ${generateChecklistItem('lbl-number', 'default', localization.translate('Password_Validation_Numbers'))}
            ${generateChecklistItem('lbl-special', 'default', localization.translate('Password_Validation_Special'))}
            ${generateChecklistItem('lbl-length', 'default', localization.translate('Password_Validation_Length'))}
            ${generateChecklistItem('lbl-confirm', 'default', localization.translate('Password_Validation_Confirm'), true)}
        </div>
    `;
}

function setPasswordValidationField(field, valid) {
    let icon = field.find('.checklist-item-icon').first();
    if (valid) {
        field.removeClass('checklist-success checklist-error checklist-default').addClass('checklist-success');
        icon.removeClass('fa-square-check fa-square-xmark fa-square-minus').addClass('fa-square-check');
        icon.attr('data-icon', 'square-check');
    } else {
        field.removeClass('checklist-success checklist-error checklist-default').addClass('checklist-error');
        icon.removeClass('fa-square-check fa-square-xmark fa-square-minus').addClass('fa-square-xmark');
        icon.attr('data-icon', 'square-xmark');
    }
}

init();