(function () {
    "use strict";

    var copyBtn = document.getElementById("copyOrderBtn");
    if (!copyBtn) return;

    copyBtn.addEventListener("click", function () {
        var orderNumber = copyBtn.dataset.order;

        function done() {
            var original = copyBtn.textContent;
            copyBtn.textContent = "Copied";
            copyBtn.classList.add("is-copied");
            setTimeout(function () {
                copyBtn.textContent = original;
                copyBtn.classList.remove("is-copied");
            }, 1500);
        }

        if (navigator.clipboard) {
            navigator.clipboard.writeText(orderNumber).then(done).catch(done);
        } else {
            done();
        }
    });
})();
