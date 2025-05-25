// WBS要素
var wbs = null;
// toolbar要素
var toolbar = null;
var toolbox = null;
var toolbar_status = null;

// WBSテンプレート参照
var wbs_template = null; // テンプレート
var wbs_template_func = null; // 機能テンプレート
var wbs_template_phase = null; // フェーズテンプレート
var wbs_template_work = null; // ワークパッケージテンプレート
var wbs_template_task = null; // タスクテンプレート

// WBS編集ツール
var wbs_changed = false; // WBSが変更されたかどうかのフラグ

// 編集設定
const input_size_name = 20;
const input_size_owner = 2;
const input_size_date = 6;
const input_size_hour = 1;
const input_size_progress = 1;

// 挿入処理変数
const wbs_ctrl_mode_default = 0;    // 通常モード
const wbs_ctrl_mode_edit = 1;     // 要素挿入モード

var wbs_ctrl_mode = wbs_ctrl_mode_default;
var insert_mode = false;
var delete_mode = false;
var insert_indicator = null;
var insert_indicator_list = []; // 表示用に追加したインジケーターのリスト, 後で削除するために使用

document.addEventListener("DOMContentLoaded", function() {
    wbs = document.getElementById("WBS");

    // toolbar初期化
    initToolbar();

    // テンプレート参照取得
    wbs_template = document.getElementById("WBS_template");
    wbs_template_func = document.getElementById("WBS_function_template");
    wbs_template_phase = document.getElementById("WBS_phase_template");
    wbs_template_work = document.getElementById("WBS_work_template");
    wbs_template_task = document.getElementById("WBS_task_template");
    // 挿入位置インジケーター参照取得
    insert_indicator = wbs_template.querySelector(".insert_indicator");

    addEditable();
});

const initToolbar = () => {
    // toolbar表示
    toolbar = document.getElementById("WBS_toolbar");
    toolbar.style.setProperty("display", "table-row");
    toolbox = document.getElementById("WBS_toolbar_toolbox");
    toolbar_status = document.getElementById("WBS_toolbar_status").querySelector(".text");

    // 保存ボタン
    const saveButton = makeSaveButton();
    toolbox.appendChild(saveButton);
    // タスク追加ボタン
    const addTaskButton = makeEditButton();
    toolbox.appendChild(addTaskButton);
}
const resetToolbar = () => {
    // toolbar非表示
    toolbar.style.setProperty("display", "none");
    // 追加したボタンを削除
    toolbox.innerHTML = "";

    setWbsChanged(false);
}

const addEditable = () => {
    //
    const wbs = document.getElementById("WBS");
    const root = wbs.querySelector("tbody");
    const rows = root.querySelectorAll("tr");
    addEditableImpl(rows);
}
const addEditableImpl = (rows) => {
    for (let i = 0; i < rows.length; i++) {
        const row = rows[i];
        switch (row.className) {
            case "function":
                addFunctionEditable(row);
                break;
            case "phase":
                addPhaseEditable(row);
                break;
            case "work_package":
                addWorkEditable(row);
                break;
            case "task":
                addTaskEditable(row);
                break;

            default:
                break;
        }
    }
}
const addFunctionEditable = (row) => {
    // functionの変更対象
    const name = row.querySelector(".work_name span");
    applyEditable(name, null, input_size_name);
}
const addPhaseEditable = (row) => {
    // phaseの変更対象
    const name = row.querySelector(".work_name span");
    applyEditable(name, null, input_size_name);
}
const addWorkEditable = (row) => {
    // work_packageの変更対象
    const name = row.querySelector(".work_name span");
    applyEditable(name, null, input_size_name);
    const owner = row.querySelector(".work_owner span");
    applyEditable(owner, null, input_size_owner);
    const plan_date_begin = row.querySelector(".work_date_begin .plan span");
    applyEditable(plan_date_begin, null, input_size_date);
    const plan_date_end = row.querySelector(".work_date_end .plan span");
    applyEditable(plan_date_end, null, input_size_date);
    const actual_date_begin = row.querySelector(".work_date_begin .actual span");
    applyEditable(actual_date_begin, null, input_size_date);
    const actual_date_end = row.querySelector(".work_date_end .actual span");
    applyEditable(actual_date_end, null, input_size_date);
    const plan_man_hour = row.querySelector(".work_man_hour .plan span");
    applyEditable(plan_man_hour, null, input_size_hour);
    const actual_man_hour = row.querySelector(".work_man_hour .actual span");
    applyEditable(actual_man_hour, null, input_size_hour);
    const progress = row.querySelector(".work_progress span");
    applyEditable(progress, null, input_size_progress);
}
const addTaskEditable = (row) => {
    // taskの変更対象
    const name = row.querySelector(".work_name .text");
    applyEditable(name, null, input_size_name);
    const owner = row.querySelector(".work_owner .text");
    applyEditable(owner, null, input_size_owner);
    const plan_date_begin = row.querySelector(".work_date_begin .plan span");
    applyEditable(plan_date_begin, null, input_size_date);
    const plan_date_end = row.querySelector(".work_date_end .plan span");
    applyEditable(plan_date_end, null, input_size_date);
    const actual_date_begin = row.querySelector(".work_date_begin .actual span");
    applyEditable(actual_date_begin, null, input_size_date);
    const actual_date_end = row.querySelector(".work_date_end .actual span");
    applyEditable(actual_date_end, null, input_size_date);
    const plan_man_hour = row.querySelector(".work_man_hour .plan span");
    applyEditable(plan_man_hour, null, input_size_hour);
    const actual_man_hour = row.querySelector(".work_man_hour .actual span");
    applyEditable(actual_man_hour, null, input_size_hour);
    const progress = row.querySelector(".work_progress span");
    applyEditable(progress, null, input_size_progress);
}

