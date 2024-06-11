function hTabControl(divname, tabId, tabcount) {
    $("#" + divname).addClass("tabstrip-wrapper ");
    //创建tab列容器
    $("#" + divname).append("<div class='tabstrip-top-container' id='tabstripTop" + tabId + "'></div>");
    //创建内容容器
    $("#" + divname).append("<div class='tabstrip-content-container' id='tabstripContent" + tabId + "'></div>");

    this.AddTab = function (tabid, tabtitle, tabcontent, openor) {//分别传ID,显示名称，内容，是否初始显示
        $("#tabstripTop" + tabId).append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + tabtitle + "</div><div class='tab-close-div'><a href='#' class='tab-close-button'><i class='fa fa-close'></i></a></div></div></div>");
        $("#tabstripContent" + tabId).append("<div class='tab-content-div' id='tabContent" + tabid + "'>" + tabcontent + "</div>");
        tab_Click(tabid); //点击或移过
        //tab_Mouseover(tabid);
        if (openor == true) {
            $("#tabBar" + tabid).addClass("tabbar-down");
            $("#tabContent" + tabid).removeClass("tab-content-div");
            $("#tabContent" + tabid).addClass("tab-content-div-down");
        }
    }

    function tab_Click(tabid) {//点击
        $("#tabBar" + tabid).click(function () {
            $("#tabstripTop" + tabId + ">div").removeClass("tabbar-down");
            $("#tabBar" + tabid).addClass("tabbar-down");
            $("#tabstripContent" + tabId + ">div").removeClass("tab-content-div-down");
            $("#tabstripContent" + tabId + ">div").addClass("tab-content-div");
            $("#tabContent" + tabid).removeClass("tab-content-div");
            $("#tabContent" + tabid).addClass("tab-content-div-down");
        })
    }

    function tab_Mouseover(tabid) {//移过
        $("#tabBar" + tabid).mouseover(function () {
            $("#tabstripTop" + tabId + ">div").removeClass("tabbar-down");
            $("#tabBar" + tabid).addClass("tabbar-down");
            $("#tabstripContent" + tabId + ">div").removeClass("tab-content-div-down");
            $("#tabstripContent" + tabId + ">div").addClass("tab-content-div");
            $("#tabContent" + tabid).removeClass("tab-content-div");
            $("#tabContent" + tabid).addClass("tab-content-div-down");
        })
    }

}