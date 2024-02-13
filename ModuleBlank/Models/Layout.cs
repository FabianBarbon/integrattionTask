using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterPlugin.Models
{
    public class Layout
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Row { get; set; }
        public int Columna { get; set; }
        public bool ObjectIsDefault { get; set; }
        public string ObjectType { get; set; }
        public string AspectTypes { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string ModifiedByUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public bool estado { get; set; }
    }
}