const applyEditable = (elem, validator, size) => {
    elem.addEventListener("click", (e) => {
       // クリックしたらinputで編集可能にする
        if (elem.dataset.temp === undefined || elem.dataset.temp === "") {
            const input = document.createElement("input");

            const elem_class = elem.getAttribute("class");
            if (elem_class === "date") {
                // 初期値チェック
                if (elem.dataset.value === undefined || elem.dataset.value === "") {
                    elem.dataset.value = new Date().toISOString().slice(0, 10);
                }
                //
                input.type = "date";
                input.value = elem.dataset.value;
                input.size = size;
                elem.dataset.temp = elem.dataset.value;
                elem.innerText = "";
                elem.appendChild(input);
                input.focus();
                input.addEventListener("blur", () => {
                    if (validator) {
                        // バリデーションチェック
                        if (validator(input.value) === false) {
                            alert("無効な値です。");
                            input.value = elem.dataset.temp;
                        }
                    }
                    if (elem.dataset.temp !== input.value) {
                        setWbsChanged(true);
                    }
                    elem.dataset.temp = "";
                    elem.dataset.value = input.value;
                    // 表示用テキスト作成
                    const date = new Date(input.value);
                    const year = date.getFullYear();
                    const month = String(date.getMonth() + 1).padStart(2, '0');
                    const day = String(date.getDate()).padStart(2, '0');
                    elem.innerText = month + "/" + day;
                });
            } else {
                input.type = "text";
                input.value = elem.innerText;
                input.size = size;
                elem.dataset.temp = elem.innerText;
                elem.innerText = "";
                elem.appendChild(input);
                input.focus();
                input.addEventListener("blur", () => {
                    if (validator) {
                        // バリデーションチェック
                        if (validator(input.value) === false) {
                            alert("無効な値です。");
                            input.value = elem.dataset.temp;
                        }
                    }
                    if (elem.dataset.temp !== input.value) {
                        setWbsChanged(true);
                    }
                    elem.dataset.temp = "";
                    elem.innerText = input.value;
                });
            }

            input.addEventListener("keydown", (e) => {
                if (e.key === "Enter") {
                    input.blur();
                } else if (e.key === "Escape") {
                    input.value = elem.dataset.temp;
                    input.blur();
                }
            });
        }
    });
}

// F5キーによるreloadを禁止
document.addEventListener("keydown", function (e) {
    if (wbs_changed === true) {
        // deprecated: (e.which || e.keyCode) == 116
        if (e.key === 'F5') {
            var result = window.confirm('WBSが変更されています。保存せずにリロードすると変更内容は失われます。');
            if (result === true) {
                // 
                wri.preventClose = false;
            } else {
                e.preventDefault();
            }
        }
    }
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        // ブラウザの保存ダイアログを無効化
        e.preventDefault();
        // 保存処理を実行
        saveChange();
    }
});

