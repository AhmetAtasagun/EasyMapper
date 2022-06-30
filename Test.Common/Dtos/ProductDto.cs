using System;

namespace Test.Common.Dtos
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
        public Int16? UnitsInStock { get; set; }
        public int UnitsOnOrder { get; set; }
        public Int16 ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
    }
}
