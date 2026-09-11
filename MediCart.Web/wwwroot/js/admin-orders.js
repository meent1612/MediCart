document.addEventListener('DOMContentLoaded', function () {
    var openBtn = document.getElementById('openRejectModal');
    var closeBtn = document.getElementById('closeRejectModal');
    var modal = document.getElementById('rejectModal');

    if (!modal) return;

    if (openBtn) {
        openBtn.addEventListener('click', function () {
            modal.classList.add('is-open');
        });
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            modal.classList.remove('is-open');
        });
    }

    modal.addEventListener('click', function (e) {
        if (e.target === modal) {
            modal.classList.remove('is-open');
        }
    });
});