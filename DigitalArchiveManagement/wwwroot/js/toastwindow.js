function TS() { }

TS.alertWin = function (h,t,p) {
    $.toast({
        heading: h,
        text: t,
        position: p,
        stack: false,
        icon: 'warning',
        bgColor: '#c70604',
        textColor: '#fff',
        loaderBg:'#d9544f'
    });
}

TS.hintWin = function (h, t, p) {
    $.toast({
        heading: h,
        text: t,
        position: p,
        stack: false,
        icon: 'info',
        bgColor: '#5bc0de',
        textColor: '#fff',
        loaderBg: '#c2f1fa'
    });
}


