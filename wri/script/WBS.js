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

// 挿入処理変数
var insert_mode = false;
var insert_indicator = null;
var insert_indicator_list = []; // 表示用に追加したインジケーターのリスト, 後で削除するために使用

document.addEventListener("DOMContentLoaded", function() {
    wbs = document.getElementById("WBS");

    // toolbar初期化
    initToolbar();

    // 挿入位置インジケーター参照取得
    insert_indicator = document.getElementById("work_insert_indicator");
    // テンプレート参照取得
    wbs_template = document.getElementById("WBS_template");
    wbs_template_func = wbs_template.querySelector(".function");
    wbs_template_phase = wbs_template.querySelector(".phase");
    wbs_template_work = wbs_template.querySelector(".work_package");
    wbs_template_task = wbs_template.querySelector(".task");

    addEditable();
});

const initToolbar = () => {
    // toolbar表示
    toolbar = document.getElementById("WBS_toolbar");
    toolbar.style.setProperty("display", "grid");
    toolbox = document.getElementById("WBS_toolbar_toolbox");
    toolbar_status = document.getElementById("WBS_toolbar_status").querySelector(".text");

    // 保存ボタン
    const saveButton = makeSaveButton();
    toolbox.appendChild(saveButton);
    // タスク追加ボタン
    const addTaskButton = makeAddButton();
    toolbox.appendChild(addTaskButton);
}
const resetToolbar = () => {
    // toolbar非表示
    toolbar.style.setProperty("display", null);
    // 追加したボタンを削除
    toolbox.innerHTML = "";

    setWbsChanged(false);
}

const addEditable = () => {
    // 
    addFunctionEditable(wbs);
}
const addFunctionEditable = (parent) => {
    const rows = parent.getElementsByClassName("function");
    // functionの前にインジケーターを追加
    for (let id = 0; id < rows.length; id++) {
        const row = rows[id];
        // functionの変更対象
        const name = row.querySelector(".caption .work_name .text");
        applyEditable(name, null);

        // phaseに適用
        addPhaseEditable(row);
    }
}
const addPhaseEditable = (parent) => {
    const rows = parent.getElementsByClassName("phase");
    // phaseの前にインジケーターを追加
    for (let id = 0; id < rows.length; id++) {
        const row = rows[id];
        // phaseの変更対象
        const name = row.querySelector(".caption .work_name .text");
        applyEditable(name, null);

        // work_packageに適用
        addWorkEditable(row);
    }
}
const addWorkEditable = (parent) => {
    const rows = parent.getElementsByClassName("work_package");
    for (let id = 0; id < rows.length; id++) {
        const row = rows[id];
        // work_packageの変更対象
        const name = row.querySelector(".caption .work_name .text");
        applyEditable(name, null);
        const owner = row.querySelector(".caption .work_owner .text");
        applyEditable(owner, null);
        const plan_date_begin = row.querySelector(".caption .work_plan_date_begin .text");
        applyEditable(plan_date_begin, null);
        const plan_date_end = row.querySelector(".caption .work_plan_date_end .text");
        applyEditable(plan_date_end, null);
        const actual_date_begin = row.querySelector(".caption .work_actual_date_begin .text");
        applyEditable(actual_date_begin, null);
        const actual_date_end = row.querySelector(".caption .work_actual_date_end .text");
        applyEditable(actual_date_end, null);
        const plan_man_hour = row.querySelector(".caption .work_plan_man_hour .text");
        applyEditable(plan_man_hour, null);
        const actual_man_hour = row.querySelector(".caption .work_actual_man_hour .text");
        applyEditable(actual_man_hour, null);
        const progress = row.querySelector(".caption .work_progress .value");
        applyEditable(progress, null);

        // taskに適用
        addTaskEditable(row);
    }
}
const addTaskEditable = (parent) => {
    const rows = parent.getElementsByClassName("task");
    for (let id = 0; id < rows.length; id++) {
        const row = rows[id];
        // taskの変更対象
        const name = row.querySelector(".work_name .text");
        applyEditable(name, null);
        const owner = row.querySelector(".work_owner .text");
        applyEditable(owner, null);
        const plan_date_begin = row.querySelector(".work_plan_date_begin .text");
        applyEditable(plan_date_begin, null);
        const plan_date_end = row.querySelector(".work_plan_date_end .text");
        applyEditable(plan_date_end, null);
        const actual_date_begin = row.querySelector(".work_actual_date_begin .text");
        applyEditable(actual_date_begin, null);
        const actual_date_end = row.querySelector(".work_actual_date_end .text");
        applyEditable(actual_date_end, null);
        const plan_man_hour = row.querySelector(".work_plan_man_hour .text");
        applyEditable(plan_man_hour, null);
        const actual_man_hour = row.querySelector(".work_actual_man_hour .text");
        applyEditable(actual_man_hour, null);
        const progress = row.querySelector(".work_progress .value");
        applyEditable(progress, null);
    }
}

