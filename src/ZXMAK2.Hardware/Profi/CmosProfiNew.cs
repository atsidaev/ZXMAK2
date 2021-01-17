using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.Profi
{
    public class CmosProfiNew : CmosProfi
    {
        protected override  bool IsExtendedMode
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

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            // Karabas Pro handles 0xDF port also for DS clock
            bmgr.Events.SubscribeWrIo(0x00DF, 0x00DF, BusWriteRtc);
            bmgr.Events.SubscribeRdIo(0x00DF, 0x00DF, BusReadRtc);
        }
    }
}
