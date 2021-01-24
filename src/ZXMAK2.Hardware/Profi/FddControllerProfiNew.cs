using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Hardware.Profi
{
    public class FddControllerProfiNew : FddControllerProfi
    {
        public FddControllerProfiNew() : base()
        {
            Name = "FDD PROFI (NEW)";
            Description = "Profi FDD with new port addresses decoding";
            NoDelay = true;
        }

        protected override bool IsExtendedMode
        {
            get
            {
                var cpm = (m_memory.CMR1 & 0x20) != 0;
                var rom48 = (m_memory.CMR0 & 0x10) != 0;
                
                // For new port decoding scheme
                var fromSysOrDos = (m_memory.SYSEN || m_memory.DOSEN);

                return (cpm && rom48) || (!cpm && fromSysOrDos);
            }
        }

        private bool AllowOldTrDosPorts
        {
            get 
            {
                var rom14 = (m_memory.CMR0 & 0x10) != 0;
                var cpm = (m_memory.CMR1 & 0x20) != 0;

                return rom14 == false && cpm == false;
            }
        }

        #region IBusHandler


        protected override void BusWriteFdc(ushort addr, byte value, ref bool handled)
        {
            if (DOSEN && AllowOldTrDosPorts)
                return;

            base.BusWriteFdc(addr, value, ref handled);
        }

        // Use old WD93 ports from TR-DOS only

        protected override void BusReadFdc(ushort addr, ref byte value, ref bool handled)
        {
            if (DOSEN && AllowOldTrDosPorts)
                return;

            base.BusReadFdc(addr, ref value, ref handled);
        }

        protected override void BusWriteSys(ushort addr, byte value, ref bool handled)
        {
            if (DOSEN && AllowOldTrDosPorts)
                return;

            base.BusWriteSys(addr, value, ref handled);
        }

        protected override void BusReadSys(ushort addr, ref byte value, ref bool handled)
        {
            if (DOSEN && AllowOldTrDosPorts)
                return;

            base.BusReadSys(addr, ref value, ref handled);
        }


        #endregion

    }
}
