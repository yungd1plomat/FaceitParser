$(document).ready(function () {
    $(".add").click(function () {
        $.post("../accounts")
            .done(function (data) {
                console.log(data);
                var form =  '<form class="createForm" action="parser/create" method="post">' +
                            '<h1>Создать</h1>' +
                            '<br>' +
                            '<br>' +
                            '<p>' +
                            '<span class="subtitle" for="Name">Название</span>' +
                            '<input name="Name" id="Name" class="swal2-input" required>' +
                            '<p>' +
                            '<span class="subtitle">Аккаунт</span> ' +
                            '<select name="Token" class="swal2-select inline-flex" id="Token">';
                for (var nick in data) {
                    form += '<option value="' + data[nick] + '">' + nick + '</option>';
                }
                form += '</select> ' +
                        '<p>' +
                        '<span class="subtitle">Локация</span> ' +
                        '<select name="Location" class="swal2-select inline-flex" id="Location" required>' +
                        '<option value="EU">EU</option>' +
                        '<option value="RU (sng)">RU (sng)</option>' +
                        '<option value="Oceania">Oceania</option>' +
                        '</select> ' +
                        '<p>' +
                        '<span class="subtitle">Максимальный лвл</span> ' +
                        '<input name="MaxLvl" placeholder="10" type="number" min="1" max="10" id="MaxLvl" class="swal2-input" required>' +
                        '<p>' +
                        '<span class="subtitle">Максимально матчей</span> ' +
                        '<input name="MaxMatches" placeholder="1000000" id="MaxMatches" type="number" min="0" class="swal2-input" required>' +
                        '<p>' +
                        '<span class="subtitle">Задержка (мс)</span> ' +
                        '<input name="Delay" id="Delay" placeholder="2000" type="number" min="1000" class="swal2-input" required>' +
                        '<p>' +
                        '<span class="subtitle">Инвентарь от ($)</span> ' +
                        '<input name="MinPrice" id="MinPrice" placeholder="100" type="number" min="0" class="swal2-input" required>' +
                        '<p>' +
                        '<span class="subtitle">Автодобавление в друзья</span> ' +
                        '<select name="AutoAdd" class="swal2-select inline-flex" id="AutoAdd" required>' +
                        '<option value="true">Включено</option>' +
                        '<option value="false">Отключено</option>' +
                        '</select> ' +
                        '<p>' +
                        '<span class="subtitle">Прокси (ip:port:log:pass)</span> ' +
                        '<input name="Proxy" placeholder="41.5.151.88:5501:d1plomat:password1234" id="Proxy" class="swal2-input">' +
                        '<p>' +
                        '<span class="subtitle">Тип прокси</span> ' +
                        '<select name="ProxyType" class="swal2-select inline-flex" id="ProxyType">' +
                        '<option value="http">http</option>' +
                        '<option value="https">https</option>' +
                        '<option value="socks4">socks4</option>' +
                        '<option value="socks5">socks5</option>' +
                        '</select> ' +
                        '<p>' +
                        '<input type="submit" value="Создать" class="swal2-confirm swal2-styled submitBtn">' +
                        '</form>'
                Swal.fire({
                    html: form,
                    showCancelButton: false,
                    showConfirmButton: false
                })
            });
    });
});

function Delete(parserName) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.post("../parser/delete", { name: parserName })
                .done(function () {
                    $("#" + parserName).parent().parent().remove();
                    Swal.fire(
                        'Deleted!',
                        'Parser has been deleted.',
                        'success'
                    )
                });
        }
    })
}