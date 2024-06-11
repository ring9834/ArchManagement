$.validator.setDefaults({
    submitHandler: function (form) {
        //alert("提交事件!");
        //form.submit();//提交时拦截
        $(form).ajaxSubmit();
    },
    errorElement: 'div',
    errorPlacement: function (error, element) {
        var delay = 200,
            fadeDuration = 250,
            fontSize = '1.0em',
            theme = 'light',
            textColor = '#757575',
            shadowColor = '#337ab7',
            fontFamily = "'Roboto-Medium', 'Roboto-Regular', Arial";
        if (element.is(":radio")) {
            var eid = error.attr("id");
            var eobj = $("#" + eid);
            if (eobj.length == 0) {
                error.attr("class", "awesome-error");
                error.tooltip({
                    text: error.text(),
                    delay: delay,
                    fadeDuration: fadeDuration,
                    fontSize: fontSize,
                    theme: theme,
                    textColor: textColor,
                    shadowColor: shadowColor,
                    fontFamily: fontFamily
                });
                error.text('');
                error.append("<i class='fa fa-minus-circle' style='color:red;'></i>");
                error.appendTo(element.parent().parent());
            }
            else {
                eobj.tooltip({
                    text: error.text(),
                    delay: delay,
                    fadeDuration: fadeDuration,
                    fontSize: fontSize,
                    theme: theme,
                    textColor: textColor,
                    shadowColor: shadowColor,
                    fontFamily: fontFamily
                });
            }
        }
        //else if (element.is(":checkbox")) {
        //    error.appendTo(element.next());
        //}
        else {
            var eid = error.attr("id");
            var eobj = $("#" + eid);
            if (eobj.length == 0) {
                error.attr("class", "awesome-error");
                error.tooltip({
                    text: error.text(),
                    delay: delay,
                    fadeDuration: fadeDuration,
                    fontSize: fontSize,
                    theme: theme,
                    textColor: textColor,
                    shadowColor: shadowColor,
                    fontFamily: fontFamily
                });
                error.text('');
                error.append("<i class='fa fa-minus-circle' style='color:red;'></i>");
                element.after(error);
            }
            else {
                eobj.tooltip({
                    text: error.text(),
                    delay: 400,
                    fadeDuration: 250,
                    fontSize: '1.0em',
                    theme: 'light',
                    textColor: '#757575',
                    shadowColor: '#000',
                    fontFamily: "'Roboto-Medium', 'Roboto-Regular', Arial"
                });
            }
        }

        var p = $.extend({}, element.parent().parent().offset(), {
            width: element.parent().parent().outerWidth()
            , height: element.parent().parent().outerHeight()
        })

        var pos = $.extend({}, element.offset(), {
            width: element.outerWidth()
            , height: element.outerHeight()
        }),
            actualWidth = error.outerWidth(),
            actualHeight = error.outerHeight();
        if ((pos.top - actualHeight) < 0) { actualHeight = 0; pos.width += 10; }//如果输入框距离顶端为0情况把提示放右边
        if (element.parents(".blockPage").attr("class") == "blockUI blockMsg blockPage") {//如果是弹出框的，那么设置如下
            error.css({ display: 'block', opacity: '0.6', left: 300, top: pos.top - $(document).scrollTop() - actualHeight - 100, "border-left": '0px' });
        }
        else if (element.is(":radio")) {//类型为radio的显示如下
            error.css({ display: 'block', opacity: '0.6', top: p.top + (p.height-20) / 2, left: p.left + p.width + 5 });
        }
        else {//其他均为以下显示
            error.css({ display: 'block', opacity: '0.6', top: pos.top + (pos.height-20) / 2, left: pos.left + pos.width + 5 });
            //error.css({ display: 'block', opacity: '0.6', top: pos.top - actualHeight, left: pos.left + pos.width - 10 });
        }
    },
    highlight: function (element, errorClass) {
        //高亮显示
        $(element).addClass(errorClass);
        //显示错误提示ICON
        var errorId = $(element).attr("aria-describedby");
        $("#" + errorId).css("display", "block");
    },
    unhighlight: function (element, errorClass) {
        $(element).removeClass(errorClass);
        //隐藏错误提示ICON
        var errorId = $(element).attr("aria-describedby");
        $("#" + errorId).css("display", "none");
    }
});