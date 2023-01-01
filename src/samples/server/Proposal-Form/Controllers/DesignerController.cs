using Microsoft.AspNetCore.Mvc;

namespace Proposal.Form.Controllers
{
    public class DesignerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
