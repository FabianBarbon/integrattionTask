using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterPlugin.Models
{
    public class MatrizModel
    {
        public int ID { get; set; }
        public int IdLayout { get; set; }
        public int Row { get; set; }
        public int Columna { get; set; }
        public int Camera { get; set; }
        public string Url { get; set; }
        public string Guid_Camera { get; set; }
    }
}
