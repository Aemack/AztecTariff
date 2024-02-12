window.downloadFiles = function (fileUrls) {

    for (var i = 0; i < fileUrls.length; i++) {
        console.log(fileUrls[i])
        var link = document.createElement("a");
        link.href = fileUrls[i];
        link.download = "file" + i; // You can set custom filenames
        link.target = "_blank";
        link.click();
    }
}

window.printPdf = function (pdfDataUri) {
    var blob = base64toBlob(pdfDataUri.split(',')[1], 'application/pdf');
    var blobUrl = URL.createObjectURL(blob);
    console.log("We got the blob")
    var iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    console.log("We made an iframe")
    iframe.src = blobUrl;
    console.log("The blob is in the iframe")

    document.body.appendChild(iframe);
    console.log("The iframe blob monstrosity is in the body!")

    iframe.onload = function () {
        iframe.contentWindow.print();
        console.log("OH GOD! The Ifrblob is trying ro print!")
        //document.body.removeChild(iframe);

        console.log("IT'S REMOVED THE BLOB WE'RE SAVED!")
    };
};

function base64toBlob(base64, mime) {
    var binary = atob(base64);
    var array = [];
    for (var i = 0; i < binary.length; i++) {
        array.push(binary.charCodeAt(i));
    }
    return new Blob([new Uint8Array(array)], { type: mime });
}

