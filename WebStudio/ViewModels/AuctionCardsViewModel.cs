using System.Collections.Generic;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class AuctionCardsViewModel
    {
        public List<CardClone> Cards { get; set; }
        public string ExecutorName { get; set; }
    }
}