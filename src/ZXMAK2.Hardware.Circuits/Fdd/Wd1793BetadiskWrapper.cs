using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Model.Disk;

namespace ZXMAK2.Hardware.Circuits.Fdd
{
    /// <summary>
    /// Incapsulates major part of Betadisk logic (system register etc)
    /// </summary>
    public class Wd1793BetadiskWrapper : Wd1793GenericWrapper
    {
        public Wd1793BetadiskWrapper(int driveCount = 4) : base(driveCount)
        {
        }

        protected override void WriteRegister(long tact, WD93REG reg, byte value)
        {
            switch (reg)
            {
                case WD93REG.CMD:  // COMMAND/STATUS
                case WD93REG.TRK:
                case WD93REG.SEC:
                case WD93REG.DAT:
                    Wd.Write(tact, reg, value);
                    break;

                case WD93REG.SYS:
                    LedRd = true;
                    Drive = (value & 3) % FDD.Length;
                    Side = 1 & ~(value >> 4);
                    Wd.FDD = FDD[Drive];
                    FDD[Drive].HeadSide = Side;
                    FDD[Drive].t = FDD[Drive].CurrentTrack;
                    if ((value & (int)WD_SYS.SYS_RST) == 0) // reset
                    {
                        Wd.Reset();
                    }
                    else if (((System ^ value) & (int)WD_SYS.SYS_HLT) != 0) // hlt 0->1
                    {
                        Wd.HLT = true;
                    }
                    System = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("reg");
            }
        }

        protected override byte ReadRegister(long tact, WD93REG reg)
        {
            byte value = 0xFF;
            switch (reg)
            {
                case WD93REG.CMD: // COMMAND/STATUS #1F
                case WD93REG.TRK:   // #3F
                case WD93REG.SEC:  // #5F
                case WD93REG.DAT:    // #7F
                    value = Wd.Read(tact, reg);
                    if (reg == WD93REG.CMD && (System & (int)WD_SYS.SYS_HLT) == 0)
                        value &= (byte)(value & (int)~WD_STATUS.WDS_HEADL);
                    break;

                case WD93REG.SYS: // #FF
                    LedRd = true;
                    value = (byte)((byte)Wd.Status | 0x3F);
                    break;

                default:
                    throw new InvalidOperationException();
            }
            return value;
        }
    }
}