const saveChange = () => {
    // 保存処理を実行
    if (wbs_changed === true) {
        let result = window.confirm('変更を保存します。ファイルを上書きしますがよろしいですか？');
        if (result === true) {
            // 編集モードを終了する
            if (wbs_ctrl_mode === wbs_ctrl_mode_edit) {
                changeEditMode();
            }
            // toolbarを非表示にする
            resetToolbar();

            // 保存処理を実行
            result = save(true);
            if (result === true) {
                setWbsChanged(false);
            } else {
                // 保存失敗
                alert("保存に失敗しました。");
                setWbsChanged(true);
            }

            // toolbarを初期化する
            initToolbar();
        }
    }
}

const makeSaveButton = () => {
    let button = document.createElement("button");
    button.innerText = "保存";
    button.addEventListener("click", function () {
        // 保存処理を実行
        saveChange();
    });
    return button;
}

const makeEditButton = () => {
    let button = document.createElement("button");
    button.innerText = "編集";
    button.addEventListener("click", () => {
        //
        if (wbs_ctrl_mode === wbs_ctrl_mode_edit) {
            // ボタンの表示を元に戻す
            button.innerText = "編集";
        } else {
            // ボタンの表示を変更する
            button.innerText = "編集終了";
        }
        //
        changeEditMode();
    });

    return button;
}

const changeEditMode = () => {
    if (wbs_ctrl_mode === wbs_ctrl_mode_edit) {
        // 編集モードのとき
        onExitEditMode();
        wbs_ctrl_mode = wbs_ctrl_mode_default;
    } else {
        // 編集モード以外のとき
        addRemoveButton();
        // 挿入モードを設定する
        addInsertIndicator();
        wbs_ctrl_mode = wbs_ctrl_mode_edit;
    }
}

const onExitEditMode = () => {
    // 編集モード終了時の処理
    // インジケーターを削除
    removeInsertIndicator();
    // 削除ボタンを消しながらIDを修正する
    const wbs = document.getElementById("WBS");
    const root = wbs.querySelector("tbody");
    const rows = root.querySelectorAll("tr");
    let i = 0;
    let func_id = 0;
    let phase_id = 1;
    let work_id = 1;
    let task_id = 1;
    while (i < rows.length) {
        const row = rows[i];
        const tgt_elem = row.querySelector(".work_id");
        const id_elem = tgt_elem?.querySelector("span");
        switch (row.className) {
            case "function":
                func_id++;
                phase_id = 0;
                work_id = 0;
                task_id = 0;
                tgt_elem.removeChild(tgt_elem.lastElementChild);
                id_elem.innerText = func_id;
                break;

            case "phase":
                phase_id++;
                work_id = 0;
                task_id = 0;
                tgt_elem.removeChild(tgt_elem.lastElementChild);
                id_elem.innerText = func_id + "-" + phase_id;
                break;

            case "work_package":
                work_id++;
                task_id = 0;
                tgt_elem.removeChild(tgt_elem.lastElementChild);
                id_elem.innerText = func_id + "-" + phase_id + "-" + work_id;
                break;

            case "task":
                task_id++;
                tgt_elem.removeChild(tgt_elem.lastElementChild);
                id_elem.innerText = func_id + "-" + phase_id + "-" + work_id + "-" + task_id;
                break;

            default:
                break;
        }
        i++;
    }
}

