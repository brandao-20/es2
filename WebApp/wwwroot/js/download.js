window.downloadFileFromBytes = (filename, contentType, byteArray) => {
    // Cria um blob a partir do array de bytes recebido
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    // Cria uma URL temporária para o blob
    const url = URL.createObjectURL(blob);
    // Cria um elemento <a> para simular o clique de download
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = filename;
    anchorElement.click();
    // Libera a URL temporária
    URL.revokeObjectURL(url);
};
