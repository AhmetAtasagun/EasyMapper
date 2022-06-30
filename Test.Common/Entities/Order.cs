using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Common.Entities
{
    public class Order
    {
        public int OrderID { get; set; }

        [Column(TypeName = "nchar(5)")]
        public string CustomerID { get; set; }

        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public ShipVia ShipVia { get; set; } = ShipVia.Harbor;

        [Column(TypeName = "money")]
        public decimal Freight { get; set; }

        [StringLength(40)]
        public string ShipName { get; set; }

        [StringLength(60)]
        public string ShipAddress { get; set; }

        [StringLength(15)]
        public string ShipCity { get; set; }

        [StringLength(15)]
        public string ShipRegion { get; set; }

        [StringLength(10)]
        public string ShipPostalCode { get; set; }

        [StringLength(15)]
        public string ShipCountry { get; set; }
    }
}
