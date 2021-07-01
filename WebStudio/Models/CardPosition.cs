using System;
using System.Threading;

namespace WebStudio.Models
{
    public class CardPosition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string StockNumber { get; set; }
        public string CodTNVED { get; set; }
        public string Name { get; set; }
        public string Measure { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryTerms { get; set; }
        
        public string CardId { get; set; }
        public virtual Card Card { get; set; }
    }
}