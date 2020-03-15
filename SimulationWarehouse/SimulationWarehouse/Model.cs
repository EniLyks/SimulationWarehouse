using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace SimulationWarehouse
{
    public class Model
    {
        [Key]
        public int IdModel { get; set; }
        public string NameModel { get; set; }
        public int PopulationCity { get; set; }

        private double fEP;
        public double FEP
        {
            get
            {
                return fEP;
            }
            set
            {
                if (value <= 0)
                {
                    fEP = 1;
                }
                else if (value > 0)
                {
                    fEP = value;
                }
            }
        }
        public int IndexSort { get; set; }

        public ICollection<PropertiesProduct> PropertiesProduct { get; set; }
        public Model()
        {
            PropertiesProduct = new List<PropertiesProduct>();
        }
    }
}
