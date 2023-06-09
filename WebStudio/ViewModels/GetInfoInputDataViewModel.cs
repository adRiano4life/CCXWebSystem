﻿using System.Collections.Generic;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class GetInfoInputDataViewModel
    {
        public InputDataUser ListInputData { get; set; }
        public List<Currency> Currencies { get; set; }
        public CardPosition Position { get; set; }
        public Offer Offer { get; set; }

        public string Value { get; set; }

    }
}