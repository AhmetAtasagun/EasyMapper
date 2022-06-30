using EasyMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Test.Common;
using Test.Common.Dtos;
using Test.Common.Entities;

namespace Console.TestApp
{
    public class Test
    {
        public static List<Supplier> suppliers;
        public static List<Category> categories;
        public static List<Product> products;
        public static List<Employee> employees;
        public static List<Order> orders;
        TestUI testUI = new TestUI();
        public Test()
        {
            var query = new Sql();
            suppliers = query.GetSupplierList();
            categories = query.GetCategoryList();
            products = query.GetProductList();
            employees = query.GetEmployeeList();
            orders = query.GetOrderList();
        }

        public void ListOfProductToProductDto()
        {
            testUI.Run("Case 1", () =>
            {
                var entityProducts = products.Take(3).ToList();
                var dtoProducts = entityProducts.ToMap<ProductDto>().ToList();
                TestUI.MatchOldAndNewData(entityProducts, dtoProducts);
            });
        }

        public void ProductToProductDto()
        {
            testUI.Run("Case 2", () =>
            {
                var entityProduct = products.LastOrDefault();
                var dtoProduct = entityProduct.ToMap<ProductDto>();
                TestUI.MatchOldAndNewData(entityProduct, dtoProduct);
            });
        }

        public void EmployeesToEmployeesDto()
        {
            testUI.Run("Case 3", () =>
            {
                var employees3 = employees.ToList();
                var employeeDtos = employees3.ToMap<EmployeeDto>().ToList();
                TestUI.MatchOldAndNewData(employees3, employeeDtos);
            });
        }

        public void EmployeeUpdateToEmployeeDto()
        {
            testUI.Run("Case 4", () =>
            {
                var paramEmployee = new EmployeeDto
                {
                    FirstName = "Ahmet",
                    LastName = "Ata",
                    Address = "Sille Selçuklu",
                    City = "KONYA",
                    Title = "Yazılım Geliştirici"
                };
                var employee = employees.First();
                var newEmployee = paramEmployee.ToMap(employee);
                TestUI.MatchOldAndNewData(employee, newEmployee);
            });
        }

        public void Func5()
        {
            testUI.Run("Case 5", () =>
            {

            });
        }

        public void Func6()
        {
            testUI.Run("Case 6", () =>
            {

            });
        }
    }

    public class TestUI
    {
        public void Run(string methodName, Action action)
        {
            StartInfo(methodName);
            action();
            FinishInfo(methodName);
        }
        public static void StartInfo(string methodName)
        {
            System.Console.WriteLine($"\n{methodName} is started...");
        }

        public static void FinishInfo(string methodName)
        {
            System.Console.WriteLine($"{methodName} is finished.\n");
        }

        public static void MatchOldAndNewData<TEntity, TDto>(List<TEntity> entitesList, List<TDto> dtosList)
        {
            var titleColumn = 3;
            var dataColum = 30;
            var rightColumnStart = 60;
            for (int i = 0; i < entitesList.Count; i++)
            {
                System.Console.WriteLine("");
                var props = entitesList[i].GetType().GetProperties();
                var mapProps = dtosList[i].GetType().GetProperties();
                ConsoleSetColumAndWrite(dataColum, $"*** {props[i].DeclaringType.Name} ***");
                ConsoleSetColumAndWriteLine(rightColumnStart, $"*** {mapProps[i].DeclaringType.Name} ***");
                for (int j = 0; j < props.Length; j++)
                {
                    ConsoleSetColumAndWrite(titleColumn, $"{props[j].Name} : ");
                    ConsoleSetColumAndWrite(dataColum, $"{props[j].GetValue(entitesList[i])}");
                    ConsoleSetColumAndWriteLine(rightColumnStart, $"{mapProps[j].GetValue(dtosList[i])}");
                }
            }
        }

