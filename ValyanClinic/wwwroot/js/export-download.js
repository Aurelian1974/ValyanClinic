window.exportDownload = async function (url) {
    try {
        const response = await fetch(url, { method: 'GET', credentials: 'same-origin' });

        if (response.status === 204) {
            return { success: false, reason: 'no-data', message: 'Nu există date pentru export cu filtrul selectat.' };
        }

        if (!response.ok) {
            let txt = await response.text();
            return { success: false, reason: 'http-error', status: response.status, message: txt || 'Eroare la export' };
        }

        const disposition = response.headers.get('content-disposition') || '';
        let filename = '';
        const match = /filename\*=UTF-8''(.+)$/.exec(disposition) || /filename="?([^;\"]+)"?/.exec(disposition);
        if (match) {
            filename = decodeURIComponent(match[1]);
        }

        const blob = await response.blob();
        if (!filename) {
            const contentType = response.headers.get('content-type');
            const ext = (contentType && contentType.includes('spreadsheet')) ? 'xlsx' : 'csv';
            filename = `export_${new Date().toISOString().replace(/[:.]/g,'')}.${ext}`;
        }

        const link = document.createElement('a');
        const objectUrl = URL.createObjectURL(blob);
        link.href = objectUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(objectUrl);

        return { success: true, filename };
    } catch (err) {
        return { success: false, reason: 'exception', message: err?.message || String(err) };
    }
};