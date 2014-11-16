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
        //public string imagePath { get; set; }
        public ActionMetaData metaData { get; set; }

        public ActionItem(int id)
        {
            this.id = id;
            System.Console.WriteLine("ActionItem was created, id: {0}", id);
        }

    }
}
