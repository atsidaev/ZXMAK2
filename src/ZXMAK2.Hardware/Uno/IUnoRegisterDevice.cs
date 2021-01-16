using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Hardware.Uno
{
    public interface IUnoRegisterDevice
    {
        void WriteRegister(byte addr, byte data);
        byte ReadRegister(byte addr);
    }
}
