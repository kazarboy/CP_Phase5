$(document).ready(function () {

    sameHeight();
    content_width()

    $(".uploadlogo").change(function () {
        var filename = readURL(this);
        $(this).parent().children('span').html(filename);
    });

});

window.onresize = function () {

    var x = window.matchMedia("(max-width: 767px)")

    if (!x.matches) {
        menu_open = !menu_open;

        $("div.sidenav").show();

        $("div.mobile-menu").hide();
        $("div.mobile-overlay").hide();
        $(".close-btn").hide()
    }

    else if (screen.width < 767) {

        setTimeout(function () { $("div.sidenav").hide(); }, 300);

        //$("div.sidenav").hide();
    }

    sameHeight();
    content_width()
}

var open = false;

$("#open-sub").click(function () {

    if (open) {
        $("div.subsidiary-info ").slideDown()
        $("i.fa-sub").removeClass("fa-chevron-down")
        $("i.fa-sub").addClass("fa-chevron-up");
    } else {
        $("div.subsidiary-info ").slideUp();
        $("i.fa-sub").addClass("fa-chevron-down")
        $("i.fa-sub").removeClass("fa-chevron-up");
    }

    open = !open;

});

function sameHeight() {

    setTimeout(function () { $('.card-col').jQueryEqualHeight(); }, 300);

}

function content_width() {

    // $(".content").width($(window).width() - 110);
    // $("div.sidenav").height( $("body").height())

    setTimeout(function () {
        $(".content").width($(window).width() - 110);
        $("div.sidenav").height($("div.content").height())

    }, 100);

}

function popup_upload() {
    $('#update-member').modal('show');
}


var menu_open = false;

function open_menu() {

    menu_open = !menu_open;

    if (!menu_open) {
        $("div.sidenav").show();
        $("body").addClass("no-scroll");
        $("div.mobile-overlay").show();
        $(".close-btn").show();
    }

    else {
        $("div.sidenav").hide();
        $("body").removeClass("no-scroll");
        $("div.mobile-overlay").hide();
        $(".close-btn").hide();
    }

}


function readURL(input) {

    var url = input.value;
    var ext = url.substring(url.lastIndexOf('.') + 1).toLowerCase();

    //filter pdf type
    if (input.files && input.files[0] && (ext === "xlsx")) {

        var path = $(input).val();
        var filename = path.replace(/^.*\\/, "");

        return "" + filename;
    }

    else {
        $(input).val("");
        return "Only excel formats are allowed!";
    }

}