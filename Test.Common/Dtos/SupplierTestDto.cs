using System.Collections.Generic;

namespace Test.Common
{
    public class SupplierTestDto
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public virtual List<ProductDto> Products { get; set; }
    }
}
