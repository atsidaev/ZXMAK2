using System;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.Circuits.Fdd;
using ZXMAK2.Model.Disk;


namespace ZXMAK2.Hardware.Quorum
{
    public class FddControllerQuorum : FddController
    {
        #region Wd1793QuorumWrapper
        public class Wd1793QuorumWrapper : Wd1793GenericWrapper
        {
            private DiskImage _noDisk = new DiskImage();

            public Wd1793QuorumWrapper() : base(2)
            {
            }

            protected override void WriteRegister(long tact, WD93REG reg, byte value)
            {
                switch (reg)
                {
                    case WD93REG.CMD:
                    case WD93REG.TRK:
                    case WD93REG.SEC:
                    case WD93REG.DAT:
                        Wd.Write(tact, reg, value);
                        break;

                    case WD93REG.SYS:
                        LedRd = true;

                        // D0 selects first drive, D1 - second, D2 and D3 are reserved for third and fourth.
                        // This means that we should check if any drive is selected at all, and also remap this bit set to a Drive index.
                        var selectedDrive = value & 0x7;
                        if (selectedDrive == 0x01)
                            Drive = 0;
                        else if (selectedDrive == 0x02)
                            Drive = 1;
                        else
                            Drive = -1; // If no drive is selected, or the reserved D2 and D3 bits are used, then do not connect any floppy disk to WD1793

                        Side = (value >> 4) & 0x01;

                        if (Drive == -1)
                        {
                            // Connect dummy floppy drive without a disk
                            Wd.FDD = _noDisk;
                        }
                        else
                        {
                            Wd.FDD = FDD[Drive];
                            Wd.FDD.HeadSide = Side;
                            Wd.FDD.t = Wd.FDD.CurrentTrack;
                        }

                        System = value;
                        break;
                }
            }

            protected override byte ReadRegister(long tact, WD93REG reg)
            {
                byte value = 0xFF;
                switch (reg)
                {
                    case WD93REG.CMD:
                    case WD93REG.TRK:
                    case WD93REG.SEC:
                    case WD93REG.DAT:
                        LedRd = true;
                        value = Wd.Read(tact, reg);
                        break;
                }
                
                return value;
            }
        }
        #endregion

        public FddControllerQuorum() : base(new Wd1793QuorumWrapper())
        {
            Name = "FDD QUORUM";
            Description = "FDD controller WD1793 with QUORUM port activation";
        }
        
        #region BetaDiskInterface

        protected override void OnSubscribeIo(IBusManager bmgr)
        {
            // mask - #9C
            // #80 - CMD
            // #81 - TRK
            // #82 - SEC
            // #83 - DAT
            // #84, #85 - SYS (#85 in documentation, but some system programs use #84)
            bmgr.Events.SubscribeWrIo(0x9C, 0x80 & 0x9C, BusWriteFdc);
            bmgr.Events.SubscribeRdIo(0x9C, 0x80 & 0x9C, BusReadFdc);
            bmgr.Events.SubscribeWrIo(0x9C, 0x84 & 0x9E, BusWriteSys);
            bmgr.Events.SubscribeRdIo(0x9C, 0x84 & 0x9E, BusReadSys);
            bmgr.Events.SubscribeReset(BusReset);
        }

        private void BusReset()
        {
            m_wd.Wd1793.Reset();
        }

        public override bool IsActive => true;
        
        protected override void BusWriteFdc(ushort addr, byte value, ref bool handled)
        {
            if (handled || !IsActive)
                return;
            handled = true;

            var fdcReg = addr & 0x03;
            if (LogIo)
            {
                LogIoWrite(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
            m_wd.Write(m_cpu.Tact, (WD93REG)fdcReg, value);
        }

        protected override void BusReadFdc(ushort addr, ref byte value, ref bool handled)
        {
            if (handled || !IsActive)
                return;
            handled = true;

            var fdcReg = addr & 0x03;
            value = m_wd.Read(m_cpu.Tact, (WD93REG)fdcReg);
            if (LogIo)
            {
                LogIoRead(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }

        protected override void BusWriteSys(ushort addr, byte value, ref bool handled)
        {
            if (handled || !IsActive)
                return;
            handled = true;

            if (LogIo)
            {
                LogIoWrite(m_cpu.Tact, WD93REG.SYS, value);
            }
            m_wd.Write(m_cpu.Tact, WD93REG.SYS, value);
        }

        protected override void BusReadSys(ushort addr, ref byte value, ref bool handled)
        {
            if (handled || !IsActive)
                return;
            handled = true;

            value = m_wd.Read(m_cpu.Tact, WD93REG.SYS);
            if (LogIo)
            {
                LogIoRead(m_cpu.Tact, WD93REG.SYS, value);
            }
        }

        #endregion
    }
}
