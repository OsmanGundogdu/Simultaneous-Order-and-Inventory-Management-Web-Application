using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using YazlabBirSonProje.Data;

namespace YazlabBirSonProje.Models
{
    public class Logs
    {
        [Key]
        public int LogID { get; set; }
        public int? CustomerID { get; set; }
        public string CustomerType { get; set; } = string.Empty; 
        public string LogType { get; set; } = string.Empty;
        public int? OrderID { get; set; }
        public string ProductDetails { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        
    }
}