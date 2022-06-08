using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Common
{
    public class SupplierSingleTestDto
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public virtual ProductDto Product { get; set; }
    }
}
