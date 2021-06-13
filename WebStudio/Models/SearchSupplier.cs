using System;
using System.Collections.Generic;

namespace WebStudio.Models
{
    public class SearchSupplier
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public List<string> Tags { get; set; }
    }
}