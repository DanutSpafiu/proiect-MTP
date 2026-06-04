using Microsoft.EntityFrameworkCore;
using proiectMTP.Models;

namespace proiectMTP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Profesor> Professors => Set<Profesor>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.HasIndex(p => p.Email).IsUnique();

            entity.HasMany(p => p.Students)
                .WithOne(s => s.Professor)
                .HasForeignKey(s => s.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Sessions)
                .WithOne(s => s.Professor)
                .HasForeignKey(s => s.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasMany(s => s.Sessions)
                .WithOne(se => se.Student)
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.Property(s => s.Price).HasColumnType("decimal(10,2)");
        });
    }
}
