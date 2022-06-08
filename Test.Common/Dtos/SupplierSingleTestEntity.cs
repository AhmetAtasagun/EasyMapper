using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Common
{
    public class SupplierSingleTestEntity
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public virtual Product Product { get; set; }
    }
}
