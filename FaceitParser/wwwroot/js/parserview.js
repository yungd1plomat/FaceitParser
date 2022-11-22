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
    $(".steamIdContainer")
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
        var html = "";
        $.each(data["steamIds"], function (index, steamid) {
            console.log(steamid);
            html += "<p>" + steamid + "</p>";
        });
        $('.steamIdContainer').html(html);
        Scroll();
    });
}

function Scroll() {
    if (!dontScroll) {
        let logsLength = $('.logsContainer').children().length;
        $('.logsContainer').animate({
            scrollTop: logsLength * 70
        }, 'slow');
    }
}