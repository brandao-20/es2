window.downloadFileFromBytes = (fileName, contentType, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement("a");
    anchorElement.href = url;
    anchorElement.download = fileName;
    anchorElement.click();
    URL.revokeObjectURL(url);
};
