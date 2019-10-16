using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Model.Disk;

namespace ZXMAK2.Hardware.Circuits.Fdd
{
    public interface IWd1793Wrapper
    {
        DiskImage[] FDD { get; }
        bool LedWr {get;set;}
        bool LedRd {get;set;}
        bool NoDelay {get;set;}
        byte Read(long tact, WD93REG reg);
        void Write(long tact, WD93REG reg, byte value);
        Wd1793 Wd1793 { get; }
        string DumpState();
    }
}
