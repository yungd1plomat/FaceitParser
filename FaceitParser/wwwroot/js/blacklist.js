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
});

function Delete(profile) {
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
            $.post("../blacklist/remove", { profile: profile })
                .done(function () {
                    $("#" + profile).parent().parent().remove();
                    Swal.fire(
                        'Deleted!',
                        'Player has been deleted.',
                        'success'
                    )
                });
        }
    })
}