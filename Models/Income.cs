using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project1.Models
{
    public class Income
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
    }
}
