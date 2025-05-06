
var toolbar = null;

// WBSテンプレート参照
var wbs_template = null; // テンプレート
var wbs_template_task = null; // タスクテンプレート

// 挿入処理変数
const INSERTMODE_NONE = 0; // 挿入モードなし
const INSERTMODE_TASK = 1; // タスク挿入モード
var insert_mode = INSERTMODE_NONE;
var insert_indicator = null;
var insert_indicator_list = []; // 表示用に追加したインジケーターのリスト, 後で削除するために使用

document.addEventListener("DOMContentLoaded", function() {
    // toolbar表示
    toolbar = document.getElementById("WBS_toolbar");
    toolbar.style.setProperty("display", "block");

    // 挿入位置インジケーター参照取得
    insert_indicator = document.getElementById("work_insert_indicator");
    // テンプレート参照取得
    wbs_template = document.getElementById("WBS_template");
    wbs_template_task = wbs_template.querySelector(".task");

    // 操作ボタン追加
    // タスク追加ボタン
    const addTaskButton = makeTaskAddButton();
    toolbar.appendChild(addTaskButton);

});

// F5キーによるreloadを禁止
document.addEventListener("keydown", function (e) {
    // deprecated: (e.which || e.keyCode) == 116
    if (e.key === 'F5') {
        var result = window.confirm('よろしいですか？');
        if (result === true) {
        } else {
            e.preventDefault();
        }
    }
});


const makeTaskAddButton = () => {
    var button = document.createElement("button");
    button.innerText = "タスク追加";
    button.addEventListener("click", function () {
        if (insert_mode === INSERTMODE_TASK) {
            // タスク挿入モードのとき、インジケーターを削除して終了
            removeInsertIndicator();
            insert_mode = INSERTMODE_NONE;
        } else {
            // タスク挿入モード以外のとき
            // NONE以外なら一度インジケーターを削除
            if (insert_mode !== INSERTMODE_NONE) {
                removeInsertIndicator();
            }
            // タスク挿入モードを設定する
            addInsertIndicator(INSERTMODE_TASK);

            insert_mode = INSERTMODE_TASK;
        }
    });

    return button;
}

const removeInsertIndicator = () => {
    // インジケーターを削除する処理
    insert_indicator_list.forEach(indicator => {
        indicator.remove();
    });
    insert_indicator_list = [];
}

const addInsertIndicator = (mode) => {
    let id = 0;

    const wbs = document.getElementById("WBS");

    switch (mode) {
        case INSERTMODE_TASK:
            // タスク挿入モード
            // 親要素単位で処理する
            const parent_rows = wbs.getElementsByClassName("phase");
            for (let i=0; i < parent_rows.length; i++) {
                const parent = parent_rows[i];
                // class="task"の前後に追加可能
                const rows = parent.getElementsByClassName("task");
                // taskの前にインジケーターを追加
                for (id = 0; id < rows.length; id++) {
                    const row = rows[id];
                    const indicator = makeTaskInsertIndicator();
                    // 前にインジケーターを追加
                    row.before(indicator);
                }
                // 最後のtaskの後にインジケーターを追加
                const row = rows[id - 1];
                const indicator = makeTaskInsertIndicator();
                // 前にインジケーターを追加
                row.after(indicator);
            }
            break;
        default:
            break;
    }
}

const makeTaskInsertIndicator = () => {
    const indicator = insert_indicator.cloneNode(true);
    indicator.id = null;
    indicator.innerText = "<<タスク挿入>>";
    indicator.addEventListener("click", () => {
        // インジケータークリック:タスク挿入処理
        // インジケーターの後ろに新しいタスクを追加する
        const elem = wbs_template_task.cloneNode(true);
        indicator.after(elem);
        //
        const new_indicator = makeTaskInsertIndicator();
        elem.after(new_indicator);
    });
    // 追加したインジケーターをリストに追加
    insert_indicator_list.push(indicator);
    return indicator;
}
