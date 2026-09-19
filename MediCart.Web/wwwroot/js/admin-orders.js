document.addEventListener('DOMContentLoaded', function () {
    function setupModal(modalId, openBtnId, closeBtnId) {
        var modal = document.getElementById(modalId);
        var openBtn = document.getElementById(openBtnId);
        var closeBtn = document.getElementById(closeBtnId);

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
    }

    setupModal('rejectModal', 'openRejectModal', 'closeRejectModal');
    setupModal('cancelModal', 'openCancelModal', 'closeCancelModal');

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal-overlay.is-open').forEach(function (m) {
                m.classList.remove('is-open');
            });
        }
    });
});