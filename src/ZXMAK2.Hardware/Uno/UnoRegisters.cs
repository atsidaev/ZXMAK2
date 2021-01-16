using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.Uno
{
    public class UnoRegisters : BusDeviceBase
    {
        private bool _isSandbox;

        private IUnoRegisterDevice[] _registerDevices;
        private byte _currentAddress;

        public UnoRegisters()
        {
            Category = BusDeviceCategory.Other;
            //Name = "ZX Uno Registers emulation"; // Use this when UI for device attachment will be ready
            Name = "ZX Uno UART with ESP8266";     // And for now this name is much less confusing
            Description = "Emulation of Uno registers. It is required for registry-based Uno devices.";

            _registerDevices = new IUnoRegisterDevice[256];
        }

        # region BusDeviceBase
        public override void BusInit(IBusManager bmgr)
        {
            _isSandbox = bmgr.IsSandbox;

            // IOADDR
            bmgr.Events.SubscribeRdIo(0xFFFF, 0xFC3B, ReadIoAddr);
            bmgr.Events.SubscribeWrIo(0xFFFF, 0xFC3B, WriteIoAddr);

            // IODATA
            bmgr.Events.SubscribeRdIo(0xFFFF, 0xFD3B, ReadIoData);
            bmgr.Events.SubscribeWrIo(0xFFFF, 0xFD3B, WriteIoData);
        }

        public override void BusConnect()
        {
            if (!_isSandbox)
            {
                // Hardcoding for now
                var uart = new UnoUart();
                _registerDevices[0xC6] = uart; // UART_DATA_REG
                _registerDevices[0xC7] = uart; // UART_STAT_REG
            }
        }

        public override void BusDisconnect()
        {
        }
        #endregion

        # region Bus Handlers
        protected virtual void WriteIoAddr(ushort addr, byte val, ref bool handled)
        {
            _currentAddress = val;
            handled = true;
        }

        protected virtual void ReadIoAddr(ushort addr, ref byte val, ref bool handled)
        {
            Console.WriteLine();
            handled = false;
        }

        protected virtual void WriteIoData(ushort addr, byte val, ref bool handled)
        {
            var currentDevice = _registerDevices[_currentAddress];
            currentDevice?.WriteRegister(_currentAddress, val);
            handled = true;
        }

        protected virtual void ReadIoData(ushort addr, ref byte val, ref bool handled)
        {
            var currentDevice = _registerDevices[_currentAddress];
            val = currentDevice?.ReadRegister(_currentAddress) ?? 0xFF;
            handled = true;
        }

        # endregion
    }
}
