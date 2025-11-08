// JavaScript helpers for file downloads

window.downloadFileFromBytes = function (filename, contentType, data) {
    // Create blob from byte array
    const blob = new Blob([new Uint8Array(data)], { type: contentType });
    
    // Create download link
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    
    // Trigger download
    document.body.appendChild(link);
   link.click();
    
    // Cleanup
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};