        public static void MatchOldAndNewData<TEntity, TDto>(TEntity entity, TDto dto)
        {
            var titleColumn = 3;
            var dataColum = 30;
            var rightColumnStart = 90;
            System.Console.WriteLine("");
            var props = entity.GetType().GetProperties();
            var mapProps = dto.GetType().GetProperties();
            ConsoleSetColumAndWrite(dataColum, $"*** {entity.GetType().Name} ***");
            ConsoleSetColumAndWriteLine(rightColumnStart, $"*** {dto.GetType().Name} ***");
            for (int j = 0; j < props.Length; j++)
            {
                ConsoleSetColumAndWrite(titleColumn, $"{props[j].Name} : ");
                ConsoleSetColumAndWrite(dataColum, $"{props[j].GetValue(entity)}");
                ConsoleSetColumAndWriteLine(rightColumnStart, $"{mapProps[j].GetValue(dto)}");
            }
        }

        static void ConsoleSetColumAndWrite(int column, string text)
        {
            System.Console.SetCursorPosition(column, System.Console.CursorTop);
            System.Console.Write(text);
        }

        static void ConsoleSetColumAndWriteLine(int column, string text)
        {
            System.Console.SetCursorPosition(column, System.Console.CursorTop);
            System.Console.WriteLine(text);
        }
    }

    #region Temp
    //class MyClass
    //{
    //    public void MyMethod()
    //    {
    //        suppliersSingleProduct = suppliers.Select(s => new SupplierSingleTestEntity
    //        {
    //            SupplierID = s.SupplierID,
    //            CompanyName = s.CompanyName,
    //            Product = s.Products.Select(sp => new Product
    //            {
    //                Category = sp.Category,
    //                CategoryID = sp.CategoryID,
    //                ProductName = sp.ProductName,
    //                Discontinued = sp.Discontinued,
    //                ProductID = sp.ProductID,
    //                QuantityPerUnit = sp.QuantityPerUnit,
    //                ReorderLevel = sp.ReorderLevel,
    //                Supplier = sp.Supplier,
    //                SupplierID = sp.SupplierID,
    //                UnitPrice = sp.UnitPrice,
    //                UnitsInStock = sp.UnitsInStock,
    //                UnitsOnOrder = sp.UnitsOnOrder
    //            }).FirstOrDefault(),
    //        }).ToList();


    //        // manuel Mapping
    //        var singleTestEntity = suppliersSingleProduct.FirstOrDefault();
    //        var singleTestListEntities = suppliers.FirstOrDefault();
    //        var exam1Dest = new SupplierSingleTestDto
    //        {
    //            SupplierID = singleTestEntity.SupplierID,
    //            CompanyName = singleTestEntity.CompanyName,
    //            Product = new ProductDto
    //            {
    //                ProductID = singleTestEntity.Product.ProductID,
    //                SupplierID = singleTestEntity.Product.SupplierID,
    //                CategoryID = singleTestEntity.Product.CategoryID,
    //                ProductName = singleTestEntity.Product.ProductName,
    //                UnitsInStock = singleTestEntity.Product.UnitsInStock,
    //                UnitPrice = singleTestEntity.Product.UnitPrice,
    //                Discontinued = singleTestEntity.Product.Discontinued.GetValueOrDefault(),
    //                QuantityPerUnit = singleTestEntity.Product.QuantityPerUnit,
    //                ReorderLevel = singleTestEntity.Product.ReorderLevel,
    //                UnitsOnOrder = singleTestEntity.Product.UnitsOnOrder,
    //                Category = null,
    //                Supplier = null,
    //            }
    //        };

    //        #region Independent Data Mapping
    //        // EasyMapping (Single entity Inline Single entity)
    //        var singleTestDto = singleTestEntity.ToMap<SupplierSingleTestDto>(); // Status : 

    //        // EasyMapping (List entity Inline Single entity)
    //        var listTestDto = products.ToMap<ProductDto>().ToList(); // Status : 

