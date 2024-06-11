
function hTabControl() {
    //var tabtopdiv = top;
    var tabarray = [];
    this.IsFirstTabClosible = true;//属性，决定第一个tab上是否有关闭按钮，默认有
    var forwardStep = 50;//点击滚动步长
    var longforwardStep = 200;//长按滚动步长
    var addordel = true;//是在增加tab或减去tab的开关 true为增加，false为正在关闭tab
    var tabsTotalWidth = 0;//增加或删除tab后需要更新的值
    var tabsTotalWidthBeforeActive = 0;//激活tab之前的tabs的总宽度
    var activeTabId;//当前处于激活状态的tabid
    addWrappers();//初始化一些包裹tabbars和contents的静态div
    var callbackFunc;


    //增加tab
    this.AddTab = function (tabid, title, contenturl, needupdate, callback) {//分别传ID,显示名称，内容url
        callbackFunc = callback;
        var tab = $("#tabBar" + tabid);
        //当tabbar不存在时，增加这个tabbar
        if (tab.length == 0) {
            var tabnow = {
                "id": tabid,
                "active": "true",
                "url": contenturl,
                "title": title,
                "needupdate": needupdate
            };
            tabarray.push(tabnow);//记住每个创建的tabbar及其按下状态

            if (tabarray.length == 1 && !this.IsFirstTabClosible) {
                //在控件顶部增加tab
                $("#tabbarsWrapper_scroll").append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + title + "</div></div></div>");
            }
            else {
                //在控件顶部增加tab,带关闭按钮
                $("#tabbarsWrapper_scroll").append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + title + "</div><div id='tabclosea_" + tabid + "' class='tab-close-div'><i class='fa fa-times-circle-o' title='点击关闭'></i></div></div></div>");
            }

            var ifr = $("<iframe id='iframe_" + tabid + "' src='" + contenturl + "' class='tab-content-iframes' frameborder='0'></iframe>");
            //增加内容div
            $("#tabstripContent").append("<div class='tab-content-div' id='tabContent" + tabid + "'>" + ifr.prop("outerHTML") + "</div>");
            //$("#tabstripContent").append("<div class='tab-content-div' id='tabContent" + tabid + "'><iframe id='iframe_'" + tabid + " src='" + contenturl + "' class='tab-content-iframes' frameborder='0'></iframe></div>");

            //显示tab动态指示条
            //$("#dynamicHintBar").css("display", "block");
            addClickEventToTabbar(tabid, contenturl, needupdate);
            addClickEventToCloseButton(tabid);
            updateTabsTotalWidth();//更新全局totalWidth
            changeStyleofSelected(tabid);
            showForwardTools();
            //changeDynamicHint(tabid);

        }
        else {//当tabbar存在时，仅激活使其显示即可
            changeStyleofSelected(tabid);
            showForwardTools();
            //changeDynamicHint(tabid);

            //更新iframe中的内容(依赖具体数据库表格时才更新)
            if (needupdate == true) {
                var frame = $("#iframe_" + tabid);
                if (frame.length > 0) {
                    frame.attr("src", contenturl);
                }
            }
        }
        addordel = true;
    }

    function addWrappers() {
        $("#tabstripLeftForward").bind("click", function () { leftForward(forwardStep); });
        $("#tabstripRightForward").bind("click", function () { rightForward(forwardStep); });
        $("#tabstripLeftForward").bind("mousedown", function () { leftForwardMouseDown(); });
        $("#tabstripLeftForward").bind("mouseup", function () { offMultiForward(); });
        $("#tabstripLeftForward").bind("mouseout", function () { offMultiForward(); });
        $("#tabstripRightForward").bind("mousedown", function () { rightForwardMouseDown(); });
        $("#tabstripRightForward").bind("mouseup", function () { offMultiForward(); });
        $("#tabstripRightForward").bind("mouseout", function () { offMultiForward(); });
        $("#tabstripTop").resize(function () { winResize(); });
    }

    //窗口尺寸发生改变时
    var winResize = function () {
        showForwardTools();
        //changeDynamicHint(null);//自动寻找已激活的tab，然后改变动态条的状态
    }

    //tabs左滚
    var leftForward = function (fw) {
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        if (iLeft < 0) {
            iLeft += fw;
            if (iLeft > 0) {
                iLeft = 0;
            }
            $("#tabbarsWrapper_scroll").animate({ left: iLeft + "px" }, 180, function () { changeDynamicHint(); });
        }
    }

    //tabs右滚
    var rightForward = function (fw) {
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        var n = Math.abs(iLeft) + tabbarsWrapperWidth;
        if (n < tabsTotalWidth) {
            var toLeft = iLeft - fw;
            if (n > tabsTotalWidth) {
                toLeft = tabbarsWrapperWidth - tabsTotalWidth;
            }
            $("#tabbarsWrapper_scroll").animate({ left: toLeft + "px" }, 180, function () { changeDynamicHint(); });
        }
    }

    //tabs长按左滚
    var leftForward2 = function () {
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        if (iLeft < 0) {
            iLeft += longforwardStep;
            if (iLeft > 0) {
                iLeft = 0;
            }
            $("#tabbarsWrapper_scroll").animate({ left: iLeft + "px" }, 180, function () { changeDynamicHint(); });
        }
    }

    //tabs长按右滚
    var rightForward2 = function () {
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();

        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        var n = Math.abs(iLeft) + tabbarsWrapperWidth;
        if (n < tabsTotalWidth) {
            var toLeft = iLeft - longforwardStep;
            if (n > tabsTotalWidth) {
                toLeft = tabbarsWrapperWidth - tabsTotalWidth;
            }
            $("#tabbarsWrapper_scroll").animate({ left: toLeft + "px" }, 180, function () { changeDynamicHint(); });
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
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();

        if (tabsTotalWidth > tabbarsWrapperWidth) {
            $("#tabbarsWrapper_scroll").width(tabsTotalWidth);
            if (addordel) {//增加tab时
                var balanceToLeft = tabbarsWrapperWidth - tabsTotalWidth;//当没有滚动到最左边，直接滚动到最左边
                $("#tabbarsWrapper_scroll").animate({ left: balanceToLeft + "px" }, 180, function () { changeDynamicHint(); });
            }
            else {//关闭tab时
                var iLeft = $("#tabbarsWrapper_scroll").position().left;
                var b = tabsTotalWidth - Math.abs(iLeft);
                if (b <= tabbarsWrapperWidth) {
                    var balanceToLeft = tabbarsWrapperWidth - tabsTotalWidth;//当没有滚动到最左边，直接滚动到最左边
                    $("#tabbarsWrapper_scroll").animate({ left: balanceToLeft + "px" }, 180, function () { changeDynamicHint(); });
                }
            }
            //显示前、后滚动按钮
            $("#tabstripLeftForward").attr("class", "tabstrip-left-forward");
            $("#tabstripRightForward").attr("class", "tabstrip-right-forward");
        }
        else {
            if (!addordel) {//关闭tab时
                $("#tabbarsWrapper_scroll").width(tabsTotalWidth);
            }

            $("#tabbarsWrapper_scroll").animate({ left: "0" }, 150, function () { changeDynamicHint(); });

            //隐藏前、后滚动按钮
            $("#tabstripLeftForward").attr("class", "tabstrip-left-forward-hide");
            $("#tabstripRightForward").attr("class", "tabstrip-right-forward-hide");
        }
    }

    //在每个tabbar上增加鼠标点击事件
    function addClickEventToTabbar(tabid, contentUrl, needupdate) {
        $("#tabBar" + tabid).click(function () {
            changeStyleofSelected(tabid);
            changeDynamicHint();
        })
    }

    function fullfillCallBackFunc(tabid) {
        for (var i = 0; i < tabarray.length; i++) {
            if (tabarray[i]["id"] == tabid) {
                var title = tabarray[i]["title"];
                var url = tabarray[i]["url"];
                var ss = url.split('/');
                url2 = "/" + ss[1] + "/" + ss[2];
                var needupdate = tabarray[i]["needupdate"];
                callbackFunc(tabid, title, url2, needupdate);
                break;
            }
        }
    }

    //改变正处于激活状态的tab的style
    var changeStyleofSelected = function (tabid) {
        $("#tabbarsWrapper_scroll>div").removeClass("tabbar-down");
        $("#tabBar" + tabid).addClass("tabbar-down");
        $("#tabstripContent>div").removeClass("tab-content-div-down");
        $("#tabstripContent>div").addClass("tab-content-div");
        $("#tabContent" + tabid).removeClass("tab-content-div");
        $("#tabContent" + tabid).addClass("tab-content-div-down");

        activeTabId = tabid;//在此集中赋值，因为添加、删除和点击tab时都使用这个函数
        tabsTotalWidthBeforeActive = 0;//初始化
        flag = false;
        //所有的tabbar中只有一个处于激活状态，那就是tabid这个
        for (var i = 0; i < tabarray.length; i++) {
            if (tabarray[i]["id"] != tabid) {
                tabarray[i]["active"] = "false";//其他的都处于非激活（点击）状态
            }
            else {
                flag = true;
                tabarray[i]["active"] = "true";
            }
            if (!flag) {//只加active之前的那些tabs的宽度
                tabsTotalWidthBeforeActive += $("#tabBar" + tabarray[i]["id"]).width() + 5;
            }
        }
        fullfillCallBackFunc(tabid);//执行回调
    }

    //改变tab动态位置指示的位置
    function changeDynamicHint() {
        var iLeft = $("#tabbarsWrapper_scroll").position().left;
        var w = $("#tabBar" + activeTabId).width() + 5;
        $("#dynamicHintBar").width(w);
        $("#dynamicHintBar").animate({ left: (tabsTotalWidthBeforeActive - Math.abs(iLeft)) + "px" }, 150);
    }

    //在每个tabbar上的关闭按钮上增加鼠标点击事件
    function addClickEventToCloseButton(tabid) {
        $("#tabclosea_" + tabid).bind('click', function (event) {
            for (var i = 0; i < tabarray.length; i++) {
                if (tabarray[i]["id"] == tabid) {
                    if (tabarray[i]["active"] == "true") {
                        if (i == tabarray.length - 1 && i - 1 >= 0) {//位置最后一个tab,且前面还有tab
                            var id = tabarray[i - 1]["id"];
                            tabarray.splice(i, 1);//删除数组中tabid对应的元素  
                            changeStyleofSelected(id);//激活前面那个tab
                        }
                        else if (i <= tabarray.length - 1) {
                            var id = tabarray[tabarray.length - 1]["id"];
                            tabarray.splice(i, 1);//删除数组中tabid对应的元素 
                            changeStyleofSelected(id);//激活最后一个tab
                        }
                    }
                    else {//如果关闭的是非激活tab
                        var id;
                        for (var k = 0; k < tabarray.length; k++) {//寻找到当前激活状态的tabid
                            if (tabarray[k]["active"] == "true") {
                                id = tabarray[k]["id"];
                                break;
                            }
                        }
                        tabarray.splice(i, 1);//删除数组中tabid对应的元素  
                        changeStyleofSelected(id);
                    }
                    updateTabsTotalWidth();
                    break;
                }
            }
            $("#tabBar" + tabid).remove();
            $("#tabContent" + tabid).remove();
            //alert(tabarray.length);
            if (tabarray.length == 0) {//隐藏动态指示条
                $("#dynamicHintBar").css("display", "none");
            }
            addordel = false
            if (tabarray.length >= 0)//关闭后，清除style属性，最主要的是清除width属性
            {
                $("#tabbarsWrapper_scroll").removeAttr("style");//使tabbarsWrapper_scroll还原为width:auto
                addordel = true;//设置为增加tab的状态，以阻止tabbarsWrapper_scroll的width属性被设置
            }

            showForwardTools();//更新前后滚动按钮和tabs位置状态（滚动）
            //changeDynamicHint(tabid); //改变tab动态位置指示的位置
            event.stopPropagation();//阻止事件冒泡，即，阻止上层元素的click事件触发
        })
    }

    //增加或删除tab时，更新全局变量tabsTotalWidth，以供其他多个函数使用
    function updateTabsTotalWidth() {
        tabsTotalWidth = 0;
        for (var i = 0; i < tabarray.length; i++) {
            tabsTotalWidth += $("#tabBar" + tabarray[i]["id"]).width() + 9;// '+5'added on 20201115 because of the 'margin-right:5px'
        }
    }
}