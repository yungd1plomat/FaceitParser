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

    $('.add').click(function() {
        Swal.fire({
            title: 'Select a file',
            showCancelButton: true,
            confirmButtonText: 'Upload',
            input: 'file',
            onBeforeOpen: () => {
                $(".swal2-file").change(function () {
                    var reader = new FileReader();
                    reader.readAsDataURL(this.files[0]);
                });
            }
        }).then((file) => {
            if (file.value) {
                var formData = new FormData();
                var file = $('.swal2-file')[0].files[0];
                formData.append("fileToUpload", file);
                $.ajax({
                    headers: { 'X-CSRF-TOKEN': $('meta[name="csrf-token"]').attr('content') },
                    method: 'post',
                    url: '../blacklist/upload',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (resp) {
                        Swal('Uploaded', 'Your file have been uploaded', 'success');
                    },
                    error: function() {
                        Swal({ type: 'error', title: 'Oops...', text: 'Something went wrong!' })
                    }
                })
            }
        })
    })
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