using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRAIM
{
    /// <summary>
    /// This class defines info for booting PRAIM
    /// </summary>
    [Serializable]
    public class BootConfig
    {
        private static readonly uint _StartingActionItemID = 1;

        public string LastProject { get; set; }
        public string LastVersion { get; set; }
        public uint CurrentActionItemID { get; set; }

        public BootConfig()
        {
            CurrentActionItemID = _StartingActionItemID;
        }
    }
}
