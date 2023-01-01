namespace Proposal.Form.Models
{
    public class Proposal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public string WorkflowInstanceId { get; set; }
        public bool IsApproval { get; set; } = false;
    }
}
