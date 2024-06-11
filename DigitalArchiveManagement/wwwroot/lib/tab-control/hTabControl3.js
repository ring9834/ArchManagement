function hTabControl(top, content) {
    var tabtopdiv = top;
    var tabcontentdiv = content;
    var tabarray = [];
    var lastbalance = 0;//记录tab增加或减少时的上一次（totalWidth - tabbarsWrapperWidth）
    this.IsFirstTabClosible = true;//属性，决定第一个tab上是否有关闭按钮，默认有
    this.ForwardStep = 50;//滚动步长
    var fs = 200;
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
                $("#" + tabtopdiv).append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + title + "</div></div></div>");
            }
            else {
                //在控件顶部增加tab,带关闭按钮
                $("#" + tabtopdiv).append("<div id='tabBar" + tabid + "' class='tabbar'><div class='tab-title-wrapper'><div class='tab-title-div'>" + title + "</div><div class='tab-close-div'><a href='#' id='tabclosea_" + tabid + "' class='tab-close-button'><i class='fa fa-close'></i></a></div></div></div>");
            }
            //增加内容div
            $("#" + tabcontentdiv).append("<div class='tab-content-div' id='tabContent" + tabid + "'><iframe src='" + contenturl + "' class='tab-content-iframes' frameborder='0'></iframe></div>");

            addClickEventToTabbar(tabid);
            addClickEventToCloseButton(tabid);
            changeStyleofSelected(tabid);
            showForwardTools();
        }
        else {//当tabbar存在时，仅激活使其显示即可
            changeStyleofSelected(tabid);
        }
    }

    //tabs左滚
    this.leftForward = function () {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var tabscrollLeft = $("#tabbarsWrapper_scroll").css("left");
        var iLeft = parseInt(tabscrollLeft.replace("px", ""));
        if (iLeft < 0) {
            iLeft += this.ForwardStep;
            if (iLeft > 0) {
                iLeft = 0;
            }
            $("#tabbarsWrapper_scroll").animate({ left: iLeft + "px" });
        }
    }

    //tabs右滚
     this.rightForward = function() {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var tabscrollLeft = $("#tabbarsWrapper_scroll").css("left");
        var iLeft = parseInt(tabscrollLeft.replace("px", ""));
        var n = Math.abs(iLeft) + tabbarsWrapperWidth;
        if (n < totalWidth) {
            var toLeft = iLeft - this.ForwardStep;
            if (n > totalWidth) {
                toLeft = tabbarsWrapperWidth - totalWidth;
            }
            $("#tabbarsWrapper_scroll").animate({ left: toLeft + "px" });
        }
    }

    var multact;
    var timeout;
    this.leftForwardMouseDown = function () {
        timeout = setTimeout(function () {
            multiLeftWard();            
        }, 200);//鼠标按下0.5秒后发生multiLeftWard事件
    }

    this.rightForwardMouseDown = function () {
        timeout = setTimeout(function () {
            multiRightWard();
        }, 200);//鼠标按下0.5秒后发生multiLeftWard事件
    }

    this.offMultiForward = function () {
        window.clearInterval(multact); 
        clearTimeout(timeout);//清理掉定时器
    }

    function multiLeftWard() {
        multact = setInterval(leftForward2, 500);//0.5秒执行1次
    }

    function multiRightWard() {
        multact = setInterval(rightForward2, 500);//0.5秒执行1次
    }

    //tabs左滚
    leftForward2 = function () {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        
        var tabscrollLeft = $("#tabbarsWrapper_scroll").css("left");
        var iLeft = parseInt(tabscrollLeft.replace("px", ""));
        if (iLeft < 0) {
            iLeft += fs;
            if (iLeft > 0) {
                iLeft = 0;
            }
            $("#tabbarsWrapper_scroll").animate({ left: iLeft + "px" });
        }
    }

    //tabs右滚
    rightForward2 = function () {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }
        var tabscrollLeft = $("#tabbarsWrapper_scroll").css("left");
        var iLeft = parseInt(tabscrollLeft.replace("px", ""));
        var n = Math.abs(iLeft) + tabbarsWrapperWidth;
        if (n < totalWidth) {
            var toLeft = iLeft - fs;
            if (n > totalWidth) {
                toLeft = tabbarsWrapperWidth - totalWidth;
            }
            $("#tabbarsWrapper_scroll").animate({ left: toLeft + "px" });
        }
    }


    //显示后隐藏两个前、后按钮;并且，当显示前后滚动按钮时，让tabs集体左滚，当滚动按钮隐藏时，让tabs集体右滚返回原来位置
    function showForwardTools() {
        var totalWidth = 0;
        var tabbarsWrapperWidth = $("#tabbarsWrapper").width();
        for (var i = 0; i < tabarray.length; i++) {
            totalWidth += $("#tabBar" + tabarray[i]["id"]).width();
        }

        if (totalWidth > tabbarsWrapperWidth) {
            var tabscrollLeft = $("#tabbarsWrapper_scroll").css("left");
            var iLeft = parseInt(tabscrollLeft.replace("px", ""));
            $("#tabbarsWrapper_scroll").width(totalWidth);
            //if (iLeft == 0) {
            //    //左滚动
            //    var balanceToLeft = iLeft - (totalWidth - tabbarsWrapperWidth);
            //    $("#tabbarsWrapper_scroll").animate({ left: balanceToLeft + "px" });
            //}
            //else if (iLeft < 0){
            var balanceToLeft = iLeft - (totalWidth - tabbarsWrapperWidth - lastbalance);
            $("#tabbarsWrapper_scroll").animate({ left: balanceToLeft + "px" });
            //}
            lastbalance = totalWidth - tabbarsWrapperWidth;

            //显示前、后滚动按钮
            $("#tabstripLeftForward").attr("class", "tabstrip-left-forward");
            $("#tabstripRightForward").attr("class", "tabstrip-right-forward");
        }
        else {
            var tabscrollLeft = $("#tabbarsWrapper_scroll").css("left");
            var iLeft = parseInt(tabscrollLeft.replace("px", ""));
            if (iLeft < 0) {
                //var balanceToRight = Math.abs(iLeft);
                $("#tabbarsWrapper_scroll").animate({ left: "0" });
            }
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
        $("#" + tabtopdiv + ">div").removeClass("tabbar-down");
        $("#tabBar" + tabid).addClass("tabbar-down");
        $("#" + tabcontentdiv + ">div").removeClass("tab-content-div-down");
        $("#" + tabcontentdiv + ">div").addClass("tab-content-div");
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
                    tabarray.splice(i, 1);//删除数组中tabid对应的元素
                    break;
                }
            }
            $("#tabBar" + tabid).remove();
            $("#tabContent" + tabid).remove();
            showForwardTools();//更新前后滚动按钮和tabs位置状态（滚动）
            event.stopPropagation();//阻止事件冒泡，即，阻止上层元素的click事件触发
        })
    }
}