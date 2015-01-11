using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Action Item Model
    /// </summary>
    public class ActionItem
    {
        public int id { get; set; }
        public byte[] snapShot { get; set; }
        public ActionMetaData metaData { get; set; }

        /// <summary>
        /// Parameterless consctructor
        /// </summary>
        public ActionItem() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public ActionItem(int id)
        {
            this.id = id;
        }

    }
}
