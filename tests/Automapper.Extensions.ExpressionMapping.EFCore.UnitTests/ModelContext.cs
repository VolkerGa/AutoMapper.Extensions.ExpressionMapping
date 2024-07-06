using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Automapper.Extensions.ExpressionMapping.EFCore.UnitTests
{
    public class ModelContext : DbContext
    {
        public ModelContext() { }
        public ModelContext(DbContextOptions<ModelContext> options) : base(options) { }
        public DbSet<Model> DbModel => Set<Model>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model>()
                .HasKey(m => m.Id);
        }
    }
}
