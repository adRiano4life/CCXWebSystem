using System;

namespace WebStudio.Models
{
    public class ResultsInputData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InputDataId { get; set; }
        
        public string Summ { get; set; }
        public string summTenge { get; set; }
        public string Broker { get; set; }
        public string NDSImport { get; set; }
        public string Investments { get; set; }
        
        public string tPay { get; set; }
        public string Bet { get; set; }
        public string Payouts { get; set; }
        public string Total { get; set; }
        public string NDS { get; set; }
        
        public string NDSTenge { get; set; }
        public string KPN { get; set; }
        public string KPNTenge { get; set; }
        public string EconomyNDS { get; set; }
        public string Profit { get; set; }
        
    }
}