const addRemoveButton = () => {
    const wbs = document.getElementById("WBS");
    const root = wbs.querySelector("tbody");
    const rows = root.querySelectorAll("tr");
    addRemoveButtonImpl(rows);
}
const addRemoveButtonImpl = (rows) => {
    let i = 0;
    while (i < rows.length) {
        const row = rows[i];
        let btn = null;
        switch (row.className) {
            case "function":
                btn = makeFunctionRemoveButton(row);
                break;

            case "phase":
                btn = makePhaseRemoveButton(row);
                break;

            case "work_package":
                btn = makeWorkRemoveButton(row);
                break;

            case "task":
                btn = makeTaskRemoveButton(row);
                break;

            default:
                break;
        }
        if (btn != null) {
            const tgt_elem = row.querySelector(".work_id");
            tgt_elem.appendChild(btn);
            const id_elem = tgt_elem.querySelector("span");
            id_elem.innerText = "";
        }

        i++;
    }
}
const makeRemoveButton = (proc) => {
    const elem = document.createElement("button");
    elem.innerText = "削除"
    elem.addEventListener("click", proc);
    return elem;
}
const makeFunctionRemoveButton = (tgt_elem) => {
    return makeRemoveButton((e) => {
        const name_elem = tgt_elem.querySelector(".work_name span");
        const result = window.confirm("[Function][" + name_elem.innerText + "]を削除します。\nPhase,WorkPackage,Taskも削除されます。\nよろしいですか？");
        if (result) {
            let next_node = tgt_elem;
            do {
                tgt_elem = next_node;
                next_node = tgt_elem.nextElementSibling;
                tgt_elem.remove();
            } while (next_node != null && next_node.className != "function");
            // WBSが変更されたフラグを立てる
            setWbsChanged(true);
        }
    });
}
const makePhaseRemoveButton = (tgt_elem) => {
    return makeRemoveButton((e) => {
        const name_elem = tgt_elem.querySelector(".work_name span");
        const result = window.confirm("[Phase][" + name_elem.innerText + "]を削除します。\WorkPackage,Taskも削除されます。\nよろしいですか？");
        if (result) {
            let next_node = tgt_elem;
            do {
                tgt_elem = next_node;
                next_node = tgt_elem.nextElementSibling;
                tgt_elem.remove();
            } while (next_node != null && next_node.className != "function" && next_node.className != "phase");
            // WBSが変更されたフラグを立てる
            setWbsChanged(true);
        }
    });
}
const makeWorkRemoveButton = (tgt_elem) => {
    return makeRemoveButton((e) => {
        const name_elem = tgt_elem.querySelector(".work_name span");
        const result = window.confirm("[WorkPackage][" + name_elem.innerText + "]を削除します。\nTaskも削除されます。\nよろしいですか？");
        if (result) {
            let next_node = tgt_elem;
            do {
                tgt_elem = next_node;
                next_node = tgt_elem.nextElementSibling;
                tgt_elem.remove();
            } while (next_node != null && next_node.className != "function" && next_node.className != "phase" && next_node.className != "work_package");
            // WBSが変更されたフラグを立てる
            setWbsChanged(true);
        }
    });
}
const makeTaskRemoveButton = (tgt_elem) => {
    return makeRemoveButton((e) => {
        const name_elem = tgt_elem.querySelector(".work_name span");
        const result = window.confirm("[Task][" + name_elem.innerText + "]を削除します。\nよろしいですか？");
        if (result) {
            let next_node = tgt_elem.nextElementSibling;
            tgt_elem.remove();
            if (next_node != null) {
                next_node.remove();
            }
            // WBSが変更されたフラグを立てる
            setWbsChanged(true);
        }
    });
}


const removeInsertIndicator = () => {
    // インジケーターを削除する処理
    insert_indicator_list.forEach(indicator => {
        indicator.remove();
    });
    insert_indicator_list = [];
}

const addInsertIndicator = () => {

    //
    const wbs = document.getElementById("WBS");
    const root = wbs.querySelector("tbody");
    const rows = root.querySelectorAll("tr");
    //addInsertIndicatorImpl(root);

    // addInsertIndicatorImpl(wbs, "function", makeFunctionInsertIndicator, (func) => {
    //     // Function要素に対して適用する処理
    //     addInsertIndicatorImpl(func, "phase", makePhaseInsertIndicator, (phase) => {
    //         // Phase要素に対して適用する処理
    //         addInsertIndicatorImpl(phase, "work_package", makeWorkInsertIndicator, (work) => {
    //             // Work要素に対して適用する処理
    //             addInsertIndicatorImpl(work, "task", makeTaskInsertIndicator, (task) => {
    //                 // Task要素に対して適用する処理
    //                 // ここでは何もしない
    //             });
    //         });
    //     });
    // });

    // let i = 0;
    // while (i < rows.length) {
    //     const row = rows[i];
    //     if (row.className === "function") {
    //         // function要素に対して処理適用
    //         i = addFunctionInsertIndicator(root, rows, i);
    //     } else {
    //         i++;
    //     }
    // }

    addFunctionInsertIndicator(root, rows, 0);
}


