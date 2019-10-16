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
    public class Wd1793BetadiskWrapper
    {
        #region Constants
        private const int Z80FQ = 3500000; // todo: #define as (conf.frame*conf.intfq)
        private const int FDD_RPS = 5;//15; // rotation speed
        #endregion

        private Wd1793 _wd1793;

        private readonly DiskImage[] fdd;
        private int drive = 0, side = 0;    // update this with changing 'system'
        private byte system;                // beta128 system register

        #region Properties

        public DiskImage[] FDD => fdd;

        public bool LedWr
        {
            get => _wd1793.LedWr;
            set => _wd1793.LedWr = value;
        }

        public bool LedRd
        {
            get => _wd1793.LedRd;
            set => _wd1793.LedRd = value;
        }

        public bool NoDelay
        {
            get => _wd1793.NoDelay;
            set => _wd1793.NoDelay = value;
        }
        #endregion

        #region Public
        public Wd1793BetadiskWrapper(int driveCount = 4)
        {
            if (driveCount < 1 || driveCount > 4)
            {
                throw new ArgumentException("driveCount");
            }
            drive = 0;
            fdd = new DiskImage[driveCount];
            for (int i = 0; i < fdd.Length; i++)
            {
                DiskImage di = new DiskImage();
                di.Init(Z80FQ / FDD_RPS);
                //di.Format();  // take ~1 second (long delay on show options)
                fdd[i] = di;
            }
            fdd[drive].t = fdd[drive].CurrentTrack;

            //fdd[i].set_appendboot(NULL);

            _wd1793 = new Wd1793(Z80FQ, FDD_RPS);
            _wd1793.FDD = fdd[drive];
        }

        public void Write(long tact, WD93REG reg, byte value)
        {
            _wd1793.Process(tact);
            switch (reg)
            {
                case WD93REG.CMD:  // COMMAND/STATUS
                case WD93REG.TRK:
                case WD93REG.SEC:
                case WD93REG.DAT:
                    _wd1793.Write(tact, reg, value);
                    break;

                case WD93REG.SYS:
                    LedRd = true;
                    drive = (value & 3) % fdd.Length;
                    side = 1 & ~(value >> 4);
                    _wd1793.FDD = fdd[drive];
                    FDD[drive].HeadSide = side;
                    FDD[drive].t = FDD[drive].CurrentTrack;
                    if ((value & (int)WD_SYS.SYS_RST) == 0) // reset
                    {
                        _wd1793.Reset();
                    }
                    else if (((system ^ value) & (int)WD_SYS.SYS_HLT) != 0) // hlt 0->1
                    {
                        _wd1793.HLT = true;
                    }
                    system = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("reg");
            }
            _wd1793.Process(tact);
        }

        public byte Read(long tact, WD93REG reg)
        {
            _wd1793.Process(tact);
            byte value = 0xFF;
            switch (reg)
            {
                case WD93REG.CMD: // COMMAND/STATUS #1F
                case WD93REG.TRK:   // #3F
                case WD93REG.SEC:  // #5F
                case WD93REG.DAT:    // #7F
                    value = _wd1793.Read(tact, reg);
                    if (reg == WD93REG.CMD && (system & (int)WD_SYS.SYS_HLT) == 0)
                        value &= (byte)(value & (int)~WD_STATUS.WDS_HEADL);
                    break;

                case WD93REG.SYS: // #FF
                    LedRd = true;
                    value = (byte)((byte)_wd1793.Status | 0x3F);
                    break;

                default:
                    throw new InvalidOperationException();
            }
            return value;
        }

        public string DumpState()
        {
            return string.Join("\n--------------------------\n", new[] { _wd1793.DumpState(), $"drive:  {drive}\nside:   {side}\n" });
        }

        #endregion
    }
}
