function hTabControl(top) {
    var tabtopdiv = top;
    var tabarray = [];
    this.IsFirstTabClosible = true;//属性，决定第一个tab上是否有关闭按钮，默认有
    var forwardStep = 50;//点击滚动步长
    var longforwardStep = 200;//长按滚动步长
    var addordel = true;//是在增加tab或减去tab的开关 true为增加，false为正在关闭tab
    addWrappers();//初始化一些包裹tabbars和contents的静态div

    //增加tab
    this.AddTab = function (tabid, title, contenturl) {//分别传ID,显示名称，内容url
        var tab = $("#tabBar" + tabid);
        //当tabbar不存在时，增加这个tabbar
        if (tab.length == 0) {
            var tabnow = {
                "id": tabid,
                "active": "true"
            };
            tabarray.push(tabnow);//记住每个创建的tabbar及其按下状态

            if (tabarray.length == 1 && !this.IsFirstTabClosible) {
                //在控件顶部增加tab
                $("#tabbarsWrapper_scroll").append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + title + "</div></div></div>");
            }
            else {
                //在控件顶部增加tab,带关闭按钮
                $("#tabbarsWrapper_scroll").append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + title + "</div><div class='tab-close-div'><a href='#' id='tabclosea_" + tabid + "' class='tab-close-button'><i class='fa fa-close'></i></a></div></div></div>");
            }

            //增加内容div
            $("#tabstripContent").append("<div class='tab-content-div' id='tabContent" + tabid + "'><iframe src='" + contenturl + "' class='tab-content-iframes' frameborder='0'></iframe></div>");

            addClickEventToTabbar(tabid);
            addClickEventToCloseButton(tabid);
            changeStyleofSelected(tabid);
            showForwardTools();
        }
        else {//当tabbar存在时，仅激活使其显示即可
            changeStyleofSelected(tabid);
        }
        addordel = true;
    }

    function addWrappers() {
        $("#" + tabtopdiv).append("<div id='tabstripTopWrapper' class='tabstrip-top-wrapper'><div id='tabstripLeftForward' class='tabstrip-left-forward'><i class='fa fa-angle-left'></i></div><div id='tabbarsWrapper' class='tabbars-wrapper'><div id='tabbarsWrapper_scroll' class='tabbars-wrapper-scroll'></div></div><div id='tabstripRightForward' class='tabstrip-right-forward'><i class='fa fa-angle-right'></i></div></div>");
        $("#" + tabtopdiv).parent().append("<div id='tabstripContent' class='tabstrip-content-container'></div> \r\n");
        $("#tabstripLeftForward").bind("click", function () { leftForward(forwardStep); });
        $("#tabstripRightForward").bind("click", function () { rightForward(forwardStep); });
        $("#tabstripLeftForward").bind("mousedown", function () { leftForwardMouseDown(); });
        $("#tabstripLeftForward").bind("mouseup", function () { offMultiForward(); });
        $("#tabstripLeftForward").bind("mouseout", function () { offMultiForward(); });
        $("#tabstripRightForward").bind("mousedown", function () { rightForwardMouseDown(); });
        $("#tabstripRightForward").bind("mouseup", function () { offMultiForward(); });
        $("#tabstripRightForward").bind("mouseout", function () { offMultiForward(); });
        $("#" + tabtopdiv).resize(function () { winResize(); });
    }

    //窗口尺寸发生改变时
    var winResize = function () {
        showForwardTools();
    }

    //tabs左滚
    var leftForward = function (fw) {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        if (iLeft < 0) {
            iLeft += fw;
            if (iLeft > 0) {
                iLeft = 0;
            }
            $("#tabbarsWrapper_scroll").animate({ left: iLeft + "px" });
        }
    }

    //tabs右滚
    var rightForward = function (fw) {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        var n = Math.abs(iLeft) + tabbarsWrapperWidth;
        if (n < totalWidth) {
            var toLeft = iLeft - fw;
            if (n > totalWidth) {
                toLeft = tabbarsWrapperWidth - totalWidth;
            }
            $("#tabbarsWrapper_scroll").animate({ left: toLeft + "px" });
        }
    }

    //tabs长按左滚
    var leftForward2 = function () {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        if (iLeft < 0) {
            iLeft += longforwardStep;
            if (iLeft > 0) {
                iLeft = 0;
            }
            $("#tabbarsWrapper_scroll").animate({ left: iLeft + "px" });
        }
    }

    //tabs长按右滚
    var rightForward2 = function () {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        var n = Math.abs(iLeft) + tabbarsWrapperWidth;
        if (n < totalWidth) {
            var toLeft = iLeft - longforwardStep;
            if (n > totalWidth) {
                toLeft = tabbarsWrapperWidth - totalWidth;
            }
            $("#tabbarsWrapper_scroll").animate({ left: toLeft + "px" });
        }
    }

    var multact;
    var timeout;
    var leftForwardMouseDown = function () {
        timeout = setTimeout(function () {
            multiLeftWard();
        }, 200);//鼠标按下0.5秒后发生multiLeftWard事件
    }

    var rightForwardMouseDown = function () {
        timeout = setTimeout(function () {
            multiRightWard();
        }, 200);//鼠标按下0.5秒后发生multiLeftWard事件
    }

    var offMultiForward = function () {
        window.clearInterval(multact);
        clearTimeout(timeout);//清理掉定时器
    }

    function multiLeftWard() {
        multact = setInterval(leftForward2, 500);//0.5秒执行1次
    }

    function multiRightWard() {
        multact = setInterval(rightForward2, 500);//0.5秒执行1次
    }

    //显示后隐藏两个前、后按钮;并且，当显示前后滚动按钮时，让tabs集体左滚，当滚动按钮隐藏时，让tabs集体右滚返回原来位置
    function showForwardTools() {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }

        if (totalWidth > tabbarsWrapperWidth) {
            $("#tabbarsWrapper_scroll").width(totalWidth);
            if (addordel) {//增加tab时
                var balanceToLeft = tabbarsWrapperWidth - totalWidth;//当没有滚动到最左边，直接滚动到最左边
                $("#tabbarsWrapper_scroll").animate({ left: balanceToLeft + "px" });
            }
            else {//关闭tab时
                var iLeft = $("#tabbarsWrapper_scroll").position().left;
                var b = totalWidth - Math.abs(iLeft);
                if (b <= tabbarsWrapperWidth) {
                    var balanceToLeft = tabbarsWrapperWidth - totalWidth;//当没有滚动到最左边，直接滚动到最左边
                    $("#tabbarsWrapper_scroll").animate({ left: balanceToLeft + "px" });
                }
            }
            //显示前、后滚动按钮
            $("#tabstripLeftForward").attr("class", "tabstrip-left-forward");
            $("#tabstripRightForward").attr("class", "tabstrip-right-forward");
        }
        else {
            if (!addordel) {//关闭tab时
                $("#tabbarsWrapper_scroll").width(totalWidth);
            }

            $("#tabbarsWrapper_scroll").animate({ left: "0" });

            //隐藏前、后滚动按钮
            $("#tabstripLeftForward").attr("class", "tabstrip-left-forward-hide");
            $("#tabstripRightForward").attr("class", "tabstrip-right-forward-hide");
        }
    }

    //在每个tabbar上增加鼠标点击事件
    function addClickEventToTabbar(tabid) {
        $("#tabBar" + tabid).click(function () {
            changeStyleofSelected(tabid);
        })
    }

    //改变正处于激活状态的tab的style
    var changeStyleofSelected = function (tabid) {
        $("#tabbarsWrapper_scroll>div").removeClass("tabbar-down");
        $("#tabBar" + tabid).addClass("tabbar-down");
        $("#tabstripContent>div").removeClass("tab-content-div-down");
        $("#tabstripContent>div").addClass("tab-content-div");
        $("#tabContent" + tabid).removeClass("tab-content-div");
        $("#tabContent" + tabid).addClass("tab-content-div-down");

        //所有的tabbar中只有一个处于激活状态，那就是tabid这个
        for (var i = 0; i < tabarray.length; i++) {
            if (tabarray[i]["id"] != tabid) {
                tabarray[i]["active"] = "false";//其他的都处于非激活（点击）状态
            }
            else {
                tabarray[i]["active"] = "true";
            }
        }

        //var bwidth = $("#tabBar" + tabid).width();
        //var bleft = $("#tabBar" + tabid).css("left");
        ////alert(bleft);
        //$("#dynamicHintBar").width(bwidth);
        //$("#dynamicHintBar").animate({ left: bleft });
    }

    //在每个tabbar上的关闭按钮上增加鼠标点击事件
    function addClickEventToCloseButton(tabid) {
        $("#tabclosea_" + tabid).bind('click', function (event) {
            for (var i = 0; i < tabarray.length; i++) {
                if (tabarray[i]["id"] == tabid) {
                    if (tabarray[i]["active"] == "true") {
                        if (i == tabarray.length - 1 && i - 1 >= 0) {//位置最后一个tab,且前面还有tab
                            changeStyleofSelected(tabarray[i - 1]["id"]);//激活前面那个tab
                        }
                        else if (i < tabarray.length - 1) {
                            changeStyleofSelected(tabarray[tabarray.length - 1]["id"]);//激活最后一个tab
                        }
                    }


                    var scrollwidth = $("#tabbarsWrapper_scroll").width();//减去已关闭tab的长度
                    var tabwidth = $("#tabBar" + tabid).width();
                    $("#tabbarsWrapper_scroll").width(scrollwidth - tabwidth);
                    tabarray.splice(i, 1);//删除数组中tabid对应的元素                 
                    break;
                }
            }
            $("#tabBar" + tabid).remove();
            $("#tabContent" + tabid).remove();
            addordel = false
            if (tabarray.length == 0)//全部关闭后，清除style属性，最主要的是清除width属性
            {
                $("#tabbarsWrapper_scroll").removeAttr("style");//使tabbarsWrapper_scroll还原为width:auto
                addordel = true;//设置为增加tab的状态，以阻止tabbarsWrapper_scroll的width属性被设置
            }

            showForwardTools();//更新前后滚动按钮和tabs位置状态（滚动）
            event.stopPropagation();//阻止事件冒泡，即，阻止上层元素的click事件触发
        })
    }
}