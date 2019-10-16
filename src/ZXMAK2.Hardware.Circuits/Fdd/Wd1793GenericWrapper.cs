using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Model.Disk;

namespace ZXMAK2.Hardware.Circuits.Fdd
{
    /// <summary>
    /// Abstraction for the class, which should incapsulate major part of Betadisk logic (system register etc)
    /// </summary>
    public abstract class Wd1793GenericWrapper : IWd1793Wrapper
    {
        #region Constants
        private const int Z80FQ = 3500000; // todo: #define as (conf.frame*conf.intfq)
        private const int FDD_RPS = 5;//15; // rotation speed
        #endregion

        protected Wd1793 Wd;

        protected int Drive { get; set; }
        protected int Side { get; set; }      // update this with changing 'system'
        protected byte System;                // beta128 system register

        #region Properties

        public DiskImage[] FDD { get; }

        public bool LedWr
        {
            get { return Wd.LedWr; }
            set { Wd.LedWr = value; }
        }

        public bool LedRd
        {
            get { return Wd.LedRd; }
            set { Wd.LedRd = value; }
        }

        public bool NoDelay
        {
            get { return Wd.NoDelay; }
            set { Wd.NoDelay = value; }
        }

        public Wd1793 Wd1793 
        {
            get { return Wd; }
        }
        #endregion

        #region Abstract
        protected abstract void WriteRegister(long tact, WD93REG reg, byte value);
        protected abstract byte ReadRegister(long tact, WD93REG reg);

        #endregion

        #region Public

        protected Wd1793GenericWrapper(int driveCount)
        {
            if (driveCount < 1 || driveCount > 4)
            {
                throw new ArgumentException("driveCount");
            }
            Drive = 0;
            Side = 0;
            FDD = new DiskImage[driveCount];
            for (int i = 0; i < FDD.Length; i++)
            {
                DiskImage di = new DiskImage();
                di.Init(Z80FQ / FDD_RPS);
                //di.Format();  // take ~1 second (long delay on show options)
                FDD[i] = di;
            }
            FDD[Drive].t = FDD[Drive].CurrentTrack;

            //fdd[i].set_appendboot(NULL);

            Wd = new Wd1793(Z80FQ, FDD_RPS);
            Wd.FDD = FDD[Drive];
        }

        public void Write(long tact, WD93REG reg, byte value)
        {
            Wd.Process(tact);
            WriteRegister(tact, reg, value);
            Wd.Process(tact);
        }

        public byte Read(long tact, WD93REG reg)
        {
            Wd.Process(tact);
            return ReadRegister(tact, reg);
        }

        public virtual string DumpState()
        {
            return string.Join("\n--------------------------\n", new[] { Wd.DumpState(), $"drive:  {Drive}\nside:   {Side}\n" });
        }

        #endregion
    }
}
