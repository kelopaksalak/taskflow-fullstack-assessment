using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var task = modelBuilder.Entity<TaskItem>();

        task.ToTable("Tasks");
        
        task.HasKey(t => t.Id);

        task.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        task.Property(t => t.Description)
            .HasMaxLength(200);
        
        task.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        task.Property(t => t.DueDate)
            .HasColumnType("date");
        
        task.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
        
    }
}