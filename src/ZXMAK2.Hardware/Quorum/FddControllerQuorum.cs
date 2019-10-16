using System;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.General;
using ZXMAK2.Hardware.Circuits.Fdd;


namespace ZXMAK2.Hardware.Quorum
{
    public class FddControllerQuorum : FddController
    {
        public class Wd1793QuorumWrapper : Wd1793GenericWrapper
        {
            #region Fields

            private static readonly int[] s_drvDecode = new int[] { 3, 0, 1, 3 };

            #endregion


            public Wd1793QuorumWrapper() : base(4)
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

                        // D0 selects first drive, D1 - second, D2 and D3 are reserved
                        var drv = s_drvDecode[value & 3];
                        drv = (byte)(((value & ~3) ^ 0x10) | drv);

                        Drive = (drv & 3) % FDD.Length;
                        Side = 1 & ~(drv >> 4);

                        Wd.FDD = FDD[Drive];
                        FDD[Drive].HeadSide = Side;
                        FDD[Drive].t = FDD[Drive].CurrentTrack;

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
