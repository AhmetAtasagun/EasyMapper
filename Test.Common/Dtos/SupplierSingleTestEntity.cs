using Test.Common.Entities;

namespace Test.Common.Dtos
{
    public class SupplierSingleTestEntity
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public virtual Product Product { get; set; } // Manule çeviriyle uğraşmamak adına entity
    }
}
