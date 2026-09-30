using Consumer.Worker.Domain;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Worker.Data;

public class ConsumerDbContext(DbContextOptions<ConsumerDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsumerDbContext).Assembly);
    }
}
