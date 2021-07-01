using System;
using System.Collections.Generic;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class RequestIndexViewModel
    {
        public List<Card> Cards { get; set; }
        public List<Request> Requests { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; } = DateTime.Now;
        public string ExecutorName { get; set; }
    }
}