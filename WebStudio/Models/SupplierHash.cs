using System;

namespace WebStudio.Models
{
    public class SupplierHash
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Hash { get; set; }
    }
}