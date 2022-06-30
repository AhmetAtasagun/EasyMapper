using EasyMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Test.Common;
using Test.Common.Dtos;
using Test.Common.Entities;

namespace Console.TestApp5
{
    internal class Program
    {
        public static List<Supplier> suppliers;
        public static List<SupplierSingleTestEntity> suppliersSingleProduct;
        public static List<Category> categories;
        public static List<Product> products;
        static void Main(string[] args)
        {
            System.Console.WriteLine("Program başladı (Core 5.0)");
            var query = new Sql();
            suppliers = query.GetSupplierList();
            suppliersSingleProduct = suppliers.Select(s => new SupplierSingleTestEntity
            {
                SupplierID = s.SupplierID,
                CompanyName = s.CompanyName,
                Product = s.Products.FirstOrDefault()
            }).Take(3).ToList();
            categories = query.GetCategoryList();
            products = query.GetProductList();

            StartTesting();
        }

        private static void StartTesting()
        {
            // manuel Mapping
            var singleTestEntity = suppliersSingleProduct.FirstOrDefault();
            var singleTestListEntities = suppliers.FirstOrDefault();
            var exam1Dest = new SupplierSingleTestDto
            {
                SupplierID = singleTestEntity.SupplierID,
                CompanyName = singleTestEntity.CompanyName,
                Product = new ProductDto
                {
                    ProductID = singleTestEntity.Product.ProductID,
                    SupplierID = singleTestEntity.Product.SupplierID,
                    CategoryID = singleTestEntity.Product.CategoryID,
                    ProductName = singleTestEntity.Product.ProductName,
                    UnitsInStock = singleTestEntity.Product.UnitsInStock,
                    UnitPrice = singleTestEntity.Product.UnitPrice,
                    Discontinued = singleTestEntity.Product.Discontinued.GetValueOrDefault(),
                    QuantityPerUnit = singleTestEntity.Product.QuantityPerUnit,
                    ReorderLevel = singleTestEntity.Product.ReorderLevel,
                    UnitsOnOrder = singleTestEntity.Product.UnitsOnOrder,
                    Category = null,
                    Supplier = null,
                }
            };

            #region Independent Data Mapping
            // EasyMapping (Single entity Inline Single entity)
            var singleTestDto = singleTestEntity.ToMap<SupplierSingleTestDto>(); // Status : OK

            // EasyMapping (List entity Inline Single entity)
            var listTestDto = products.ToMap<ProductDto>().ToList(); // Status : Failed

            // EasyMapping (List entity Inline Single entity)
            var supplierDto = singleTestListEntities.ToMap<SupplierDto>(); // Status : OK

            // EasyMapping (List entity Inline List entity)
            var categoryList = categories.ToMap<CategoryDto>().ToList(); // Status : OK

            // Data in UI
            foreach (var category in categoryList)
            {
                var productsString = "";
                foreach (var product in category.Products)
                {
                    productsString += $"\tProduct\t\t=> {product.ProductID} : {product.ProductName}\n";
                }
                System.Console.WriteLine($"{category.CategoryID} : {category.CategoryName}\n{productsString}\n");
            }
            #endregion

            System.Console.WriteLine("Independent Data Mapping Completed!");

            #region In Query Data Mapping (EF Core)
            var singleTestDto3 = new Sql().Context.Suppliers
                .Select(s => new SupplierSingleTestDto
                {
                    CompanyName = s.CompanyName,
                    SupplierID = s.SupplierID,
                    Product = s.Products.FirstOrDefault().ToMap<ProductDto>(), // Status : 
                }).FirstOrDefault();

            var singleTestDto4 = new Sql().Context.Suppliers
                .Select(s => s.ToMap<SupplierSingleTestDto>()) // Status : 
                .FirstOrDefault();

            var singleTestDto5 = new Sql().Context.Suppliers
                .Select(s => s.ToMap<SupplierTestDto>()) // Status : 
                .FirstOrDefault();

            var singleTestDto6 = new Sql().Context.Suppliers.Include(i => i.Products)
                .Select(s => new SupplierSingleTestDto
                {
                    CompanyName = s.CompanyName,
                    SupplierID = s.SupplierID,
                    Product = s.Products.FirstOrDefault().ToMap<ProductDto>(), // Status : 
                }).FirstOrDefault();

            var singleTestDto7 = new Sql().Context.Suppliers.Include(i => i.Products)
                .Select(s => s.ToMap<SupplierSingleTestDto>()) // Status : 
                .FirstOrDefault();

            var singleTestDto8 = new Sql().Context.Suppliers.Include(i => i.Products)
                .Select(s => s.ToMap<SupplierTestDto>()) // Status : 
                .FirstOrDefault();

            var singleTestDto9 = new Sql().Context.Suppliers.Include(i => i.Products)
                .Select(s => s.ToMap(typeof(SupplierTestDto))) // Status : 
                .FirstOrDefault();

            #endregion

            System.Console.WriteLine("In Query Data Mapping Completed!");
        }
    }
}
