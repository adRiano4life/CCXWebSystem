using System;
using System.Collections.Generic;

namespace EZParser.jsonmodel
{
    public class Trader
    {
        public string name { get; set; }
        public string phone { get; set; }
    }

    public class Client
    {
        public string name { get; set; }
        public string idn { get; set; }
    }

    public class Result
    {
        public string number { get; set; }
        public string name { get; set; }
        public string sum { get; set; }
        public DateTime submission_end { get; set; }
        public DateTime bidding_begin { get; set; }
        public Trader trader { get; set; }
        public Client client { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string documents { get; set; }
        public object participants { get; set; }
        public string best_sum { get; set; }
    }

    public class Root
    {
        public int total { get; set; }
        public List<Result> results { get; set; }
    }
}