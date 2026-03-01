export function downloadBlob(filename, contentType, data, xhr) {
    const downloadUrl = URL.createObjectURL(new Blob([data], { type: contentType }));

    const a = document.createElement('a');
    a.href = downloadUrl;

    let fileName = filename;
    if (xhr) {
        const disposition = xhr.getResponseHeader('Content-Disposition');
        if (disposition && disposition.indexOf('attachment') !== -1) {
            const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
            const matches = filenameRegex.exec(disposition);
            if (matches != null && matches[1]) {
                fileName = matches[1].replace(/['"]/g, '');
            }
        }
    }

    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(downloadUrl);
}