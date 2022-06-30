namespace Test.Common.Dtos
{
    public class SupplierSingleTestDto
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
        public virtual ProductDto Product { get; set; }
    }
}
