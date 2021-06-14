using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using WebStudio.Models;

namespace WebStudio.ViewModels
{
    public class CreateRequestViewModel
    {
        public string Text { get; set; }
        public string TextView { get; set; }
        public DateTime DateOfCreate { get; set; } = DateTime.Now;
        public List<string> FilePath { get; set; }
        public string OverallPath { get; set; }
        public string SupplierSearchInput { get; set; }

        public string ExecutorId { get; set; }
        public virtual User Executor { get; set; }

        public string CardId { get; set; }
        public virtual Card Card { get; set; }

        public virtual List<Supplier> Suppliers { get; set; }

        [NotMapped] 
        public List<IFormFile> Files { get; set; }
        
        
        
        
        
       
    }
}