namespace BogusExample_02.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTimeOffset CreationDate { get; set; }
        public ICollection<ProductProductCategory> ProductProductCategories { get; set; } = new List<ProductProductCategory>();
        public string Description { get; set; } = null!;
    }
}
