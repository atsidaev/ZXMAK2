using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Hardware.Circuits.Network;

namespace ZXMAK2.Hardware.Uno
{
    public class UnoUart : IUnoRegisterDevice
    {
        public const byte UARTDATA = 0xC6;
        public const byte UARTSTAT = 0xC7;

        private byte _status = 0;
        private byte _data = 0;

        private ISerialDevice _attachedDevice = new Esp8266AT();

        public byte ReadRegister(byte addr)
        {
            switch (addr)
            {
                case UARTDATA:
                    {
                        byte data;
                        if (_attachedDevice.ReadByte(out data))
                            return data;
                        return 0;
                    }
                case UARTSTAT:
                    return (byte)((_attachedDevice.WasReceived() ? 1 : 0) << 7 | (_attachedDevice.IsSending() ? 1 : 0) << 6);
                default:
                    throw new Exception("UART: Reading invalid register " + addr);
            }
        }

        public void WriteRegister(byte addr, byte data)
        {
            if (addr != UARTDATA)
                throw new Exception("UART: Writing invalid register " + addr);

            _attachedDevice.SendByte(data);
        }
    }
}
