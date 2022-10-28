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
            html: '<form class="createForm" action="../accounts/add" method="post">' +
                '<h1>Добавить</h1>' +
                '<br>' +
                '<br>' +
                '<p>' +
                '<span class="subtitle" for="Username">Токен</span>' +
                '<input name="Token" id="Token" class="swal2-input" required>' +
                '<p>' +
                '<input type="submit" value="Добавить" class="swal2-confirm swal2-styled submitBtn">' +
                '</form>',
            showCancelButton: false,
            showConfirmButton: false
        })
    });
});

function Delete(accountToken) {
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
            $.post("../accounts/delete", { Token: accountToken })
                .done(function (data) {
                    $("#" + accountToken).parent().parent().remove();
                    Swal.fire(
                        'Deleted!',
                        'User ' + data + ' has been deleted.',
                        'success'
                    )
                });
        }
    })
}