using System;

namespace WebStudio.Models
{
    public class Currency
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public double Сourse { get; set; }
    }
}