const addFunctionInsertIndicator = (root, rows, idx) => {
    // functionを指した状態でコールする
    // functionの前に必ずインジケーターを追加
    let i = idx;
    while (i < rows.length) {
        const row = rows[i];
        if (row.className === "function") {
            // functionの前にインジケーターを追加
            const indicator = makeFunctionInsertIndicator(["function", null, null, null]);
            row.before(indicator);
            // 次の要素をチェック
            i++;
            if (i < rows.length) {
                const next_row = rows[i];
                switch (next_row.className) {
                    case "function":
                        // function -> function
                        // 間にプロセスが無い場合、挿入インジケーターを追加する
                        const indicator = makePhaseInsertIndicator(["phase", row.dataset.func_id, null, null]);
                        next_row.before(indicator);
                        break;
                    case "phase":
                        i = addPhaseInsertIndicator(root, rows, i);
                        break;
                    case "work_package":
                    case "task":
                    default:
                        // ありえない
                        alert("不正なWBS構造です : addFunctionInsertIndicator() : function以外の要素が出現しました");
                        throw new Error("不正なWBS構造です : addFunctionInsertIndicator() : function以外の要素が出現しました");
                }
            }
        } else {
            // function以外の出現はありえない
            // alert("不正なWBS構造です : addFunctionInsertIndicator() : function以外の要素が出現しました");
            // throw new Error("不正なWBS構造です : addFunctionInsertIndicator() : function以外の要素が出現しました");
            // ありえないので、無視する
            i++;
        }
    }
    // function末尾のインジケーターを追加する
    const indicator = makeFunctionInsertIndicator(["function", null, null, null]);
    root.appendChild(indicator);
    return i;
}
const addPhaseInsertIndicator = (root, rows, idx) => {
    let i = idx;
    let finish = false;
    let func_id = null;
    let phase_id = null;
    while (i < rows.length && finish === false) {
        const row = rows[i];
        if (row.className === "phase") {
            func_id = row.dataset.func_id;
            phase_id = row.dataset.phase_id;
            // phaseの前にインジケーターを追加
            const indicator = makePhaseInsertIndicator(["phase", func_id, null, null]);
            row.before(indicator);
            // 次の要素をチェック
            i++;
            if (i < rows.length) {
                const next_row = rows[i];
                switch (next_row.className) {
                    case "function":
                        finish = true;
                        break;
                    case "phase":
                        // phase -> phase
                        // 間にworkが無い場合、挿入インジケーターを追加する
                        const indicator = makeWorkInsertIndicator(["work_package", func_id, phase_id, null]);
                        next_row.before(indicator);
                        break;
                    case "work_package":
                        i = addWorkInsertIndicator(root, rows, i);
                        break;
                    case "task":
                    default:
                        // ありえない
                        alert("不正なWBS構造です : addPhaseInsertIndicator() : phase以外の要素が出現しました");
                        throw new Error("不正なWBS構造です : addPhaseInsertIndicator() : phase以外の要素が出現しました");
                }
            }
        } else {
            // phase以外の出現で終了
            finish = true;
        }
    }
    // phase末尾のインジケーターを追加する
    const indicator = makePhaseInsertIndicator(["phase", func_id, null, null]);
    if (i < rows.length) {
        const last_row = rows[i - 1];
        last_row.after(indicator);
    } else {
        // 末尾に追加する
        root.appendChild(indicator);
    }
    return i;
}
const addWorkInsertIndicator = (root, rows, idx) => {
    let i = idx;
    let finish = false;
    let func_id = null;
    let phase_id = null;
    let work_id = null;
    let has_task = false;
    while (i < rows.length && finish === false) {
        const row = rows[i];
        if (row.className === "work_package") {
            func_id = row.dataset.func_id;
            phase_id = row.dataset.phase_id;
            work_id = row.dataset.work_id;
            // workの前にインジケーターを追加
            const indicator = makeWorkInsertIndicator(["work_package", func_id, phase_id, null]);
            row.before(indicator);
            // 次の要素をチェック
            i++;
            if (i < rows.length) {
                const next_row = rows[i];
                switch (next_row.className) {
                    case "function":
                        finish = true;
                        break;
                    case "phase":
                        finish = true;
                        break;
                    case "work_package":
                        // work -> work
                        // 間にtaskが無い場合、挿入インジケーターを追加する
                        const indicator = makeTaskInsertIndicator(["task", func_id, phase_id, work_id]);
                        next_row.before(indicator);
                        break;
                    case "task":
                        has_task = true;
                        i = addTaskInsertIndicator(root, rows, i);
                        break;
                    default:
                        // ありえない
                        alert("不正なWBS構造です : addWorkInsertIndicator() : work以外の要素が出現しました");
                        throw new Error("不正なWBS構造です : addWorkInsertIndicator() : work以外の要素が出現しました");
                }
            }
        } else {
            // work以外の出現で終了
            finish = true;
        }
    }
    // workブロック内にtaskが無い場合、挿入インジケーターを追加する
    if (!has_task) {
        const indicator = makeTaskInsertIndicator(["task", func_id, phase_id, work_id]);
        if (i < rows.length) {
            const last_row = rows[i - 1];
            last_row.after(indicator);
        } else {
            // 末尾に追加する
            root.appendChild(indicator);
        }
    }
    // work末尾のインジケーターを追加する
    const indicator = makeWorkInsertIndicator(["work_package", func_id, phase_id, null]);
    if (i < rows.length) {
        const last_row = rows[i - 1];
        last_row.after(indicator);
    } else {
        // 末尾に追加する
        root.appendChild(indicator);
    }
    return i;
}
const addTaskInsertIndicator = (root, rows, idx) => {
    let i = idx;
    let finish = false;
    let func_id = null;
    let phase_id = null;
    let work_id = null;
    while (i < rows.length && finish === false) {
        const row = rows[i];
        if (row.className === "task") {
            func_id = row.dataset.func_id;
            phase_id = row.dataset.phase_id;
            work_id = row.dataset.work_id;
            // taskの前にインジケーターを追加
            const indicator = makeTaskInsertIndicator(["task", func_id, phase_id, work_id]);
            row.before(indicator);
            i++;
        } else {
            // task以外の出現で終了
            finish = true;
        }
    }
    // task末尾のインジケーターを追加する
    const indicator = makeTaskInsertIndicator(["task", func_id, phase_id, work_id]);
    if (i < rows.length) {
        const last_row = rows[i - 1];
        last_row.after(indicator);
    } else {
        // 末尾に追加する
        root.appendChild(indicator);
    }
    return i;
}


