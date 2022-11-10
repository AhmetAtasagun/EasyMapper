using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Test.Common.Entities;

namespace Test.Common
{
    public class Sql
    {
        public NorthwindDbContext Context;
        public Sql()
        {
            Context = new NorthwindDbContext();
        }

        public List<Category> GetCategoryList()
        {
            using (var context = new NorthwindDbContext())
            {
                var categories = context.Categories.Include("Products").ToList();
                return categories;
            }
        }

        public List<Supplier> GetSupplierList()
        {
            using (var context = new NorthwindDbContext())
            {
                var suppliers = context.Suppliers.Include("Products").ToList();
                return suppliers;
            }
        }

        public List<Product> GetProductList()
        {
            using (var context = new NorthwindDbContext())
            {
                var products = context.Products.Include("Category").Include("Supplier").ToList();
                return products;
            }
        }

        public List<Employee> GetEmployeeList()
        {
            using (var context = new NorthwindDbContext())
            {
                var products = context.Employees.Include("Orders").ToList();
                return products;
            }
        }

        public List<Order> GetOrderList()
        {
            using (var context = new NorthwindDbContext())
            {
                var products = context.Orders.Include("Employee").ToList();
                return products;
            }
        }

        public static bool UpdateSimulate<T>(T entitiy)
        {
            return true;
        }
    }
}
