using BogusExample_02.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusExample_02.Data.Configurations
{
    internal class ProductProductCategoryConfiguration : IEntityTypeConfiguration<ProductProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductProductCategory> builder)
        {
            builder.ToTable("ProductProductCategory");

            builder.HasKey(x => new { x.ProductId, x.CategoryId });

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductProductCategories)
                .HasForeignKey(x => x.ProductId);

            builder.HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId);
        }
    }
}
