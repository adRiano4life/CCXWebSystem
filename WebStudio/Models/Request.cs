using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebStudio.Models
{
    public class Request
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; }
        public DateTime DateOfCreate { get; set; }
        public string FilePath { get; set; }

        public string ExecutorId { get; set; }
        public virtual User Executor { get; set; }

        public string CardId { get; set; }
        public virtual Card Card { get; set; }

        public virtual List<Supplier> Suppliers { get; set; }
    }
}