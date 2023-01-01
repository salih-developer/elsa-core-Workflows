using Elsa.Activities.Signaling.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Open.Linq.AsyncExtensions;
using Proposal.Form.Database;
using Proposal.Form.Models;
using SetWorkFlowEngine.Models;
using System.Diagnostics;

namespace SetWorkFlowEngine.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISignaler _signaler;
        private readonly ProposalDbContext dbContext;
        public HomeController(ILogger<HomeController> logger, ISignaler iSignaler, ProposalDbContext _dbcontext)
        {
            _logger = logger;
            this._signaler = iSignaler;
            this.dbContext = _dbcontext;
        }


        public IActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        //public async Task<IActionResult> Trigger1( CancellationToken cancellationToken = default)
        //{
        //    var result = await _signaler.TriggerSignalAsync("basla", "basla","", "aa4e12ebec714a0586c65a7d8a017df4",  cancellationToken).ToList();
        //    var result1 = await _signaler.TriggerSignalAsync("basla", "basla", "", "", cancellationToken).ToList();
        //    var data = await _signaler.TriggerSignalAsync("basla","basla", workflowInstanceId: "aa4e12ebec714a0586c65a7d8a017df4");

        //    return Ok("Signal triggered :)");
        //}
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public async Task<IActionResult> CustomerApproval()
        {
            List<Customer> data = await dbContext.Customers.Where(x => !x.IsApproval).ToListAsync();
            return View("CustomerApproval", data);
        }
        [HttpGet]
        public async Task<IActionResult> Approval(int id, CancellationToken cancellationToken = default)
        {
            var data = await dbContext.Customers.Where(x => x.Id==id).FirstOrDefaultAsync();
            data.IsApproval = true;
            dbContext.Update(data);
            await dbContext.SaveChangesAsync();

            var result = await _signaler.TriggerSignalAsync("ApprovalCustomer", data, null, null, cancellationToken).ToList();

            return View(data);
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Customer customer, CancellationToken cancellationToken = default)
        {
            dbContext.Add(customer);
            await dbContext.SaveChangesAsync();

            var result = await _signaler.TriggerSignalAsync("TeklifAl", customer, null, null, cancellationToken).ToList();

            customer.WorkflowInstanceId = result.FirstOrDefault().WorkflowInstanceId;

            dbContext.Update(customer);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("CustomerApproval");
        }
    }
}