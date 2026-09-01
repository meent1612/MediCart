(function () {
    "use strict";

    var filterCategory = document.getElementById("filterCategory");
    var filterProductType = document.getElementById("filterProductType");
    var form = document.getElementById("medicineFilterForm");

    if (!form) return;

    [filterCategory, filterProductType].forEach(function (select) {
        select?.addEventListener("change", function () {
            form.submit();
        });
    });
})();