(function () {
    "use strict";

    /* ---------------------------------------------------------------------
       Side effects: add / remove / reindex / empty state
       ------------------------------------------------------------------ */
    var list = document.getElementById("sideEffectsList");
    var template = document.getElementById("sideEffectRowTemplate");
    var addBtn = document.getElementById("addSideEffectBtn");
    var emptyHint = document.getElementById("sideEffectsEmpty");

    function updateSideEffectsEmptyState() {
        if (!list || !emptyHint) return;
        emptyHint.hidden = list.children.length > 0;
    }

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

            var newRow = wrapper.firstElementChild;
            list.appendChild(newRow);
            updateSideEffectsEmptyState();
            newRow.querySelector("input")?.focus();
        });

        list.addEventListener("click", function (e) {
            if (e.target.classList.contains("remove-side-effect-btn")) {
                e.target.closest(".side-effect-row").remove();
                reindexRows();
                updateSideEffectsEmptyState();
            }
        });

        updateSideEffectsEmptyState();
    }

    /* ---------------------------------------------------------------------
       Image preview (file upload or pasted URL)
       ------------------------------------------------------------------ */
    var previewBox = document.getElementById("imagePreviewBox");
    var clearBtn = document.getElementById("imageClearBtn");
    var imageFileInput = document.getElementById("imageFileInput");
    var imageFileName = document.getElementById("imageFileName");
    var imageUrlInput = document.getElementById("imageUrlInput");

    var PLACEHOLDER_ICON =
        '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" ' +
        'stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2" />' +
        '<circle cx="9" cy="9" r="2" /><path d="m21 15-5-5L5 21" /></svg>';

    function showPreviewImage(src) {
        if (!previewBox) return;
        previewBox.querySelector("img, .image-upload__placeholder")?.remove();

        var img = document.createElement("img");
        img.id = "imagePreviewImg";
        img.alt = "Medicine image preview";
        img.src = src; // only ever called with a real, non-empty value
        previewBox.insertBefore(img, clearBtn || null);

        if (clearBtn) clearBtn.hidden = false;
    }

    function showPreviewPlaceholder() {
        if (!previewBox) return;
        previewBox.querySelector("img, .image-upload__placeholder")?.remove();

        var span = document.createElement("span");
        span.className = "image-upload__placeholder";
        span.id = "imagePreviewPlaceholder";
        span.innerHTML = PLACEHOLDER_ICON + "No image yet";
        previewBox.insertBefore(span, clearBtn || null);

        if (clearBtn) clearBtn.hidden = true;
    }

    // Pasted URL takes priority visually the moment it's typed.
    imageUrlInput?.addEventListener("input", function () {
        var url = imageUrlInput.value.trim();
        if (url) {
            showPreviewImage(url);
        } else if (!imageFileInput?.files?.length) {
            showPreviewPlaceholder();
        }
    });

    // Choosing a file previews it locally; the actual upload happens on submit.
    imageFileInput?.addEventListener("change", function () {
        var file = imageFileInput.files && imageFileInput.files[0];

        if (!file) {
            if (imageFileName) imageFileName.textContent = "No file chosen";
            if (!imageUrlInput?.value.trim()) showPreviewPlaceholder();
            return;
        }

        if (imageFileName) imageFileName.textContent = file.name;

        var reader = new FileReader();
        reader.onload = function (e) {
            showPreviewImage(e.target.result);
        };
        reader.readAsDataURL(file);
    });

    clearBtn?.addEventListener("click", function () {
        if (imageFileInput) imageFileInput.value = "";
        if (imageUrlInput) imageUrlInput.value = "";
        if (imageFileName) imageFileName.textContent = "No file chosen";
        showPreviewPlaceholder();
    });

    /* ---------------------------------------------------------------------
       Delete medicine — themed confirmation modal
       ------------------------------------------------------------------ */
    var deleteBtn = document.getElementById("deleteMedicineBtn");
    var deleteOverlay = document.getElementById("deleteModalOverlay");
    var deleteNameEl = document.getElementById("deleteModalName");
    var deleteCancelBtn = document.getElementById("deleteModalCancel");
    var deleteConfirmBtn = document.getElementById("deleteModalConfirm");

    function openDeleteModal() {
        if (!deleteOverlay) return;
        if (deleteNameEl) deleteNameEl.textContent = deleteBtn.dataset.name || "this medicine";
        deleteOverlay.classList.add("is-open");
        deleteOverlay.setAttribute("aria-hidden", "false");
        deleteCancelBtn?.focus();
        document.addEventListener("keydown", onDeleteModalKeydown);
    }

    function closeDeleteModal() {
        if (!deleteOverlay) return;
        deleteOverlay.classList.remove("is-open");
        deleteOverlay.setAttribute("aria-hidden", "true");
        document.removeEventListener("keydown", onDeleteModalKeydown);
        deleteBtn?.focus();
    }

    function onDeleteModalKeydown(e) {
        if (e.key === "Escape") {
            closeDeleteModal();
            return;
        }
        if (e.key === "Tab") {
            var focusables = [deleteCancelBtn, deleteConfirmBtn];
            var currentIndex = focusables.indexOf(document.activeElement);
            e.preventDefault();
            var nextIndex = e.shiftKey
                ? (currentIndex <= 0 ? focusables.length - 1 : currentIndex - 1)
                : (currentIndex === focusables.length - 1 ? 0 : currentIndex + 1);
            focusables[nextIndex]?.focus();
        }
    }

    function submitDelete() {
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
    }

    if (deleteBtn && deleteOverlay) {
        deleteBtn.addEventListener("click", openDeleteModal);
        deleteCancelBtn?.addEventListener("click", closeDeleteModal);
        deleteOverlay.addEventListener("click", function (e) {
            if (e.target === deleteOverlay) closeDeleteModal();
        });
        deleteConfirmBtn?.addEventListener("click", submitDelete);
    } else if (deleteBtn) {
        // Fallback in case the modal markup isn't present for some reason.
        deleteBtn.addEventListener("click", function () {
            if (confirm("Delete this medicine permanently? This cannot be undone.")) {
                submitDelete();
            }
        });
    }

    /* ---------------------------------------------------------------------
       Dismissible / auto-dismissing alerts
       ------------------------------------------------------------------ */
    document.querySelectorAll("[data-alert]").forEach(function (alert) {
        function dismiss() {
            alert.style.transition = "opacity .2s ease, transform .2s ease";
            alert.style.opacity = "0";
            alert.style.transform = "translateY(-6px)";
            setTimeout(function () { alert.remove(); }, 200);
        }

        alert.querySelector(".admin-alert__dismiss")?.addEventListener("click", dismiss);

        if (alert.dataset.autoDismiss === "true") {
            setTimeout(dismiss, 5000);
        }
    });
})();
