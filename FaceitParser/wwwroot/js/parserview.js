var dontScroll = false;
$(document).ready(function () {
    setInterval(updateData, 1000);
    $(".logsContainer")
        .mouseenter(function (e) {
            dontScroll = true;
        })
        .mouseleave(function (e) {
            dontScroll = false;
        });
});

function updateData() {
    $.post(window.location.href, function (data) {
        $(".account").html(data["account"]);
        var a = $("#games");
        $(".games").text(data["games"]);
        $(".parsed").text(data["parsed"]);
        $(".delay").text(data["delay"]);
        $(".total").text(data["total"]);
        $(".added").text(data["added"]);
        $.each(data["logs"], function (index, message) {
            console.log(message);
            $('.logsContainer').append("<p>" + message + "</p>");
        });
        Scroll();
    });
}

function Scroll() {
    if (!dontScroll) {
        let scrollLenght = $('.logsContainer').children().length;
        $('.logsContainer').animate({
            scrollTop: scrollLenght * 70
        }, 'slow');
    }
}