(function () {

    "use strict";

    var list = document.getElementById("sideEffectsList");

    var template = document.getElementById("sideEffectRowTemplate");

    var addBtn = document.getElementById("addSideEffectBtn");

    function reindexRows() {

        if (!list) return;

        var rows = list.querySelectorAll(".side-effect-row");

        rows.forEach(function (row, index) {

            row.querySelectorAll("[name]").forEach(function (field) {

                field.name = field.name.replace(
                    /SideEffects\[\d+\]/,
                    "SideEffects[" + index + "]"
                );

            });

        });

    }

    if (list && template && addBtn) {

        addBtn.addEventListener("click", function () {

            var html = template.innerHTML.replace(
                /__INDEX__/g,
                list.children.length
            );

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

    }

    // ---- Image URL preview ----

    var imageUrlInput = document.getElementById("imageUrlInput");

    var imageUrlPreview = document.getElementById("imageUrlPreview");

    if (imageUrlInput && imageUrlPreview) {

        imageUrlInput.addEventListener("input", function () {

            var url = imageUrlInput.value.trim();

            if (url) {

                imageUrlPreview.src = url;

                imageUrlPreview.hidden = false;

            } else {

                imageUrlPreview.src = "";

                imageUrlPreview.hidden = true;

            }

        });

    }

    // ---- Client-side preview when a file is chosen ----
    // Actual upload happens when the form is submitted.

    var imageFileInput = document.getElementById("imageFileInput");

    if (imageFileInput && imageUrlPreview) {

        imageFileInput.addEventListener("change", function () {

            var file = imageFileInput.files && imageFileInput.files[0];

            if (!file) return;

            var reader = new FileReader();

            reader.onload = function (e) {

                imageUrlPreview.src = e.target.result;

                imageUrlPreview.hidden = false;

            };

            reader.readAsDataURL(file);

        });

    }

    // ---- Delete medicine ----

    var deleteBtn = document.getElementById("deleteMedicineBtn");

    if (deleteBtn) {

        deleteBtn.addEventListener("click", function () {

            if (!confirm("Delete this medicine permanently? This cannot be undone.")) return;

            var tokenInput = document.querySelector(
                '#medicineForm input[name="__RequestVerificationToken"]'
            );

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