using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterPlugin.Models
{
    internal class Response
    {
        // Clase para modelar la estructura de la respuesta
        public class ResponseModel
        {
            public MessageModel message { get; set; }
            public bool status { get; set; }
        }

        // Clase para modelar la estructura del mensaje dentro de la respuesta
        public class MessageModel
        {
            public List<WindowItem> success { get; set; }
            public List<WindowItem> failure { get; set; }
            public List<LayoutSaveItem> layoutSave { get; set; }
        }
    }
}
