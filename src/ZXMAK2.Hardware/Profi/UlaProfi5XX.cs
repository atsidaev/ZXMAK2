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
                value &= 0x7F;    // UniCopy by Michael Markowsky

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

        // To convert colors from RGB332 to RGB888 we need to convert 3-bit color to 8-bit.
        // (0x11111111 / 0x111) = 36.42857142857143, which is float and its usage is slow/inaccurate in runtime,
        // so we better have rounded precaclulated table.
        private readonly byte[] m_colors3to8BitsRecalcTable = {
            0, 36, 73, 109, 146, 182, 219, 255
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
            //build tables...
            for (int i = 0; i < 0x100; i++)
            {
                //g4g2g1 r4r2r1 b4b2 (naming from palet.com)
                int g = m_colors3to8BitsRecalcTable[(i >> 5) & 7];
                int r = m_colors3to8BitsRecalcTable[(i >> 2) & 7];
                int b = m_colors3to8BitsRecalcTable[(i << 1) & 7];
                m_pal_map[i] = 0xFF000000 | (uint)(r << 16) | (uint)(g << 8) | (uint)b;
            }
        }

    }
}
