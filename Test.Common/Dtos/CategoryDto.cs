using System.Collections.Generic;

namespace Test.Common
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
        public virtual List<ProductDto> Products { get; set; }
    }
}
