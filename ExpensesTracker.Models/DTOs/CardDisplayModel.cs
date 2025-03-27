using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesTracker.Services.DTOs
{
    public class CardDisplayModel
    {
        public decimal TotalExpenses { get; set; }
        public decimal TotalWants { get; set; }
        public decimal TotalNeeds { get; set; }
    }
}