const makeInsertIndicator = (updateInfo, label, color, template, procInsertIndicator, procUpdateId) => {
    const indicator = insert_indicator.cloneNode(true);
    const tool_elem = indicator.querySelector("td");
    indicator.id = null;
    //const button = indicator.querySelector("div");
    //button.style.setProperty("background-color", color);
    const button = document.createElement("button");
    tool_elem.appendChild(button);
    button.innerText = label;
    button.addEventListener("click", () => {
        // インジケータークリック:挿入処理
        // 挿入要素をコピーして作成
        const root = template.cloneNode(true);
        // root.style.setProperty("display", "contents");
        // indicator.after(root);
        // インジケーターを適用
        //const root = wbs.querySelector("tbody");
        const rows = root.querySelectorAll("tr");
        addEditableImpl(rows);
        addRemoveButtonImpl(rows);
        procInsertIndicator(root, rows, 0);
        // インジケーターの前に新しい要素を追加する
        const ary = Array.from(root.children);
        ary.pop();
        while (ary.length > 0) {
            const e = ary.shift();
            indicator.before(e);
        }
        //
        // 機能IDを更新する
        // procUpdateId(updateInfo);
        // WBSが変更されたフラグを立てる
        setWbsChanged(true);
    });
    // 追加したインジケーターをリストに追加
    insert_indicator_list.push(indicator);
    return indicator;
}
const makeFunctionInsertIndicator = (updateInfo) => {
    return makeInsertIndicator(updateInfo, "Function", "rgb(0, 65, 139)", wbs_template_func, addFunctionInsertIndicator, updateFuncId);
}
const makePhaseInsertIndicator = (updateInfo) => {
    return makeInsertIndicator(updateInfo, "Phase", "rgb(223, 243, 255)", wbs_template_phase, addPhaseInsertIndicator, updatePhaseId);
}
const makeWorkInsertIndicator = (updateInfo) => {
    return makeInsertIndicator(updateInfo, "Work", "rgb(243, 255, 223)", wbs_template_work, addWorkInsertIndicator, updateWorkId);
}
const makeTaskInsertIndicator = (updateInfo) => {
    return makeInsertIndicator(updateInfo, "Task", "rgb(255, 255, 255)", wbs_template_task, addTaskInsertIndicator, updateTaskId);
}


