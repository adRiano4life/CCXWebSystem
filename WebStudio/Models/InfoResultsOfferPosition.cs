using System;

namespace WebStudio.Models
{
    public class InfoResultsOfferPosition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string OfferId { get; set; }
        public virtual Offer Offer { get; set; }
        
        public string PositionId { get; set; }
        public virtual CardPosition Position { get; set; }
        
        public string InputDataId { get; set; }
        public virtual InputData InputData { get; set; }
        
        public string ResultsInputDataId { get; set; }
        public virtual ResultsInputData ResultsInputData { get; set; }
        
        
    }
}