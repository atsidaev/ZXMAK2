using System;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Profi
{
    public class UlaProfi5XX : UlaProfi3XX
    {
        public UlaProfi5XX()
        {
            Name = "PROFI 5.xx";
            InitStaticTables();
        }


        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.Events.SubscribeReset(busReset);
        }

        #endregion

        #region UlaDeviceBase

        protected override void WritePortFE(ushort addr, byte value, ref bool handled)
        {
            if ((addr & 0x0081) == 0 && (Memory.CMR1 & 0x80) != 0)
                SetPalette((PortFE ^ 0x0F) & 0x0F, (byte)~(addr >> 8));
            base.WritePortFE(addr, value, ref handled);
        }

        protected override void ReadPortAll(ushort addr, ref byte value, ref bool handled)
        {
            base.ReadPortAll(addr, ref value, ref handled);
            if ((addr & 1) == 0)
            {
                //LogAgent.Info("RD #FE @ PC=#{0:X4}", CPU.regs.PC);
                value &= 0x7F; // UniCopy by Michael Markowsky

                //value |= GX0
                if ((Memory.CMR1 & 0x80) != 0)
                {
                    if ((m_pal[(~PortFE) & 0x07] & 0x40) != 0)
                        value |= 0x80;
                }
                else
                {
                    if ((m_pal[PortFE & 0x07] & 0x40) != 0)
                        value |= 0x80;
                }
            }
        }

        private void busReset()
        {
            var newPalette = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                m_pal[i] = m_pal_startup[i];
                newPalette[i] = m_pal_map[m_pal[i]];
            }
            //TODO: remove palette substitution and replace with UpdatePalette
            SpectrumRenderer.Palette = newPalette;
            ProfiRenderer.Palette = newPalette;
        }

        #endregion

        private readonly byte[] m_pal = new byte[16];
        private readonly uint[] m_pal_map = new uint[0x100];
        private readonly byte[] m_pal_startup = new byte[]
        {
            0x00, 0x02, 0x10, 0x12, 0x80, 0x82, 0x90, 0x92,
            0x00, 0x03, 0x18, 0x1B, 0xC0, 0xC3, 0xD8, 0xDB,
        };

        // Voltage values from read video DAC collected by solegstar (https://zx-pk.ru/threads/16830-zxmak2-virtualnaya-mashina-zx-spectrum/page189.html)
        private new Tuple<double, double, double>[] _palletteVoltages = {
            new Tuple<double,double,double>(0, 0, 0),
            new Tuple<double,double,double>(0, 0, 0.28),
            new Tuple<double,double,double>(0, 0, 0.84),
            new Tuple<double,double,double>(0, 0, 1.6),
            new Tuple<double,double,double>(0, 0, 0),
            new Tuple<double,double,double>(0, 0, 0.28),
            new Tuple<double,double,double>(0, 0, 0.84),
            new Tuple<double,double,double>(0, 0, 1.6),
            new Tuple<double,double,double>(0.2, 0, 0),
            new Tuple<double,double,double>(0.28, 0, 0.32),
            new Tuple<double,double,double>(0.28, 0, 0.96),
            new Tuple<double,double,double>(0.36, 0, 1.72),
            new Tuple<double,double,double>(0.52, 0, 0),
            new Tuple<double,double,double>(0.56, 0, 0.36),
            new Tuple<double,double,double>(0.6, 0, 0.96),
            new Tuple<double,double,double>(0.68, 0, 1.72),
            new Tuple<double,double,double>(0.92, 0, 0),
            new Tuple<double,double,double>(0.96, 0, 0.36),
            new Tuple<double,double,double>(1.04, 0, 1),
            new Tuple<double,double,double>(1.08, 0, 1.76),
            new Tuple<double,double,double>(1.24, 0, 0),
            new Tuple<double,double,double>(1.28, 0, 0.36),
            new Tuple<double,double,double>(1.32, 0, 1),
            new Tuple<double,double,double>(1.4, 0, 1.76),
            new Tuple<double,double,double>(1.64, 0, 0),
            new Tuple<double,double,double>(1.68, 0, 0.4),
            new Tuple<double,double,double>(1.72, 0, 1.04),
            new Tuple<double,double,double>(1.8, 0, 1.76),
            new Tuple<double,double,double>(2, 0, 0),
            new Tuple<double,double,double>(2, 0, 0.44),
            new Tuple<double,double,double>(2.08, 0, 1.04),
            new Tuple<double,double,double>(2.16, 0, 1.8),
            new Tuple<double,double,double>(0, 0, 0),
            new Tuple<double,double,double>(0, 0, 0.32),
            new Tuple<double,double,double>(0, 0, 0.96),
            new Tuple<double,double,double>(0, 0, 1.68),
            new Tuple<double,double,double>(0, 0, 0),
            new Tuple<double,double,double>(0, 0, 0.32),
            new Tuple<double,double,double>(0, 0, 0.96),
            new Tuple<double,double,double>(0, 0, 1.68),
            new Tuple<double,double,double>(0.28, 0, 0),
            new Tuple<double,double,double>(0.28, 0, 0.36),
            new Tuple<double,double,double>(0.36, 0, 1),
            new Tuple<double,double,double>(0.4, 0, 1.72),
            new Tuple<double,double,double>(0.56, 0, 0),
            new Tuple<double,double,double>(0.64, 0, 0.36),
            new Tuple<double,double,double>(0.68, 0, 1),
            new Tuple<double,double,double>(0.72, 0, 1.76),
            new Tuple<double,double,double>(0.96, 0, 0),
            new Tuple<double,double,double>(1, 0, 0.4),
            new Tuple<double,double,double>(1.04, 0, 1),
            new Tuple<double,double,double>(1.08, 0, 1.76),
            new Tuple<double,double,double>(1.28, 0, 0),
            new Tuple<double,double,double>(1.32, 0, 0.4),
            new Tuple<double,double,double>(1.36, 0, 1.04),
            new Tuple<double,double,double>(1.44, 0, 1.76),
            new Tuple<double,double,double>(1.68, 0, 0),
            new Tuple<double,double,double>(1.72, 0, 0.44),
            new Tuple<double,double,double>(1.76, 0, 1.04),
            new Tuple<double,double,double>(1.84, 0, 1.8),
            new Tuple<double,double,double>(2, 0, 0),
            new Tuple<double,double,double>(2.08, 0, 0.44),
            new Tuple<double,double,double>(2.08, 0.12, 1.08),
            new Tuple<double,double,double>(2.16, 0.16, 1.84),
            new Tuple<double,double,double>(0, 0.32, 0),
            new Tuple<double,double,double>(0, 0.34, 0.32),
            new Tuple<double,double,double>(0, 0.36, 0.96),
            new Tuple<double,double,double>(0, 0.4, 1.76),
            new Tuple<double,double,double>(0, 0.32, 0),
            new Tuple<double,double,double>(0, 0.34, 0.36),
            new Tuple<double,double,double>(0, 0.4, 0.96),
            new Tuple<double,double,double>(0, 0.4, 1.72),
            new Tuple<double,double,double>(0.28, 0.32, 0),
            new Tuple<double,double,double>(0.32, 0.4, 0.4),
            new Tuple<double,double,double>(0.36, 0.4, 1.04),
            new Tuple<double,double,double>(0.4, 0.44, 1.76),
            new Tuple<double,double,double>(0.6, 0.36, 0),
            new Tuple<double,double,double>(0.64, 0.4, 0.4),
            new Tuple<double,double,double>(0.68, 0.4, 1),
            new Tuple<double,double,double>(0.72, 0.48, 1.76),
            new Tuple<double,double,double>(1, 0.4, 0),
            new Tuple<double,double,double>(1.04, 0.4, 0.4),
            new Tuple<double,double,double>(1.08, 0.48, 1.04),
            new Tuple<double,double,double>(1.08, 0.48, 1.8),
            new Tuple<double,double,double>(1.32, 0.4, 0),
            new Tuple<double,double,double>(1.36, 0.44, 0.44),
            new Tuple<double,double,double>(1.4, 0.48, 1.08),
            new Tuple<double,double,double>(1.44, 0.52, 1.84),
            new Tuple<double,double,double>(1.68, 0.44, 0),
            new Tuple<double,double,double>(1.72, 0.48, 0.48),
            new Tuple<double,double,double>(1.76, 0.48, 1.08),
            new Tuple<double,double,double>(1.84, 0.52, 1.84),
            new Tuple<double,double,double>(2.04, 0.44, 0),
            new Tuple<double,double,double>(2.04, 0.48, 0.48),
            new Tuple<double,double,double>(2.12, 0.52, 1.12),
            new Tuple<double,double,double>(2.16, 0.56, 1.84),
            new Tuple<double,double,double>(0, 0.64, 0),
            new Tuple<double,double,double>(0, 0.64, 0.32),
            new Tuple<double,double,double>(0, 0.72, 0.96),
            new Tuple<double,double,double>(0, 0.72, 1.68),
            new Tuple<double,double,double>(0, 0.64, 0),
            new Tuple<double,double,double>(0, 0.72, 0.36),
            new Tuple<double,double,double>(0, 0.72, 1),
            new Tuple<double,double,double>(0, 0.76, 1.76),
            new Tuple<double,double,double>(0.28, 0.68, 0),
            new Tuple<double,double,double>(0.36, 0.72, 0.4),
            new Tuple<double,double,double>(0.4, 0.72, 1.04),
            new Tuple<double,double,double>(0.4, 0.8, 1.76),
            new Tuple<double,double,double>(0.64, 0.72, 0),
            new Tuple<double,double,double>(0.68, 0.72, 0.4),
            new Tuple<double,double,double>(0.72, 0.76, 1.04),
            new Tuple<double,double,double>(0.76, 0.8, 1.76),
            new Tuple<double,double,double>(1.04, 0.72, 0),
            new Tuple<double,double,double>(1.08, 0.76, 0.44),
            new Tuple<double,double,double>(1.08, 0.8, 1.08),
            new Tuple<double,double,double>(1.12, 0.84, 1.84),
            new Tuple<double,double,double>(1.36, 0.72, 0),
            new Tuple<double,double,double>(1.36, 0.8, 0.48),
            new Tuple<double,double,double>(1.4, 0.8, 1.08),
            new Tuple<double,double,double>(1.44, 0.88, 1.84),
            new Tuple<double,double,double>(1.72, 0.72, 0),
            new Tuple<double,double,double>(1.76, 0.8, 0.48),
            new Tuple<double,double,double>(1.8, 0.84, 1.12),
            new Tuple<double,double,double>(1.84, 0.88, 1.84),
            new Tuple<double,double,double>(2.04, 0.76, 0),
            new Tuple<double,double,double>(2.12, 0.8, 0.48),
            new Tuple<double,double,double>(2.16, 0.88, 1.12),
            new Tuple<double,double,double>(2.2, 0.88, 1.84),
            new Tuple<double,double,double>(0, 0.96, 0),
            new Tuple<double,double,double>(0, 0.96, 0.4),
            new Tuple<double,double,double>(0, 1.04, 1),
            new Tuple<double,double,double>(0, 1.08, 1.76),
            new Tuple<double,double,double>(0, 0.96, 0),
            new Tuple<double,double,double>(0, 1, 0.4),
            new Tuple<double,double,double>(0, 1.08, 1.04),
            new Tuple<double,double,double>(0, 1.08, 1.76),
            new Tuple<double,double,double>(0.32, 1, 0),
            new Tuple<double,double,double>(0.36, 1.04, 0.4),
            new Tuple<double,double,double>(0.4, 1.08, 1.04),
            new Tuple<double,double,double>(0.44, 1.12, 1.8),
            new Tuple<double,double,double>(0.64, 1.04, 0),
            new Tuple<double,double,double>(0.68, 1.04, 0.4),
            new Tuple<double,double,double>(0.72, 1.12, 1.08),
            new Tuple<double,double,double>(0.76, 1.12, 1.8),
            new Tuple<double,double,double>(1.04, 1.04, 0),
            new Tuple<double,double,double>(1.08, 1.08, 0.44),
            new Tuple<double,double,double>(1.12, 1.12, 1.12),
            new Tuple<double,double,double>(1.16, 1.12, 1.84),
            new Tuple<double,double,double>(1.36, 1.04, 0),
            new Tuple<double,double,double>(1.4, 1.08, 0.48),
            new Tuple<double,double,double>(1.44, 1.12, 1.12),
            new Tuple<double,double,double>(1.48, 1.16, 1.88),
            new Tuple<double,double,double>(1.76, 1.08, 0),
            new Tuple<double,double,double>(1.8, 1.12, 0.48),
            new Tuple<double,double,double>(1.84, 1.12, 1.12),
            new Tuple<double,double,double>(1.84, 1.2, 1.88),
            new Tuple<double,double,double>(2.08, 1.08, 0),
            new Tuple<double,double,double>(2.12, 1.12, 0.48),
            new Tuple<double,double,double>(2.16, 1.16, 1.12),
            new Tuple<double,double,double>(2.2, 1.2, 1.88),
            new Tuple<double,double,double>(0, 1.2, 0),
            new Tuple<double,double,double>(0, 1.28, 0.4),
            new Tuple<double,double,double>(0, 1.28, 1.04),
            new Tuple<double,double,double>(0, 1.36, 1.76),
            new Tuple<double,double,double>(0, 1.24, 0),
            new Tuple<double,double,double>(0, 1.24, 0.4),
            new Tuple<double,double,double>(0, 1.32, 1.04),
            new Tuple<double,double,double>(0, 1.36, 1.8),
            new Tuple<double,double,double>(0.36, 1.28, 0),
            new Tuple<double,double,double>(0.4, 1.28, 0.44),
            new Tuple<double,double,double>(0.4, 1.32, 1.08),
            new Tuple<double,double,double>(0.44, 1.4, 1.8),
            new Tuple<double,double,double>(0.64, 1.28, 0),
            new Tuple<double,double,double>(0.72, 1.28, 0.48),
            new Tuple<double,double,double>(0.72, 1.36, 1.08),
            new Tuple<double,double,double>(0.8, 1.4, 1.84),
            new Tuple<double,double,double>(1.04, 1.32, 0),
            new Tuple<double,double,double>(1.08, 1.36, 0.48),
            new Tuple<double,double,double>(1.12, 1.44, 1.12),
            new Tuple<double,double,double>(1.16, 1.44, 1.84),
            new Tuple<double,double,double>(1.36, 1.32, 0),
            new Tuple<double,double,double>(1.4, 1.4, 0.48),
            new Tuple<double,double,double>(1.4, 1.44, 1.12),
            new Tuple<double,double,double>(1.48, 1.48, 1.88),
            new Tuple<double,double,double>(1.76, 1.4, 0),
            new Tuple<double,double,double>(1.8, 1.44, 0.48),
            new Tuple<double,double,double>(1.84, 1.44, 1.12),
            new Tuple<double,double,double>(1.88, 1.48, 1.92),
            new Tuple<double,double,double>(2.12, 1.4, 0),
            new Tuple<double,double,double>(2.12, 1.44, 0.52),
            new Tuple<double,double,double>(2.16, 1.48, 1.16),
            new Tuple<double,double,double>(2.2, 1.48, 1.92),
            new Tuple<double,double,double>(0, 1.64, 0),
            new Tuple<double,double,double>(0, 1.68, 0.4),
            new Tuple<double,double,double>(0, 1.76, 1.04),
            new Tuple<double,double,double>(0, 1.8, 1.8),
            new Tuple<double,double,double>(0, 1.68, 0),
            new Tuple<double,double,double>(0, 1.72, 0.44),
            new Tuple<double,double,double>(0, 1.76, 1.04),
            new Tuple<double,double,double>(0, 1.8, 1.8),
            new Tuple<double,double,double>(0.36, 1.72, 0),
            new Tuple<double,double,double>(0.36, 1.76, 0.48),
            new Tuple<double,double,double>(0.44, 1.8, 1.08),
            new Tuple<double,double,double>(0.48, 1.8, 1.84),
            new Tuple<double,double,double>(0.68, 1.72, 0),
            new Tuple<double,double,double>(0.72, 1.76, 0.48),
            new Tuple<double,double,double>(0.76, 1.8, 1.12),
            new Tuple<double,double,double>(0.8, 1.84, 1.88),
            new Tuple<double,double,double>(1.04, 1.76, 0),
            new Tuple<double,double,double>(1.08, 1.76, 0.48),
            new Tuple<double,double,double>(1.12, 1.84, 1.12),
            new Tuple<double,double,double>(1.2, 1.88, 1.88),
            new Tuple<double,double,double>(1.4, 1.76, 0),
            new Tuple<double,double,double>(1.44, 1.8, 0.48),
            new Tuple<double,double,double>(1.48, 1.84, 1.12),
            new Tuple<double,double,double>(1.52, 1.88, 1.92),
            new Tuple<double,double,double>(1.8, 1.76, 0),
            new Tuple<double,double,double>(1.84, 1.84, 0.52),
            new Tuple<double,double,double>(1.84, 1.88, 1.16),
            new Tuple<double,double,double>(1.92, 1.92, 1.92),
            new Tuple<double,double,double>(2.12, 1.8, 0),
            new Tuple<double,double,double>(2.16, 1.84, 0.56),
            new Tuple<double,double,double>(2.2, 1.88, 1.16),
            new Tuple<double,double,double>(2.24, 1.92, 1.92),
            new Tuple<double,double,double>(0, 2, 0),
            new Tuple<double,double,double>(0, 2.04, 0.4),
            new Tuple<double,double,double>(0, 2.08, 1.08),
            new Tuple<double,double,double>(0, 2.12, 1.8),
            new Tuple<double,double,double>(0, 2.04, 0),
            new Tuple<double,double,double>(0, 2.08, 0.48),
            new Tuple<double,double,double>(0, 2.12, 1.12),
            new Tuple<double,double,double>(0, 2.16, 1.84),
            new Tuple<double,double,double>(0.36, 2.08, 0),
            new Tuple<double,double,double>(0.4, 2.08, 0.48),
            new Tuple<double,double,double>(0.48, 2.12, 1.12),
            new Tuple<double,double,double>(0.52, 2.16, 1.84),
            new Tuple<double,double,double>(0.72, 2.08, 0),
            new Tuple<double,double,double>(0.76, 2.12, 0.48),
            new Tuple<double,double,double>(0.8, 2.16, 1.12),
            new Tuple<double,double,double>(0.84, 2.16, 1.88),
            new Tuple<double,double,double>(1.08, 2.08, 0),
            new Tuple<double,double,double>(1.12, 2.12, 0.48),
            new Tuple<double,double,double>(1.16, 2.16, 1.12),
            new Tuple<double,double,double>(1.2, 2.24, 1.92),
            new Tuple<double,double,double>(1.4, 2.08, 0),
            new Tuple<double,double,double>(1.44, 2.16, 0.52),
            new Tuple<double,double,double>(1.52, 2.2, 1.16),
            new Tuple<double,double,double>(1.52, 2.24, 1.92),
            new Tuple<double,double,double>(1.84, 2.12, 0),
            new Tuple<double,double,double>(1.84, 2.16, 0.56),
            new Tuple<double,double,double>(1.88, 2.24, 1.2),
            new Tuple<double,double,double>(1.92, 2.24, 1.92),
            new Tuple<double,double,double>(2.16, 2.12, 0),
            new Tuple<double,double,double>(2.16, 2.2, 0.56),
            new Tuple<double,double,double>(2.2, 2.24, 1.2),
            new Tuple<double,double,double>(2.24, 2.24, 1.96),
        };

        protected override ProfiRendererParams CreateProfiRendererParams()
        {
            var timing = base.CreateProfiRendererParams();
            timing.c_ulaProfiColor = true;
            return timing;
        }

        private void SetPalette(int index, byte value)
        {
            //LogAgent.Info("WR PAL[#{0:X2}] = #{1:X2} @ PC=#{2:X4}", index, value, CPU.regs.PC);
            m_pal[index] = value;
            Renderer.UpdatePalette(index, m_pal_map[value]);
        }

        private void InitStaticTables()
        {
            // Build table from real voltages
            for (int i = 0; i < 0x100; i++)
            {
                var K = 224;

                // Let assume that 1.92 V is the voltage level, which corresponds to maximally bright color component
                // All voltage changes above this level are ignored - i.e., generate the same color exactly
                // We want this level to be converted into K value when getting RGB bytes
                var a1 = K / 1.92; 

                // Simulate overflow effect: Profi signal levels are far from being ideal.
                // This has a strange effect of (r=111, g=111, b=110) color being white insteal of yellowish.
                // We attempt to simulate this behavior by making all color components with level more than 1.92 V equally bright.
                Func<double, byte> crop = v => {
                    var result = (byte)Math.Min(255, (int)(a1 * v));
                    return result > K ? (byte)255 : result;
                };

                var vlt = _palletteVoltages[i];

                // Get colors from voltages
                var r = crop(vlt.Item1);
                var g = crop(vlt.Item2);
                var b = crop(vlt.Item3);

                m_pal_map[i] = 0xFF000000 | (uint)((byte)r << 16) | (uint)((byte)g << 8) | (uint)b;
            }
        }

    }
}
