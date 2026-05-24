using BogusExample_01.Models;
using Microsoft.EntityFrameworkCore;

namespace BogusExample_01.Database;
public sealed class EmployeeContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=.;Database=BogusExample_01;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True");
        options.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>().HasData(DataGenerator.Employees);
        modelBuilder.Entity<Vehicle>().HasData(DataGenerator.Vehicles);
    }
}
