using Microsoft.EntityFrameworkCore;
using Topo.Data.Models;

namespace Topo.Data
{
    public class TopoDBContext : DbContext
    {
        private static bool _created = false;
        public TopoDBContext(DbContextOptions<TopoDBContext> options) : base(options)
        {
            if (!_created)
            {
                _created = true;
                Database.Migrate();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            
        }

        public DbSet<OASTemplate> OASTemplates { get; set; }
        public DbSet<OASWorksheetAnswers> OASWorksheetAnswers { get; set; }
    }
}
