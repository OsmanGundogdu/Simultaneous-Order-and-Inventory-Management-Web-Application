using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YazlabBirSonProje.Models
{
    public class OrderViewModel
    {
        public List<int> ProductIDs { get; set; } = new();
        public List<int> Quantities { get; set; } = new();
    }
}