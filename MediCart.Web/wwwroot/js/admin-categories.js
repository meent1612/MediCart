    // ---- Category edit toggle ----
    var categoryForm = document.getElementById("categoryForm");
    var categoryFormId = document.getElementById("categoryFormId");
    var categoryFormKind = document.getElementById("categoryFormKind");
    var categoryName = document.getElementById("categoryName");
    var categoryDescription = document.getElementById("categoryDescription");
    var categoryParent = document.getElementById("categoryParent");
    var categoryFormTitle = document.getElementById("categoryFormTitle");
    var categoryFormSubmit = document.getElementById("categoryFormSubmit");
    var cancelCategoryEdit = document.getElementById("cancelCategoryEdit");
    var newCategoryBtn = document.getElementById("newCategoryBtn");

    categoryForm.dataset.createAction = categoryForm.getAttribute("action");

    function resetCategoryForm() {
        categoryForm.action = categoryForm.dataset.createAction;
        categoryFormId.value = "";
        categoryFormKind.value = "category";
        categoryName.value = "";
        categoryDescription.value = "";
        categoryParent.value = "";
        categoryParent.disabled = false;
        categoryFormTitle.textContent = "Add a category";
        categoryFormSubmit.textContent = "Add category";
        cancelCategoryEdit.hidden = true;
    }

    if (categoryForm.dataset.hadError === "true") categoryForm.hidden = false;

    document.querySelectorAll(".edit-category-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var kind = btn.dataset.kind || "category";

            categoryFormId.value = btn.dataset.id;
            categoryFormKind.value = kind;
            categoryName.value = btn.dataset.name || "";
            categoryDescription.value = btn.dataset.description || "";
            categoryParent.value = btn.dataset.parentId || "";
            categoryFormTitle.textContent = kind === "subcategory" ? "Edit subcategory" : "Edit category";
            categoryFormSubmit.textContent = "Save changes";
            categoryForm.action = "/Admin/EditCategory";
            cancelCategoryEdit.hidden = false;
            categoryForm.hidden = false;
            categoryForm.scrollIntoView({ behavior: "smooth", block: "center" });
        });
    });

    cancelCategoryEdit.addEventListener("click", function () {
        resetCategoryForm();
        categoryForm.hidden = true;
    });
    newCategoryBtn.addEventListener("click", function () {
        resetCategoryForm();
        categoryForm.hidden = false;
        categoryName.focus();
    });