const applyEditable = (elem, validator) => {
    elem.addEventListener("click", (e) => {
       // クリックしたらinputで編集可能にする
        if (elem.dataset.value === undefined || elem.dataset.value === "") {
            const input = document.createElement("input");
            input.type = "text";
            input.value = elem.innerText;
            elem.dataset.value = elem.innerText;
            elem.innerText = "";
            elem.appendChild(input);
            input.focus();
            input.addEventListener("blur", () => {
                if (validator) {
                    // バリデーションチェック
                    if (validator(input.value) === false) {
                        alert("無効な値です。");
                        input.value = elem.dataset.value;
                    }
                }
                if (elem.dataset.value !== input.value) {
                    setWbsChanged(true);
                }
                elem.dataset.value = "";
                elem.innerText = input.value;
            });
            input.addEventListener("keydown", (e) => {
                if (e.key === "Enter") {
                    input.blur();
                } else if (e.key === "Escape") {
                    input.value = elem.dataset.value;
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
            if (insert_mode === true) {
                changeInsertMode();
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

const makeAddButton = () => {
    let button = document.createElement("button");
    button.innerText = "要素追加";
    button.addEventListener("click", () => {
        changeInsertMode();
        //
        if (insert_mode === true) {
            // ボタンの表示を元に戻す
            button.innerText = "要素追加";
        } else {
            // ボタンの表示を変更する
            button.innerText = "要素追加終了";
        }
    });

    return button;
}

const changeInsertMode = () => {
    if (insert_mode === true) {
        // 挿入モードのとき、
        // インジケーターを削除して終了
        removeInsertIndicator();
        insert_mode = false;
    } else {
        // 挿入モード以外のとき
        // 挿入モードを設定する
        addInsertIndicator();
        insert_mode = true;
    }
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
    addFunctionInsertIndicator(wbs);
}

const addFunctionInsertIndicator = (parent) => {
    // Function要素に対して適用する処理
    addInsertIndicatorImpl(parent, "function", makeFunctionInsertIndicator, (func) => {
        // Function要素に対して適用する処理
        addPhaseInsertIndicator(func);
    });
}
const addPhaseInsertIndicator = (parent) => {
    // Phase要素に対して適用する処理
    addInsertIndicatorImpl(parent, "phase", makePhaseInsertIndicator, (phase) => {
        // Phase要素に対して適用する処理
        addWorkInsertIndicator(phase);
    });
}
const addWorkInsertIndicator = (parent) => {
    // Work要素に対して適用する処理
    addInsertIndicatorImpl(parent, "work_package", makeWorkInsertIndicator, (work) => {
        // Work要素に対して適用する処理
        addTaskInsertIndicator(work);
    });
}
const addTaskInsertIndicator = (parent) => {
    // Work要素に対して適用する処理
    addInsertIndicatorImpl(parent, "task", makeTaskInsertIndicator, () => {
        // Task要素に対して適用する処理
        // ここでは何もしない
    });
}

const addInsertIndicatorImpl = (parent, cls_name, procInsertIndicator, proc) => {
    // 機能挿入インジケーターを追加する
    // 親要素単位で処理する
    // class="function"の前後に追加可能
    const rows = parent.getElementsByClassName(cls_name);
    // functionの前にインジケーターを追加
    for (let id = 0; id < rows.length; id++) {
        const row = rows[id];
        const indicator = procInsertIndicator(parent);
        // 前にインジケーターを追加
        row.before(indicator);

        //
        proc(row);
    }
    // functionのリストを格納する要素の末尾にインジケーターを追加
    // functionが空の場合も末尾に追加できる
    const row = parent.querySelector(".items");
    const indicator = procInsertIndicator(parent);
    // 前にインジケーターを追加
    row.appendChild(indicator);
}



const makeInsertIndicator = (parent, label, color, template, procInsertIndicator, procUpdateId) => {
    const indicator = insert_indicator.cloneNode(true);
    indicator.id = null;
    indicator.style.setProperty("background-color", color);
    indicator.innerText = label;
    indicator.addEventListener("click", () => {
        // インジケータークリック:挿入処理
        // 挿入要素をコピーして作成
        const elem = template.cloneNode(true);
        // インジケーターを適用
        procInsertIndicator(elem);
        // インジケーターの後ろに新しい要素を追加する
        indicator.after(elem);
        //
        const new_indicator = makeInsertIndicator(parent, label, color, template, procInsertIndicator, procUpdateId);
        elem.after(new_indicator);
        // 機能IDを更新する
        procUpdateId(parent);
        // WBSが変更されたフラグを立てる
        setWbsChanged(true);
    });
    // 追加したインジケーターをリストに追加
    insert_indicator_list.push(indicator);
    return indicator;
}

const makeFunctionInsertIndicator = (parent) => {
    return makeInsertIndicator(parent, "<<Function挿入>>", "rgb(84, 215, 255)", wbs_template_func, addPhaseInsertIndicator, updateFuncId);
}
const makePhaseInsertIndicator = (parent) => {
    return makeInsertIndicator(parent, "<<Phase挿入>>", "rgb(210, 255, 87)", wbs_template_phase, addWorkInsertIndicator, updatePhaseId);
}
const makeWorkInsertIndicator = (parent) => {
    return makeInsertIndicator(parent, "<<Work挿入>>", "rgb(255, 220, 63)", wbs_template_work, addTaskInsertIndicator, updateWorkId);
}
const makeTaskInsertIndicator = (parent) => {
    return makeInsertIndicator(parent, "<<Task挿入>>", "rgb(255, 160, 35)", wbs_template_task, () => { }, updateTaskId);
}


const updateFuncId = (parent) => {
    // 機能IDを更新する処理
    const rows = parent.getElementsByClassName("function");
    for (let i=0; i < rows.length; i++) {
        const row = rows[i];
        const id = i + 1;
        // data更新
        row.dataset.func_id = id;
        //
        const func_id = row.querySelector(".work_id .text");
        func_id.innerText = row.dataset.func_id;

        // フェーズIDを更新する
        updatePhaseId(row);
    }
}
const updatePhaseId = (parent) => {
    // フェーズIDを更新する処理
    const rows = parent.getElementsByClassName("phase");
    for (let i=0; i < rows.length; i++) {
        const row = rows[i];
        const id = i + 1;
        // data更新
        row.dataset.func_id = parent.dataset.func_id;
        row.dataset.phase_id = id;
        //
        const phase_id = row.querySelector(".work_id .text");
        phase_id.innerText = row.dataset.func_id + "-" + id;

        // ワークパッケージIDを更新する
        updateWorkId(row);
    }
}
const updateWorkId = (parent) => {
    // ワークパッケージIDを更新する処理
    const rows = parent.getElementsByClassName("work_package");
    for (let i=0; i < rows.length; i++) {
        const row = rows[i];
        const id = i + 1;
        // data更新
        row.dataset.func_id = parent.dataset.func_id;
        row.dataset.phase_id = parent.dataset.phase_id;
        row.dataset.work_id = id;
        //
        const work_id = row.querySelector(".work_id .text");
        work_id.innerText = row.dataset.func_id + "-" + row.dataset.phase_id + "-" + id;

        // タスクIDを更新する
        updateTaskId(row);
    }
}
const updateTaskId = (parent) => {
    // タスクIDを更新する処理
    const rows = parent.getElementsByClassName("task");
    for (let i=0; i < rows.length; i++) {
        const row = rows[i];
        const id = i + 1;
        // data更新
        row.dataset.func_id = parent.dataset.func_id;
        row.dataset.phase_id = parent.dataset.phase_id;
        row.dataset.work_id = parent.dataset.work_id;
        row.dataset.task_id = id;
        //
        const task_id = row.querySelector(".work_id .text");
        task_id.innerText = row.dataset.func_id + "-" + row.dataset.phase_id + "-" + row.dataset.work_id + "-" + id;
    }
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
