(function () {
    "use strict";

    var list = document.getElementById("sideEffectsList");
    var template = document.getElementById("sideEffectRowTemplate");
    var addBtn = document.getElementById("addSideEffectBtn");

    if (!list || !template || !addBtn) return;

    function reindexRows() {
        var rows = list.querySelectorAll(".side-effect-row");
        rows.forEach(function (row, index) {
            row.querySelectorAll("[name]").forEach(function (field) {
                field.name = field.name.replace(/SideEffects\[\d+\]/, "SideEffects[" + index + "]");
            });
        });
    }

    addBtn.addEventListener("click", function () {
        var html = template.innerHTML.replace(/__INDEX__/g, list.children.length);
        var wrapper = document.createElement("div");
        wrapper.innerHTML = html.trim();
        list.appendChild(wrapper.firstElementChild);
    });

    list.addEventListener("click", function (e) {
        if (e.target.classList.contains("remove-side-effect-btn")) {
            e.target.closest(".side-effect-row").remove();
            reindexRows();
        }
    });
    var deleteBtn = document.getElementById("deleteMedicineBtn");
    if (deleteBtn) {
        deleteBtn.addEventListener("click", function () {
            if (!confirm("Delete this medicine permanently? This cannot be undone.")) return;

            var tokenInput = document.querySelector('#medicineForm input[name="__RequestVerificationToken"]');

            var deleteForm = document.createElement("form");
            deleteForm.method = "post";
            deleteForm.action = "/Admin/DeleteMedicine";
            deleteForm.style.display = "none";

            var idField = document.createElement("input");
            idField.type = "hidden";
            idField.name = "id";
            idField.value = deleteBtn.dataset.id;
            deleteForm.appendChild(idField);

            if (tokenInput) {
                var tokenField = document.createElement("input");
                tokenField.type = "hidden";
                tokenField.name = "__RequestVerificationToken";
                tokenField.value = tokenInput.value;
                deleteForm.appendChild(tokenField);
            }

            document.body.appendChild(deleteForm);
            deleteForm.submit();
        });
    }
})();