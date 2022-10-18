$(document).ready(function () {
    $(".search").click(function () {
        let inp = $(".searchInput");
        if (!inp.val()) {
            inp.focus();
            return;
        }
        location.href = "?search=" + inp.val();
    });

    var searchParams = new URLSearchParams(window.location.search);
    let pageNum = searchParams.has('page') ? Number(searchParams.get('page')) : 0;
    if (pageNum)
        $(".prev").show();

    $('.pageNum').text(pageNum);

    $(".prev").click(function () {
        let href = "?page=" + --pageNum;
        if (searchParams.has('search'))
            href += '&search=' + searchParams.get('search');
        location.href = href;
    });

    $(".next").click(function () {
        let href = "?page=" + ++pageNum;
        if (searchParams.has('search'))
            href += '&search=' + searchParams.get('search');
        location.href = href;
    });

    $(".add").click(function () {
        Swal.fire({
            html: '<form class="createForm" action="account/create" method="post">' +
                '<h1>Создать</h1>' +
                '<br>' +
                '<br>' +
                '<p>' +
                '<span class="subtitle" for="Username">Юзернейм</span>' +
                '<input name="Username" id="Username" class="swal2-input" required>' +
                '<p>' +
                '<span class="subtitle">Пароль</span> ' +
                '<input name="Password" type="password" id="Token" class="swal2-input" required>' +
                '<p>' +
                '<span class="subtitle">Роль</span> ' +
                '<select name="Role" class="swal2-select inline-flex" id="Role" required>' +
                '<option value="user">user</option>' +
                '<option value="admin">admin</option>' +
                '</select> ' +
                '<p>' +
                '<input type="submit" value="Создать" class="swal2-confirm swal2-styled submitBtn">' +
                '</form>',
            showCancelButton: false,
            showConfirmButton: false
        })
    });
});

function Delete(user) {
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
            $.post("../account/delete", { username: user })
                .done(function () {
                    $("#" + user).parent().parent().remove();
                    Swal.fire(
                        'Deleted!',
                        'User has been deleted.',
                        'success'
                    )
                });
        }
    })
}

function Edit(user) {
    Swal.fire({
        html: '<form class="createForm" action="account/edit" method="post">' +
            '<h1>Создать</h1>' +
            '<br>' +
            '<br>' +
            '<input name="OldUsername" id="OldUsername" type="hidden" class="swal2-input" value="' +
            user +
            '">' +
            '<p>' +
            '<span class="subtitle" for="Username">Юзернейм*</span>' +
            '<input name="NewUsername" id="NewUsername" class="swal2-input">' +
            '<p>' +
            '<span class="subtitle">Пароль*</span> ' +
            '<input name="Password" type="password" id="Token" class="swal2-input">' +
            '<p>' +
            '<span class="subtitle">Роль*</span> ' +
            '<select name="Role" class="swal2-select inline-flex" id="Role">' +
            '<option value="user">user</option>' +
            '<option value="admin">admin</option>' +
            '</select> ' +
            '<p>' +
            '<input type="submit" value="Сохранить" class="swal2-confirm swal2-styled submitBtn">' +
            '</form>',
        showCancelButton: false,
        showConfirmButton: false
    })
}