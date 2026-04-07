using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = "Fake";

        public string Status { get; set; } = "Pending"; // Pending / Paid
    }
}