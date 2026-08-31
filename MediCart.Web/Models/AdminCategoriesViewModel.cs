using System.ComponentModel.DataAnnotations;

namespace MediCart.Web.Models
{
    public class CategoryRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int SubCategoryCount { get; set; }
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
        public List<ProductTypeRowViewModel> ProductTypes { get; set; } = new();
        public List<CategoryOptionViewModel> ParentCategoryOptions { get; set; } = new();
    }

    public class CategoryFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Enter a category name")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
    }

    public class ProductTypeFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Enter a product type name")]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = "";
    }
}