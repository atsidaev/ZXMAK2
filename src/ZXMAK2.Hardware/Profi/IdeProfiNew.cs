using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Hardware.Profi
{
    public class IdeProfiNew : IdeProfi
    {
        protected override bool IsExtendedMode
        {
            get
            {
                var cpm = (m_memory.CMR1 & 0x20) != 0;
                var rom48 = (m_memory.CMR0 & 0x10) != 0;
                var csExtended = cpm && rom48;

                // For new port decoding scheme
                var fromSysOrDos = (m_memory.SYSEN || m_memory.DOSEN);

                return csExtended || (!cpm && fromSysOrDos);
            }
        }
    }
}
