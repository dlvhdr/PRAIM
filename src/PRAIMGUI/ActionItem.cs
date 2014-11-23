using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM
{
    public class ActionItem
    {
        public int id { get; set; }
        public byte[] snapShot { get; set; }
        public ActionMetaData metaData { get; set; }

        public ActionItem() { }

        public ActionItem(int id)
        {
            this.id = id;
        }

    }
}
