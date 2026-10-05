using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;

namespace WebApplication5.Controllers
{
    public class DiscountController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new DiscountViewModel());
        }

        [HttpPost]
        public IActionResult Index(DiscountViewModel model)
        {
            model.FinalPrice = model.Price - (model.Price * model.DiscountPercent / 100);
            decimal saving = model.Price - model.FinalPrice;
            model.Message = $"Ваша економія склала {saving} грн";

            return View(model);
        }
    }
}