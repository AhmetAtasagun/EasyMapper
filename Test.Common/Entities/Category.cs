using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Test.Common.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }

        [StringLength(15)]
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public virtual List<Product> Products { get; set; }
    }
}
