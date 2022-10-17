$(document).ready(function () {
    $(".add").click(function () {
        Swal.fire({
            html: '<form class="createForm" action="parser/create" method="post">' +
                '<h1>Создать</h1>' +
                '<br>' +
                '<br>' +
                '<p>' +
                    '<span class="subtitle" for="Name">Название</span>' +
                    '<input name="Name" id="Name" class="swal2-input" required>' +
                '<p>' +
                    '<span class="subtitle" for="token">Токен</span> ' +
                    '<input name="Token" id="Token" class="swal2-input" required>' +
                '<p>' +
                    '<span class="subtitle" for="Location">Локация</span> ' +
                    '<select name="Location" class="swal2-select inline-flex" id="Location" required>' +
                        '<option value="EU">EU</option>' +
                        '<option value="RU (sng)">RU (sng)</option>' +
                        '<option value="Oceania">Oceania</option>' +
                    '</select> ' +
                '<p>' +
                    '<span class="subtitle" for="token">Максимальный лвл</span> ' +
                    '<input name="MaxLvl" type="number" id="MaxLvl" class="swal2-input" required>' +
                '<p>' +
                    '<span class="subtitle" for="token">Задержка (мс)</span> ' +
                    '<input name="Delay" id="Delay" type="number" class="swal2-input" required>' +
                '<p>' +
                    '<span class="subtitle" for="token">Прокси (ip:port:log:pass)</span> ' +
                    '<input name="Proxy" id="Proxy" class="swal2-input">' +
                '<p>' +
                    '<span class="subtitle" for="Location">Тип прокси</span> ' +
                    '<select name="ProxyType" class="swal2-select inline-flex" id="ProxyType">' +
                        '<option value="http">http</option>' +
                        '<option value="https">https</option>' +
                        '<option value="socks4">socks4</option>' +
                        '<option value="socks5">socks5</option>' +
                    '</select> ' +
                '<p>' +
                    '<input type="submit" value="SUBMIT" class="swal2-confirm swal2-styled submitBtn">' +
                '</form>',
            showCancelButton: false,
            showConfirmButton: false
        })
    });
});