using System.Collections.Generic;

namespace MediCart.Web.Models
{
    // Represents a Category row plus its SubCategories, for rendering the
    // full filter tree — independent of which medicines are currently
    // in stock/loaded, so empty categories still show up as filter options.
    public class CategoryFilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<SubCategoryFilterOption> SubCategories { get; set; } = new();
    }

    public class SubCategoryFilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class ProductTypeFilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
