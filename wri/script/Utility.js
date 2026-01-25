


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

const getConfig = () => {
    const text = wri.config.GetConfig();
    if (text !== null) {
        const json = JSON.parse(text);
        return json;
    }
    return null;
}

const save = (force = false) => {
    try {
        let result = false;
        if (force) {
            result = true;
        } else {
            result = window.confirm('変更を保存します。ファイルを上書きしますがよろしいですか？');
        }
        if (result == true) {
            const path = wri.SourcePath;
            const dom = getDOM();
            wri.io.file.SaveTo(path, dom);
            return true;
        }
    } catch (e) {
        console.error('Error saving file:', e);
    }

    return false;
}

const wait = async (msec) => {
    return new Promise((resolve) => {
        setTimeout(() => {
            resolve();
        }, msec);
    });
}

const getDroppedFilePath = async (e) => {
    if (e.dataTransfer === null) {
        return null;
    }
    if (e.dataTransfer.types.length === 0) {
        return null;
    }
    const type0 = e.dataTransfer.types[0];
    if (type0 !== "Files") {
        return null;
    }

    wri.isDragDropInProgress = true;
    //return new Promise(async (resolve) => {
    //    for (let i = 0; i < 100; i++) {
    //        if (wri.isDragDropInProgress == false) {
    //            resolve(wri.droppedFilePath);
    //        }
    //        await wait(100);
    //    }
    //});
    for (let i = 0; i < 100; i++) {
        if (wri.isDragDropInProgress == false) {
            return wri.droppedFilePath;
        }
        await wait(100);
    }
    return null;
}

const getGetParam = () => {
    return new URLSearchParams(document.location.search);
}
