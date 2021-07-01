using System;
using System.Collections.Generic;

namespace WebStudio.Models
{
    public class AuctionResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Number { get; set; }
        public string Name { get; set; }
        public DateTime DateOfAuctionStart { get; set; }
        public DateTime? DateOfSignContract { get; set; }
        public string Winner { get; set; }
        public decimal? Sum { get; set; }
    }
}