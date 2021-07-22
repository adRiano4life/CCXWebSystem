using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStudio.Models
{
    public class InputDataUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public virtual List<string> Meas { get; set; } = new List<string>(){"шт.", "тонны", "м3", "м2", "кг.", "литр", "комп.", "п.м"};
        public virtual List<string> PayTerm { get; set; } = new List<string>(){"нал", "безнал"};
        public virtual List<string> NDS { get; set; } = new List<string>(){"да", "нет"};
        public virtual List<string> DelivTerm{ get; set; } = new List<string>(){"EXW", "DDP", "DAP", "FCA", "CPT", "CIP", "DAT", "FAS", "FOB","CFR", "DDU", "CIF"};
       
        
        
    }
}