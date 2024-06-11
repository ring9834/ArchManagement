function HM() { }

HM.dealwin = function (c, w, h, t) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#5bb7e7;font-size:15px;padding:15px 0 0 10px;'> " + t + "</i>",
        content: c,
        buttonSpcl: "min|max|close",
        anim: "fadeIn-zoom",
        width: w,
        height: h,
        sizeAdapt: false,
        id: "deal-win",
        place: 5,
        drag: true,
        dragSize: true,
        index: true,
        toClose: false,
        mask: true,
        class: false,
    });
}

HM.dealwinWithId = function (c, w, h, t, winid) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#5bb7e7;font-size:15px;padding:15px 0 0 10px;'> " + t + "</i>",
        content: c,
        buttonSpcl: "min|max|close",
        anim: "fadeIn-zoom",
        width: w,
        height: h,
        sizeAdapt: false,
        id: "deal-win-" + winid,
        place: 5,
        drag: true,
        dragSize: true,
        index: true,
        toClose: false,
        mask: true,
        class: false,
    });
}

//利用登记时选择窗口专用
HM.dealwinPickTb = function (c, w, h, t) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#5bb7e7;font-size:15px;padding:15px 0 0 10px;'> " + t + "</i>",
        content: c,
        buttonSpcl: "min|max|close",
        anim: "fadeIn-zoom",
        width: w,
        height: h,
        sizeAdapt: false,
        id: "deal-win-pt",
        place: 5,
        drag: true,
        dragSize: true,
        index: true,
        toClose: false,
        mask: true,
        class: false,
    });
}

HM.alertWin = function (t) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#e40303;font-size:15px;padding:15px 0 0 10px;'> 敬告</i>",
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-times-circle' style='color:#e40303;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        box: "body",
        sizeAdapt: false,
        button: ["danger", "我知道了", function (e) {
            pop.close(e);
        }],
        buttonSpcl: "close",
        anim: "roll",
        width: 500,
        height: 200,
        id: "alert-win",
        place: 5,
        drag: false,
        dragSize: false,
        index: false,
        toClose: false,
        mask: true,
        class: false
    });
}

HM.alertWinCallBack = function (t,callback) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#e40303;font-size:15px;padding:15px 0 0 10px;'> 敬告</i>",
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-times-circle' style='color:#e40303;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        box: "body",
        sizeAdapt: false,
        button: ["danger", "我知道了", function (e) {
            callback();
            pop.close(e);
        }],
        buttonSpcl: "close",
        anim: "roll",
        width: 500,
        height: 200,
        id: "alert-win",
        place: 5,
        drag: false,
        dragSize: false,
        index: false,
        toClose: false,
        mask: true,
        class: false
    });
}

HM.hintWin = function (t) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#5bb7e7;font-size:15px;padding:15px 0 0 10px;'> 消息提示</i>",
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-exclamation-circle' style='color:#5bb7e7;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        box: "body",
        sizeAdapt: false,
        button: ["info", "我知道了", function (e) {
            pop.close(e);
        }],
        buttonSpcl: "close",
        anim: "roll",
        width: 500,
        height: 200,
        id: "hint-win",
        place: 5,
        drag: false,
        dragSize: false,
        index: false,
        toClose: false,
        mask: true,
        class: false
    });
}

HM.confirmDelWin = function (id, t, func) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#f0b308;font-size:15px;padding:15px 0 0 10px;'> 确认提示</i>",
        sizeAdapt: false,
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-exclamation-triangle' style='color:#f0b308;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        button: [["warning", "删除",
            function () {
                func(id);
            }], ["default", "取消",
            function (e) {
                pop.close(e);
            }]],
        buttonSpcl: "",
        anim: "fadeIn-zoom",
        width: 450,
        height: 200,
        id: "confirm-win",
        place: 5,
        drag: true,
        index: true,
        toClose: false,
        mask: true,
        class: false
    });
}

//统计功能专用
HM.confirmDelWintwo = function (typeid, id, t, bt, winid, func) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#f0b308;font-size:15px;padding:15px 0 0 10px;'> 确认提示</i>",
        sizeAdapt: false,
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-exclamation-triangle' style='color:#f0b308;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        button: [["warning", bt,
            function () {
                func(typeid, id);
            }], ["default", "取消",
            function (e) {
                pop.close(e);
            }]],
        buttonSpcl: "",
        anim: "fadeIn-zoom",
        width: 450,
        height: 200,
        id: "confirm-win_" + winid,
        place: 5,
        drag: true,
        index: true,
        toClose: false,
        mask: true,
        class: false
    });
}

//档案树专用，table:删除的节点对应的数据库表名
HM.confirmDelTreeWin = function (id, t, table, func) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#f0b308;font-size:15px;padding:15px 0 0 10px;'> 确认提示</i>",
        sizeAdapt: false,
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-exclamation-triangle' style='color:#f0b308;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        button: [["warning", "删除",
            function () {
                func(id, table);
            }], ["default", "取消",
            function (e) {
                pop.close(e);
            }]],
        buttonSpcl: "",
        anim: "fadeIn-zoom",
        width: 450,
        height: 200,
        id: "confirm-win2",
        place: 5,
        drag: true,
        index: true,
        toClose: false,
        mask: true,
        class: false
    });
}

HM.confirmWin = function (id, t, bt, winid, func) {
    pop.custom({
        title: "<i class='fa fa-info-circle' style='color:#f0b308;font-size:15px;padding:15px 0 0 10px;'> 确认提示</i>",
        sizeAdapt: false,
        content: "<table style='height:100%;width:calc(100% - 20px);margin:0 10px 0px 10px;'><tr><td style='width:58px;'><i class='fa fa-exclamation-triangle' style='color:#f0b308;font-size:58px; float:left;'></i></td><td style='height:100%;vertical-align:middle;'><span style='font-size:15px;'>" + t + "</span></td></tr></table> ",
        button: [["warning", bt,
            function () {
                func(id);
            }], ["default", "取消",
            function (e) {
                pop.close(e);
            }]],
        buttonSpcl: "",
        anim: "fadeIn-zoom",
        width: 450,
        height: 200,
        id: "confirm-win_" + winid,
        place: 5,
        drag: true,
        index: true,
        toClose: false,
        mask: true,
        class: false
    });
}

HM.pageLoading = function () {
    if (document.getElementById("pageLoaderWin") === null || document.getElementById("pageLoaderWin") === undefined) {
        pop.news({
            content: "<div style='width:100%;height:100%;margin-top:10px;'><i class='fa fa-2x fa-refresh fa-spin'></i></div>",
            id: "pageLoaderWin",
            place: 5,
            class: false,
            time: 100000,
            anim: "fadeIn-zoom",
            box: "body",
            only: true
        });

        $("#pageLoaderWin").removeClass('pop');
        $("#pageLoaderWin").addClass('pop-page-loader');
    }
}

HM.closePageLoading = function () {
    if (document.getElementById("pageLoaderWin") !== null) {
        pop.close("pageLoaderWin");
    }
}
