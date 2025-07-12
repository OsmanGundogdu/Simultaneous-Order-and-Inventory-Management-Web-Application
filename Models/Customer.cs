using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using YazlabBirSonProje.Data;

namespace YazlabBirSonProje.Models
{
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public decimal Budget { get; set; }
        public string CustomerType { get; set; }
        public decimal TotalSpent { get; set; }
        public double PriorityScore { get; set; }
        public double PendingTime { get; set; }
    }
}