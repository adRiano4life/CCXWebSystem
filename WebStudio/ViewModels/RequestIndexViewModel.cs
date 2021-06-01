using System.Collections.Generic;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class RequestIndexViewModel
    {
        public List<Card> Cards { get; set; }
        public List<Request> Requests { get; set; }
    }
}