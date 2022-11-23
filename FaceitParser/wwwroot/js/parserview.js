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
    $('.copySteams').click(function () {
        window.getSelection().selectAllChildren(
            document.getElementById('steamIdContainer')
        );
        document.execCommand('copy');
        if (window.getSelection) {
            if (window.getSelection().empty) {  // Chrome
                window.getSelection().empty();
            } else if (window.getSelection().removeAllRanges) {  // Firefox
                window.getSelection().removeAllRanges();
            }
        } else if (document.selection) {  // IE?
            document.selection.empty();
        }
    });
    $('.copySteams').mousedown(function () {
        $('.copySteams').css('color', '#b0b0b0');
    });

    $('.copySteams').mouseup(function () {
        $('.copySteams').css('color', '#fff');
    })
    document.addEventListener('copy', (event) => {
        const toCopy = document.getSelection().toString();
        let toPaste = "";
        toCopy.split(/\r?\n/).map(function (v, i) {
            if (v.length > 0) {
                if (toPaste != "") {
                    toPaste += '\n';
                }
                toPaste += v;
            }
        });
        event.clipboardData.setData('text/plain', toPaste);
        event.preventDefault();
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