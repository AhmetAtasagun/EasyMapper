using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Common.Entities
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [StringLength(20)]
        [Required]
        public string LastName { get; set; }

        [StringLength(10)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(30)]
        public string Title { get; set; }

        [StringLength(25)]
        public string TitleOfCourtesy { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime? HireDate { get; set; }

        [StringLength(60)]
        public string Address { get; set; }

        [StringLength(15)]
        public string City { get; set; }

        [StringLength(15)]
        public string Region { get; set; }

        [StringLength(10)]
        public string PostalCode { get; set; }

        [StringLength(15)]
        public string Country { get; set; }

        [StringLength(24)]
        public string HomePhone { get; set; }

        [StringLength(4)]
        public string Extension { get; set; }

        [Column(TypeName = "image")]
        public byte[]? Photo { get; set; }

        [Column(TypeName = "ntext")]
        public string Notes { get; set; }

        public ReportsTo? ReportsTo { get; set; } = Common.ReportsTo.Chef;

        [StringLength(255)]
        public string PhotoPath { get; set; }
        public List<Order> Orders { get; set; }
    }
}
