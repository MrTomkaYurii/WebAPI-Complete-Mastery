using BogusExample_02.Data.Configurations;
using BogusExample_02.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusExample_02.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<ProductProductCategory> ProductProductCategories => Set<ProductProductCategory>();

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the tables
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductProductCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());

            // Generate seed data with Bogus
            var databaseSeeder = new DatabaseSeeder();

            // Apply the seed data on the tables
            modelBuilder.Entity<Product>().HasData(databaseSeeder.Products);
            modelBuilder.Entity<ProductCategory>().HasData(databaseSeeder.ProductCategories);
            modelBuilder.Entity<ProductProductCategory>().HasData(databaseSeeder.ProductProductCategories);

            base.OnModelCreating(modelBuilder);
        }
    }
}
