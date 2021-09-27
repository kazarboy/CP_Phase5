$(document).ready(function () {
    $(".uploadlogo").change(function () {
        var filename = readURL(this);
        $(this).parent().children('span').html(filename);
    });

});

function readURL(input) {

    var url = input.value;
    var ext = url.substring(url.lastIndexOf('.') + 1).toLowerCase();

    //filter pdf type
    if (input.files && input.files[0] && (ext === "pdf")) {

        var path = $(input).val();
        var filename = path.replace(/^.*\\/, "");

        return "" + filename;
    }

    else {
        $(input).val("");
        return "Only pdf formats are allowed!";
    }

}