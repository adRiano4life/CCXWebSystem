using System;
using System.Collections.Generic;
using WebStudio.Enums;

namespace WebStudio.Models
{
    public class CardClone
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Number { get; set; }
        public string Name { get; set; }
        public decimal StartSumm { get; set; }
        public DateTime DateOfAcceptingEnd { get; set; }
        public DateTime DateOfAuctionStart { get; set; }
        public string Initiator { get; set; }
        public string Broker { get; set; }
        public string Auction { get; set; }
        public string State { get; set; }
        public string BestPrice { get; set; }
        public CardState CardState { get; set; } = CardState.Новая;
        public List<string> Links { get; set; }
        public List<string> LinkNames { get; set; }
        public string Comment { get; set; }
        public virtual List<CardPosition> Positions { get; set; }
        public int Bidding { get; set; }
        public string ExecutorId { get; set; }
        public virtual User Executor { get; set; }
    }
}