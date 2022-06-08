namespace Test.Common
{
    public class ProductDto
    {
        public int ProductID { get; set; }
        public SupplierDto Supplier { get; set; }
        public int SupplierID { get; set; }
        public CategoryDto Category { get; set; }
        public int CategoryID { get; set; }
        public string ProductName { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public int ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
    }
}