    //        // EasyMapping (List entity Inline Single entity)
    //        var supplierDto = singleTestListEntities.ToMap<SupplierDto>(); // Status : 

    //        // EasyMapping (List entity Inline List entity)
    //        var categoryList = categories.ToMap<CategoryDto>().ToList(); // Status : 

    //        //------------------
    //        var opt = new MapOptions().GetDefaultOptions();

    //        // EasyMapping (Single entity Inline Single entity)
    //        var singleTestDto2 = singleTestEntity.ToMap<SupplierSingleTestDto>(opt); // Status : 

    //        // EasyMapping (List entity Inline Single entity)
    //        var listTestDto2 = products.ToMap<ProductDto>(opt).ToList(); // Status : 

    //        var opt8 = new MapOptions(GenerationLevel.Eighth, "SupplierID");
    //        // EasyMapping (List entity Inline Single entity)
    //        var supplierDto2 = singleTestListEntities.ToMap<SupplierDto>(opt8); // Status : 

    //        // EasyMapping (List entity Inline List entity)
    //        var categoryList2 = categories.ToMap<CategoryDto>(opt8).ToList(); // Status : 

    //        // Data in UI
    //        foreach (var category in categoryList)
    //        {
    //            var productsString = "";
    //            foreach (var product in category.Products)
    //            {
    //                productsString += $"\tProduct\t\t=> {product.ProductID} : {product.ProductName}\n";
    //            }
    //            System.Console.WriteLine($"{category.CategoryID} : {category.CategoryName}\n{productsString}\n");
    //        }
    //        #endregion

    //        System.Console.WriteLine("Independent Data Mapping Completed!");

    //        #region In Query Data Mapping (EF Core)
    //        var singleTestDto3 = new Sql().Context.Suppliers
    //            .Select(s => new SupplierSingleTestDto
    //            {
    //                CompanyName = s.CompanyName,
    //                SupplierID = s.SupplierID,
    //                Product = s.Products.FirstOrDefault().ToMap<ProductDto>(),
    //            }).FirstOrDefault();

    //        var singleTestDto4 = new Sql().Context.Suppliers
    //            .Select(s => s.ToMap<SupplierSingleTestDto>())
    //            .FirstOrDefault();

    //        var singleTestDto5 = new Sql().Context.Suppliers
    //            .Select(s => s.ToMap<SupplierTestDto>())
    //            .FirstOrDefault();

    //        var singleTestDto6 = new Sql().Context.Suppliers.Include(i => i.Products)
    //            .Select(s => new SupplierSingleTestDto
    //            {
    //                CompanyName = s.CompanyName,
    //                SupplierID = s.SupplierID,
    //                Product = s.Products.FirstOrDefault().ToMap<ProductDto>(),
    //            }).FirstOrDefault();

    //        var singleTestDto7 = new Sql().Context.Suppliers.Include(i => i.Products)
    //            .Select(s => s.ToMap<SupplierSingleTestDto>())
    //            .FirstOrDefault();

    //        var singleTestDto8 = new Sql().Context.Suppliers.Include(i => i.Products)
    //            .Select(s => s.ToMap<SupplierTestDto>())
    //            .FirstOrDefault();

    //        var singleTestDto9 = new Sql().Context.Suppliers.Include(i => i.Products)
    //            .Select(s => s.ToMap<SupplierTestDto>(opt8))
    //            .FirstOrDefault();

    //        var productTest = new Sql().Context.Products.ToMap<ProductDto>().ToList();
    //        var productTest2 = new Sql().Context.Products.Include("Category").ToMap<ProductDto>().ToList();
    //        var productTest3 = new Sql().Context.Products.Include("Category").ToMap<ProductDto>(opt8).ToList();

    //        var testProductInstance = new object().ToMap<ProductDto>(); // No Error, CreateInstance Success!

    //        #endregion

    //        System.Console.WriteLine("In Query Data Mapping Completed!");
    //    }
    //} 
    #endregion
}
