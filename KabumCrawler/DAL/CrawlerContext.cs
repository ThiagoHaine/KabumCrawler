using KabumCrawler.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace KabumCrawler.DAL
{
    public class CrawlerContext : DbContext
    {
        public CrawlerContext() : base("CrawlerContext")
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<Product>().HasIndex(p => new { p.KabumID }).IsUnique();
        }
    }
}
