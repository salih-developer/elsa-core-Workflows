using Elsa.Activities.Signaling.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Open.Linq.AsyncExtensions;
using Proposal.Form.Database;
using Proposal.Form.Models;
namespace Proposal.Form.Controllers
{
    public class ProposalController : Controller
    {
        private readonly ISignaler _signaler;
        private readonly ProposalDbContext dbContext;

        public ProposalController(ISignaler signaler, ProposalDbContext dbContext)
        {
            _signaler = signaler;
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Proposal.Form.Models.Proposal proposalViewModel, CancellationToken cancellationToken = default)
        {
            proposalViewModel.WorkflowInstanceId = "";
            dbContext.Add(proposalViewModel);
            await dbContext.SaveChangesAsync();

            var result = await _signaler.TriggerSignalAsync("take_proposal", proposalViewModel, null, null, cancellationToken).ToList();

            proposalViewModel.WorkflowInstanceId = result.FirstOrDefault().WorkflowInstanceId;

            dbContext.Update(proposalViewModel);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("ProposalApprovalListForm");
        }

        [HttpGet]
        public async Task<IActionResult> ProposalApprovalListForm( CancellationToken cancellationToken = default)
        {
            List<Proposal.Form.Models.Proposal> data = await dbContext.Proposals.Where(x => !x.IsApproval).ToListAsync();
            return View("ProposalApprovalListForm", data);
        }

        [HttpGet]
        public async Task<IActionResult> ProposalApprovalResult(int id, CancellationToken cancellationToken = default)
        {
            var data = await dbContext.Proposals.Where(x => x.Id == id).FirstOrDefaultAsync();
            data.IsApproval = true;
            dbContext.Update(data);
            await dbContext.SaveChangesAsync();

            var result = await _signaler.TriggerSignalAsync("Approval_Proposal", data, data.WorkflowInstanceId, null, cancellationToken).ToList();

            return View(data);
        }
    }
}
