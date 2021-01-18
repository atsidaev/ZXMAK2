using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Engine.Entities;

namespace ZXMAK2.Hardware.Profi
{
    public class MemoryProfi1024New : MemoryProfi1024
    {
        protected override void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if ((CMR0 & 0x10) != 0)
            {
                if (!DOSEN)
                {
                    DOSEN = true;
                }
            }
        }

        protected override void BusReadMemRamM1(ushort addr, ref byte value)
        {
            if ((CMR0 & 0x10) != 0)
            {
                if (SYSEN)
                {
                    SYSEN = false;
                }
                if (DOSEN)
                {
                    DOSEN = false;
                }
            }
        }

        protected override void UpdateMapping()
        {
            m_lock = ((CMR0 & 0x20) != 0);
            int ramPage = CMR0 & 7;
            var rom14 = (CMR0 & 0x10) != 0;
            int romPage = rom14 ?
                GetRomIndex(RomId.ROM_SOS) :
                GetRomIndex(RomId.ROM_128);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN && rom14)      // trdos or 48/128
                romPage = GetRomIndex(RomId.ROM_DOS);// 2;
            if (SYSEN || (DOSEN && !rom14))
                romPage = GetRomIndex(RomId.ROM_SYS);// 3;

            int sega = CMR1 & m_cmr1mask;
            bool norom = NOROM;
            bool sco = SCO;   // selectors RAM gates
            bool scr = SCR;   // !??CMR0.D3=1??!
            //bool cpm = CPM;

            if (norom)
                m_lock = false;

            ramPage |= sega << 3;

            if (m_ulaProfi != null)
            {
                m_ulaProfi.SetPageMappingProfi(
                    DS80,
                    videoPage,
                    norom ? 0 : -1,
                    sco ? ramPage : 5,
                    scr ? 6 : 2,
                    sco ? 7 : ramPage);
            }
            else
            {
                m_ula.SetPageMapping(
                    videoPage,
                    norom ? 0 : -1,
                    sco ? ramPage : 5,
                    scr ? 6 : 2,
                    sco ? 7 : ramPage);
            }
            MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = sco ? RamPages[ramPage] : RamPages[5];
            MapRead8000 = scr ? RamPages[6] : RamPages[2];
            MapReadC000 = sco ? RamPages[7] : RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }
    }
}
