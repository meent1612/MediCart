using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class CategoryRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int SubCategoryCount { get; set; }
        public int MedicineCount { get; set; }
    }

    public class SubCategoryRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int MedicineCount { get; set; }
    }

    public class ProductTypeRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int MedicineCount { get; set; }
    }

    public class CategoryOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class AdminCategoriesViewModel
    {
        public List<CategoryRowViewModel> Categories { get; set; } = new();
        public List<SubCategoryRowViewModel> SubCategories { get; set; } = new();
        public List<ProductTypeRowViewModel> ProductTypes { get; set; } = new();

        // Options for the "Parent category" dropdown when adding/editing —
        // selecting one here means the row being saved is a SubCategory.
        public List<CategoryOptionViewModel> ParentCategoryOptions { get; set; } = new();
    }

    public class CategoryFormViewModel
    {
        public int? Id { get; set; }

        // "category" or "subcategory" — set by JS on edit (data-kind on the edit button).
        // Ignored on Add: Add always infers Kind from whether ParentCategoryId is set.
        public string? Kind { get; set; }

        [Required(ErrorMessage = "Enter a name")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z\s']*$",
            ErrorMessage = "Name can only contain letters, spaces, and apostrophes — no numbers or symbols")]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Description { get; set; }

        // null = save as a top-level Category. Set = save as a SubCategory under this Category.
        public int? ParentCategoryId { get; set; }
    }

    public class ProductTypeFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Enter a product type name")]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z\s/()'-]*$",
            ErrorMessage = "Name can only contain letters — no numbers")]
        public string Name { get; set; } = "";
    }
}