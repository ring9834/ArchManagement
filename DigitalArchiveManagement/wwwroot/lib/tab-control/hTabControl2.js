function hTabControl(top, content) {
    var tabtopdiv = top;
    var tabcontentdiv = content;
    var tabarray = [];
    this.IsFirstTabClosible = true;//属性，决定第一个tab上是否有关闭按钮，默认有
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
        }
        else {//当tabbar存在时，仅激活使其显示即可
            changeStyleofSelected(tabid);
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
            event.stopPropagation();//阻止事件冒泡，即，阻止上层元素的click事件触发
        })
    }
}