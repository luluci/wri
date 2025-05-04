


const zeroPadding = (value, len) => {
    return value.padStart(len, '0');
}
const toHex = (value, len) => {
    return zeroPadding(value.toString(16), len);
}
const sleepThread = (msec) => {
    var e = new Date().getTime() + (msec);
    while (new Date().getTime() <= e) { }
}

const getDOM = () => {
    const content = document.documentElement.outerHTML; // ページ全体のHTMLを取得
    return content;
} 

const writeToClipboard = (text) => {
    navigator.clipboard.writeText(text).then(() => {
        //console.log('Clipboard write successful!');
    }).catch(err => {
        console.error('Clipboard write failed: ', err);
    });
}

const save = () => {
    try {
        var result = window.confirm('変更を保存します。ファイルを上書きしますがよろしいですか？');
        if (result == true) {
            const path = wri.SourcePath;
            const dom = getDOM();
            wri.io.file.SaveTo(path, dom);
        }
    } catch (e) {
        console.error('Error saving file:', e);
    }
}
