using Microsoft.EntityFrameworkCore;
using Proposal.Form.Models;

namespace Proposal.Form.Database
{
    public class ProposalDbContext : DbContext
    {
        public ProposalDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Proposal.Form.Models.Proposal> Proposals { get; set; }
    }
}
