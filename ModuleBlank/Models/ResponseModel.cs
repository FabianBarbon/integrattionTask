using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterPlugin.Models
{
    public class ResponseModel
    {
        public MessageModel message { get; set; }
        public bool status { get; set; }
    }

    public class MessageModel
    {
        public List<WindowItem> success { get; set; }
        public List<WindowItem> failure { get; set; }
        public List<LayoutSaveItem> layoutSave { get; set; }
    }

    public class WindowItem
    {
        public string idWindow { get; set; }
        public string response { get; set; }
    }

    public class LayoutSaveItem
    {
        public bool savedStatus { get; set; }
        public string response { get; set; }
        public string layoutName { get; set; }
        public string wall { get; set; }
    }
}
