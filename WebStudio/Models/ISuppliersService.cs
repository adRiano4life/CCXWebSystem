using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebStudio.ViewModels;

namespace WebStudio.Models
{
    public interface ISuppliersService
    {
        IEnumerable<Supplier> GetAll();
        Supplier Get(string id);
        IActionResult AddSupplier(CreateSupplierViewModel model);
    }
}