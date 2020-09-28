using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<NFCRecord> NFCRecords { get; set; }
    public DbSet<Record> Records { get; set; }
    public DbSet<Branch> Branches { get; set; }

}