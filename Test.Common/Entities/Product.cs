using System;
using System.ComponentModel.DataAnnotations;

namespace Test.Common
{
    public class Product
    {
        public int ProductID { get; set; }

        [StringLength(40)]
        public string ProductName { get; set; }
        public Supplier Supplier { get; set; }
        public int SupplierID { get; set; }
        public Category Category { get; set; }
        public int CategoryID { get; set; }

        [StringLength(20)]
        public string QuantityPerUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public Int16 UnitsInStock { get; set; }
        public Int16 UnitsOnOrder { get; set; }
        public Int16 ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
    }
}
