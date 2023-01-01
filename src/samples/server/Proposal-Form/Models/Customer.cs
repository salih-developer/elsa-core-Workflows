namespace Proposal.Form.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string WorkflowInstanceId { get; set; }
        public bool IsApproval { get; set; } = false;
    }
}
