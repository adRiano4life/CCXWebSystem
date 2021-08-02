using System;

namespace WebStudio.Models
{
    public class InputData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OfferId { get; set; }
        public string PositionId { get; set; }
                
        public string Currency { get; set; }
        public string Prepay { get; set; }
        public string NDS { get; set; }
        public string KPN { get; set; }
        public string PayMethod { get; set; }
        public string DelivTerm { get; set; }
        public string Meas { get; set; } 
        public string Purchase { get; set; }
        public string Amount { get; set; }
        public string Bet { get; set; }
        public string Duty { get; set; }
        public string Transport { get; set; }
        public string Administrative { get; set; }
        public string TermPayment { get; set; }
        public string City { get; set; }
        public DateTime DeliveryTime { get; set; }
        public string Description { get; set; }
        
        
        

        
        
        
        
    }
}