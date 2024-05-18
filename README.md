# EasyMapper
### __ [EN] __
## Write, Let It Work! No confusing settings, no tedious details.
    No dependency injection!
    No predefined class!
    No complicated settings!
This library is a mapper built on extensions to get rid of pre-coding pre-configurations and to provide ease of writing. After including the library in your project, you can start coding without any pre-configuration.

<br>

### __ [TR] __
## Yaz, Çalışsın! Kafa karıştırıcı ayarlar yok, uğraştıran detaylar yok.
    Bağımlılık enjeksiyonu yok!
    Ön tanımlama sınıfı yok!
    Karmaşık ayarlar yok!
Bu kütüphane, kodlama öncesi ön yapılandırmalardan kurtulmak ve yazım kolaylığı sağlamak adına uzantılar üzerine kurulmuş bir haritalayıcıdır. Kütüphaneyi projenize dahil ettikten sonra hiçbir ön yapılandırma gerekmeden kodlamaya başlayabilirsiniz.

&nbsp;<br>
&nbsp;<br>
&nbsp;<br>

---
### **Add to Project**
---

Nuget Package Manager > Search : "Utilities.EasyMapper"<br>
Select EasyMapper library and install right side
&nbsp;<br>
<a href="https://www.nuget.org/packages/Atasagun.Utilities.EasyMapper" target="_blank">
    ![Static Badge](https://img.shields.io/badge/Nuget-V.1.0.1-blue)
</a>
&nbsp;<br>
&nbsp;<br>
&nbsp;<br>
&nbsp;<br>

---
### **Basic Usage**
---
<br>

&emsp;*__<ins>Manual Mapping<ins> __*
```csharp
List<ProductDto> productDtos = context.Products
    .Select(s => new ProductDto {
        ProductID = s.ProductID, // int
        Category = s.Category, // class
        ProductName = s.ProductName, // string
        Discontinued = s.Discontinued, // bool
        UnitPrice = s.UnitPrice, // decimal
        Supplier = s.Supplier, // class
        UnitsInStock = s.UnitsInStock, // short
    }).ToList();

ProductDto productDto = context.Products
    .Select(s => new ProductDto {
        ProductID = s.ProductID, // int
        Category = s.Category, // class
        ProductName = s.ProductName, // string
        Discontinued = s.Discontinued, // bool
        UnitPrice = s.UnitPrice, // decimal
        Supplier = s.Supplier, // class
        UnitsInStock = s.UnitsInStock, // short
    }).FirstOrDefault();
```
&emsp;*__<ins>EasyMapper Mapping (new object)<ins> __*
```csharp
List<ProductDto> productDtos = context.Products
    .Include("Category").Include("Supplier")
    .ToMap<ProductDto>().ToList();

ProductDto productDto = context.Products
    .Include("Category").Include("Supplier")
    .FirstOrDefault().ToMap<ProductDto>();

ProductDto productDto = context.Products
    .Include("Category").Include("Supplier")
    .FirstOrDefault().ToMap(typeof(ProductDto));

```
&emsp;*__<ins>EasyMapper Mapping (new and update object)<ins> __*
```csharp
public void MyCustomMethod(ProductDto productDto)
{
    Product product = context.Products.FirstOrDefault();
    // productDto fields updated from product not default value fields.
    productDto = product.ToMap(productDto);
}
/* Note : Update, Just for Single Mapping!... */
```
&emsp;*__<ins>EasyMapper Mapping With Options (new object)<ins> __*
```csharp
var option = new MapOptions(GenerationLevel.Third, "UnitPrice", "UnitsInStock");
List<ProductDto> productDtos = context.Products.ToMap<ProductDto>(option).ToList();

```
Note : &emsp; Entities (Supplier, Category) under Products must not be null!<br>
&emsp;&emsp;&emsp;&emsp; Update ToMap(...), Just for Single Mapping!...


&nbsp;<br>
&nbsp;<br>
&nbsp;<br>

---
### **Test Objects**
---
<br>

```csharp
// Getting Relational DB data by EfCore DbContext
NorthwindDbContext context = new NorthwindDbContext(/*ConnectionString*/);

// ------- PRODUCTS -------
List<Product> products = context.Products
    .Include("Category").Include("Supplier")
    .Select(s => new Product {
        ProductID = s.ProductID, // int
        Category = s.Category, // class
        ProductName = s.ProductName, // string
        Discontinued = s.Discontinued, // bool
        UnitPrice = s.UnitPrice, // decimal
        Supplier = s.Supplier, // class
        UnitsInStock = s.UnitsInStock, // short
    }).Take(3).ToList();
/*
    {List<Project.Product>}
        {Project.Product}
            ...
        {Project.Product}
            ...
        {Project.Product}
            ProductID = 5,
            Category = {Project.Category},
            ProductName = "Mishi Kobe Niku",
            Discontinued = true,
            UnitPrice = 21.0000,
            Supplier = {Project.Supplier},
            UnitsInStock = 15
*/
// --------------------------

// ------- CATEGORIES -------
List<Product> products = context.Categories.Include("Products")
    .Select(s => new Category {
        CategoryID = s.CategoryID, // int
        CategoryName = s.CategoryName, // string
        Picture = s.Picture, // byte[]
        Products = s.Products, // {List<Product>}
    }).Take(3).ToList();
/*
    {List<Project.Category>}
        {Project.Category}
            ...
        {Project.Category}
            ...
        {Project.Category}
            CategoryID = 3,
            CategoryName = "Beverages"
            Picture = {byte[10746]}
            Products = {List<Product>}
*/
// -------------------------

// ------- SUPPLIERS -------
List<Product> products = context.Suppliers.Include("Products")
    .Select(s => new Supplier {
        SupplierID = s.SupplierID, // int
        CompanyName = s.CompanyName, // string
        Address = s.Address, // string
        Phone = s.Phone, // string
        Products = s.Products, // {List<Product>}
    }).Take(3).ToList();
/*
    {List<Project.Supplier>}
        {Project.Supplier}
            ...
        {Project.Supplier}
            ...
        {Project.Supplier}
            SupplierID = 4,
            CompanyName = "Pavlova, Ltd."
            Address = "74 Rose St. Moonie Ponds"
            Phone = "(03) 444-2343"
            Products = {List<Product>}
*/
// --------------------------
```
Note : &emsp; ProductDto, Product entity has same fields. <br>
&emsp;&emsp;&emsp;&emsp; CategoryDto, Category entity has same fields. <br>
&emsp;&emsp;&emsp;&emsp; SupplierDto, Supplier entity has same fields

&nbsp;<br>
&nbsp;<br>
&nbsp;<br>


---
### **Detailed Usage**
---
<br>

&emsp;*__<ins>Single Model Mapping<ins> __*

&emsp;`TDestination ToMap<TDestination>(this object source)`
```csharp
ProductDto productDto = context.Products
    .FirstOrDefault().ToMap<ProductDto>();
/*
    {Project.ProductDto}
        ProductID = 5,
        Category = null,
        ProductName = "Mishi Kobe Niku",
        Discontinued = true,
        UnitPrice = 21.0000,
        Supplier = null,
        UnitsInStock = 15
*/

Product product = context.Products
    .Include("Category")//.Include("Supplier") => Not Include
    .FirstOrDefault();
    // or
Product product = context.Products
    .Select(s => new Product {
        ProductID = s.ProductID,
        Category = s.Category,
        ProductName = s.ProductName,
        Discontinued = s.Discontinued,
        UnitPrice = s.UnitPrice,
        // Supplier = s.Supplier,
        UnitsInStock = s.UnitsInStock,
    }).FirstOrDefault();
    
ProductDto productDto = product.ToMap<ProductDto>();
/*
    {Project.Product}
        ProductID = 5,
        Category = {Project.Category},
            ...
            Products = null
        ProductName = "Mishi Kobe Niku",
        Discontinued = true,
        UnitPrice = 21.0000,
        Supplier = null,
        UnitsInStock = 15
*/

Product product = context.Products
    .Include("Category").Include("Supplier")
    .FirstOrDefault();

ProductDto productDto = product.ToMap<ProductDto>();
/*
    {Project.Product}
        ProductID = 5,
        Category = {Project.Category},
            ...
            Products = null
        ProductName = "Mishi Kobe Niku",
        Discontinued = true,
        UnitPrice = 21.0000,
        Supplier = {Project.SupplierDto},
            ...
            Products = null
        UnitsInStock = 15
*/
```

<br>

&emsp;*__<ins>Single Model Mapping With Options<ins> __*

&emsp;`TDestination ToMap<TDestination>(this object source, MapOptions options)`
```csharp
Product product = context.Products
    .Include("Category").Include("Supplier")
    .FirstOrDefault();

var option = new MapOptions(GenerationLevel.Third);
ProductDto productDto = product.ToMap<ProductDto>(option);
/*
    {Project.Product}
        ProductID = 5,
        Category = {Project.Category},
            ...
            Products = {List<Project.Product>}
                {Project.Product}
                    ...
                    Supplier = {Project.SupplierDto},
                        ...
                        Products = null,
                    Category = {Project.Category},
                        ...
                        Products = null,
                {Project.Product}
                    ...
                    Supplier = {Project.SupplierDto},
                        ...
                        Products = null,
                    Category = {Project.Category},
                        ...
                        Products = null,
                {Project.Product}
                    ...
                    Supplier = {Project.SupplierDto},
                        ...
                        Products = null,
                    Category = {Project.Category},
                        ...
                        Products = null,
        ProductName = "Mishi Kobe Niku",
        Discontinued = true,
        UnitPrice = 21.0000,
        Supplier = {Project.SupplierDto},
            ...
            Products = {List<Project.Product>}
                {Project.Product}
                    ...
                    Supplier = {Project.SupplierDto},
                        ...
                        Products = null,
                    Category = {Project.Category},
                        ...
                        Products = null,
                {Project.Product}
                    ...
                    Supplier = {Project.SupplierDto},
                        ...
                        Products = null,
                    Category = {Project.Category},
                        ...
                        Products = null,
                {Project.Product}
                    ...
                    Supplier = {Project.SupplierDto},
                        ...
                        Products = null,
                    Category = {Project.Category},
                        ...
                        Products = null,
        UnitsInStock = 15
*/
var option = new MapOptions(GenerationLevel.Second, "UnitPrice", "ProductName");
ProductDto productDto = product.ToMap<ProductDto>(option);
/*
    {Project.Product}
        ProductID = 5,
        Category = {Project.Category},
            ...
            Products = {List<Project.Product>}
                {Project.Product}
                    ...
                    Supplier = null,
                    Category = null,
                {Project.Product}
                    ...
                    Supplier = null,
                    Category = null,
                {Project.Product}
                    ...
                    Supplier = null,
                    Category = null,
        ProductName = "",                       (Ignored)
        Discontinued = true,
        UnitPrice = 0,                          (Ignored)
        Supplier = {Project.SupplierDto},
            ...
            Products = {List<Project.Product>}
                {Project.Product}
                    ...
                    Supplier = null,
                    Category = null,
                {Project.Product}
                    ...
                    Supplier = null,
                    Category = null,
                {Project.Product}
                    ...
                    Supplier = null,
                    Category = null,
        UnitsInStock = 15
*/
```

<br>

&emsp;*__<ins>Single Model Update Mapping<ins> __*

&emsp;`TDestination ToMap<TDestination>(this object source, TDestination destination)`
```csharp
public void MyMethod(ProductFormDto productFormDto)
{
/*
    {Project.ProductFormDto}
        ProductName = "Sushi",
        UnitPrice = 19.0000,
*/
    Product product = context.Products.FirstOrDefault();
/*
    {Project.Product}
        ProductID = 5,
        Category = null,
        ProductName = "Mishi Kobe Niku",
        Discontinued = true,
        UnitPrice = 21.0000,
        Supplier = null,
        UnitsInStock = 15
*/
    product = productFormDto.ToMap(product);
/*
    {Project.Product}
        ProductID = 5,
        Category = null,
        ProductName = "Sushi",      (Updated)
        Discontinued = true,
        UnitPrice = 19.0000,        (Updated)
        Supplier = null,
        UnitsInStock = 15
*/
    context.Products.Add(product);
    context.SaveChanges();
}

//------ If we use normal; --------- 
product = productFormDto.ToMap<Product>();
/*
    {Project.Product}
        ProductID = 0,              (default)
        Category = null,            (default)
        ProductName = "Sushi",
        Discontinued = false,       (default)
        UnitPrice = 19.0000,
        Supplier = null,            (default)
        UnitsInStock = 0            (default)
------------------------------------*/
```
&emsp;*__<ins>List Model Mapping<ins> __*

&emsp;`IEnumerable<TDestination> ToMap<TDestination>(this object IEnumerable<source>)`
```csharp
List<ProductDto> products = context.Products
    .Select(s => new ProductDto {
        ProductID = s.ProductID, // int
        Category = s.Category, // class
        ProductName = s.ProductName, // string
        Discontinued = s.Discontinued, // bool
        UnitPrice = s.UnitPrice, // decimal
        Supplier = s.Supplier, // class
        UnitsInStock = s.UnitsInStock, // short
    }).ToList();
    // or
List<ProductDto> productDtos = context.Products
    .Include("Category").Include("Supplier")
    .ToMap<ProductDto>().ToList();
/*
    {List<Project.ProductDto>}
        {Project.ProductDto}
            ...
        {Project.ProductDto}
            ...
        {Project.ProductDto}
            ProductID = 5,
            Category = {Project.CategoryDto},
            ProductName = "Mishi Kobe Niku",
            Discontinued = true,
            UnitPrice = 21.0000,
            Supplier = {Project.SupplierDto},
            UnitsInStock = 15
*/
//-------------------------------------------
List<ProductDto> products = context.Products
    .Select(s => new ProductDto {
        ProductID = s.ProductID, // int
        // Category = s.Category, // class
        ProductName = s.ProductName, // string
        Discontinued = s.Discontinued, // bool
        UnitPrice = s.UnitPrice, // decimal
        // Supplier = s.Supplier, // class
        UnitsInStock = s.UnitsInStock, // short
    }).ToList();
    // or
List<ProductDto> productDtos = context.Products
    // .Include("Category").Include("Supplier")  => Not Includes
    .ToMap<ProductDto>().ToList();
/*
    {List<Project.ProductDto>}
        {Project.ProductDto}
            ...
        {Project.ProductDto}
            ...
        {Project.ProductDto}
            ProductID = 5,
            Category = null,
            ProductName = "Mishi Kobe Niku",
            Discontinued = true,
            UnitPrice = 21.0000,
            Supplier = null,
            UnitsInStock = 15
*/
```
&emsp;*__<ins>List Model Mapping With Options<ins> __*

&emsp;`IEnumerable<TDestination> ToMap<TDestination>(this object IEnumerable<source>, MapOptions options)`
```csharp
var option = new MapOptions(GenerationLevel.Second, "UnitPrice", "UnitsInStock");

List<ProductDto> productDtos = context.Products
    .Include("Category").Include("Supplier")
    .ToMap<ProductDto>(option).ToList();
/*
    {List<Project.ProductDto>}
        {Project.ProductDto}
            ProductID = 5,
            Category = {Project.Category},
                ...
                Products = {List<Project.Product>}
                    {Project.Product}
                        ...
                        Supplier = null,
                        Category = null,
                    {Project.Product}
                        ...
                        Supplier = null,
                        Category = null,
                    {Project.Product}
                        ...
                        Supplier = null,
                        Category = null,
            ProductName = "Mishi Kobe Niku",                       
            Discontinued = true,
            UnitPrice = 0,                          (Ignored)
            Supplier = {Project.SupplierDto},
                ...
                Products = {List<Project.Product>}
                    {Project.Product}
                        ...
                        Supplier = null,
                        Category = null,
                    {Project.Product}
                        ...
                        Supplier = null,
                        Category = null,
                    {Project.Product}
                        ...
                        Supplier = null,
                        Category = null,
            UnitsInStock = 0                        (Ignored)
*/
```
