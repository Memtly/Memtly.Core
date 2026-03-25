let auditSearchTimeout = null;

function init() {
    bindEventHandlers();
}

function bindEventHandlers() {
    bindAuditSearchBox();
}

function bindAuditSearchBox() {
    $(document).off('keyup', 'input#audit-log-search-term, input#audit-log-limit').on('keyup', 'input#audit-log-search-term, input#audit-log-limit', function (e) {
        let term = $('input#audit-log-search-term').val();
        let severity = $('select#audit-log-severity').val();
        let limit = $('input#audit-log-limit').val();

        searchAudit(term, severity, limit);
    });

    $(document).off('change', 'select#audit-log-severity').on('change', 'select#audit-log-severity', function (e) {
        let term = $('input#audit-log-search-term').val();
        let severity = $('select#audit-log-severity').val();
        let limit = $('input#audit-log-limit').val();

        searchAudit(term, severity, limit);
    });
}

export function searchAudit(term = '', severity = 3, limit = 10) {
    clearTimeout(auditSearchTimeout);
    auditSearchTimeout = setTimeout(() => {
        updateAuditList(term, severity, limit);
    }, 500);
}

export function resetAudit() {
    updateAuditList('');
}

export function updateAuditList(term = '', severity = 3, limit = 10) {
    $.ajax({
        type: 'POST',
        url: `/Audit/AuditList`,
        data: { term: term?.trim(), severity: severity, limit: limit },
        success: function (data) {
            $('#audit-list').html(data);
            bindEventHandlers();
        }
    });
}

export default init;