const updateFuncId = (updateInfo) => {
    // 機能IDを更新する処理
    const rows = wbs.querySelectorAll("tbody tr");
    let i = 0;
    updateFuncIdImpl(rows, i);
}
const updatePhaseId = (updateInfo) => {
    // フェーズIDを更新する処理
    const rows = wbs.querySelectorAll("tbody tr");
    const func_id = updateInfo[1];
    let i = 0;
    while (i < rows.length) {
        const row = rows[i];
        if (row.className == "phase") {
            if (row.dataset.func_id === func_id) {
                updatePhaseIdImpl(rows, i, func_id);
                break;
            }
        }
        i++;
    }
}
const updateWorkId = (updateInfo) => {
    // ワークパッケージIDを更新する処理
    const rows = wbs.querySelectorAll("tbody tr");
    let i = 0;
    updateWorkIdImpl(rows, i, updateInfo[1], updateInfo[2]);
}
const updateTaskId = (updateInfo) => {
    // タスクIDを更新する処理
    const rows = wbs.querySelectorAll("tbody tr");
    let i = 0;
    updateTaskIdImpl(rows, i, updateInfo[1], updateInfo[2], updateInfo[3]);
}


const updateFuncIdImpl = (rows, i) => {
    // 機能IDを更新する処理
    let func_id = 1;
    while (i < rows.length) {
        const row = rows[i];
        if (row.className == "function") {
            // id更新
            row.dataset.func_id = func_id;
            const id_elem = row.querySelector(".work_id .text");
            id_elem.innerText = func_id;
            i++;
            // phase更新
            i = updatePhaseIdImpl(rows, i, func_id);
            func_id++;
        } else {
            i++;
        }
    }
}
const updatePhaseIdImpl = (rows, i, func_id) => {
    let phase_id = 1;
    while (i < rows.length) {
        const row = rows[i];
        if (row.className == "function") {
            break;
        } else if (row.className == "phase") {
            // id更新
            row.dataset.func_id = func_id;
            row.dataset.phase_id = phase_id;
            const id_elem = row.querySelector(".work_id .text");
            id_elem.innerText = func_id + "-" + phase_id;
            i++;
            // work更新
            i = updateWorkIdImpl(rows, i, func_id, phase_id);
            phase_id++;
        } else {
            i++;
        }
    }
    return i;
}
const updateWorkIdImpl = (rows, i, func_id, phase_id) => {
    let work_id = 1;
    while (i < rows.length) {
        const row = rows[i];
        if (row.className == "function") {
            break;
        } else if (row.className == "phase") {
            break;
        } else if (row.className == "work_package") {
            // id更新
            row.dataset.func_id = func_id;
            row.dataset.phase_id = phase_id;
            row.dataset.work_id = work_id;
            const id_elem = row.querySelector(".work_id .text");
            id_elem.innerText = func_id + "-" + phase_id + "-" + work_id;
            i++;
            // Task更新
            i = updateTaskIdImpl(rows, i, func_id, phase_id, work_id);
            work_id++;
        } else {
            i++;
        }
    }
    return i;
}
const updateTaskIdImpl = (rows, i, func_id, phase_id, work_id) => {
    let task_id = 1;
    while (i < rows.length) {
        const row = rows[i];
        if (row.className == "function") {
            break;
        } else if (row.className == "phase") {
            break;
        } else if (row.className == "work_package") {
            break;
        } else if (row.className == "task") {
            // id更新
            row.dataset.func_id = func_id;
            row.dataset.phase_id = phase_id;
            row.dataset.work_id = work_id;
            row.dataset.task_id = task_id;
            const id_elem = row.querySelector(".work_id .text");
            id_elem.innerText = func_id + "-" + phase_id + "-" + work_id + "-" + task_id;
            i++;
            task_id++;
        } else {
            i++;
        }
    }
    return i;
}


const setWbsChanged = (changed) => {
    // WBSが変更されたかどうかのフラグを設定する
    wbs_changed = changed;
    wri.preventClose = changed;
    if (changed) {
        // 変更あり
        toolbar_status.innerText = "変更あり";
        toolbar_status.style.setProperty("color", "rgb(255, 63, 63)");
        //
        wri.preventCloseMsg = "WBSが変更されています。保存せずに閉じると、変更内容は失われます。";
    } else {
        // 変更なし
        toolbar_status.innerText = "編集ツール";
        toolbar_status.style.setProperty("color", null);
        //
        wri.preventCloseMsg = "";
    }
}
