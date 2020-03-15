using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace SimulationWarehouse
{
    class DataModelContext : DbContext
    {
        public DbSet<Model> Models { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PropertiesProduct> PropertiesProducts { get; set; }
    }
}
