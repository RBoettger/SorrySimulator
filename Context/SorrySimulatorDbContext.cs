using Microsoft.EntityFrameworkCore;
using SorrySimulator.Entities;

namespace SorrySimulator.Context
{
    public class SorrySimulatorDbContext : DbContext
    {
        public SorrySimulatorDbContext(DbContextOptions<SorrySimulatorDbContext> options) : base(options)
        {
        }
        public DbSet<Sender> Senders { get; set; }
        public DbSet<Receiver> Receivers { get; set; }
    }
}
