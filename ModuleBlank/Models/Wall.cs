using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterPlugin.Models
{
   internal class Wall
    {
        public int IdWall { get; set; }
        public string NameWall { get; set; }
        public int FK_CurrentLayout { get; set; }
        public int State { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
    }
}
