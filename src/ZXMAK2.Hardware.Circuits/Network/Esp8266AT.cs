using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Hardware.Circuits.Network
{
    public interface ISerialDevice
    {
        void SendByte(byte data);
        bool ReadByte(out byte data);
        bool IsSending();
        bool WasReceived();
    }
    public class Esp8266AT : ISerialDevice
    {
        /// <summary>
        /// Data, which was sent to ESP8266 via UART
        /// </summary>
        private List<byte> _uartIncomingBuffer = new List<byte>();

        /// <summary>
        /// Data, which will be sent by ESP8266 to UART client
        /// </summary>
        private List<byte> _uartOutgoingBuffer = new List<byte>();
        private readonly object _uartOutgoingLock = new object();

        private Esp8266AtFirmware _firmware;

        public Esp8266AT()
        {
            _firmware = new Esp8266AtFirmware(InternetDataReceived);
        }

        public void OnByteSent(byte data)
        {
            if (_firmware.IsAwaitingDataViaUart)
            {
                var response =_firmware.CollectData(data);
                if (response != null)
                    lock(_uartOutgoingLock)
                        _uartOutgoingBuffer.AddRange(Encoding.ASCII.GetBytes(response));

                Logger.Debug(response);
            }
            else
            {
                _uartIncomingBuffer.Add(data);
                if (_uartIncomingBuffer.Count > 1 && _uartIncomingBuffer[_uartIncomingBuffer.Count - 1] == 0x0A && _uartIncomingBuffer[_uartIncomingBuffer.Count - 2] == 0x0D)
                {
                    var s = Encoding.ASCII.GetString(_uartIncomingBuffer.ToArray());
                    Logger.Debug(s);
                    _uartIncomingBuffer.Clear();
                    lock(_uartOutgoingLock)
                    {
                        var response = _firmware.RunCommand(s);
                        Logger.Debug(response);
                        _uartOutgoingBuffer.AddRange(Encoding.ASCII.GetBytes(response));
                    }
                }
            }
        }

        public void InternetDataReceived(byte[] data)
        {
            lock(_uartOutgoingLock)
                _uartOutgoingBuffer.AddRange(data);

            Logger.Debug(Encoding.ASCII.GetString(data));
        }

        # region ISerialDevice

        public bool IsSending()
        {
            // Send is performed immediately
            return false;
        }

        public bool ReadByte(out byte data)
        {
            lock (_uartOutgoingLock)
            {
                if (_uartOutgoingBuffer.Count > 0)
                {
                    data = _uartOutgoingBuffer[0];
                    _uartOutgoingBuffer.RemoveAt(0);
                    return true;
                }
            }
            data = 0;
            return false;
        }

        public void SendByte(byte data)
        {
            OnByteSent(data);
        }

        public bool WasReceived()
        {
            lock (_uartOutgoingLock)
                return _uartOutgoingBuffer.Count > 0;
        }

        # endregion
    }
}
