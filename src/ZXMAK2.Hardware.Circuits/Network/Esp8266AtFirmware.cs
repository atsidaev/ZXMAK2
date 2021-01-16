using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ZXMAK2.Hardware.Circuits.Network
{
    public delegate void InternetDataReceived(byte[] data);
    public class Esp8266AtFirmware
    {
        public const string OK = "OK";
        public const string READY = "ready";

        private int _initialAwaitedLength = 0;
        private int _awaitedLength = 0;

        private Socket _socket = null;
        private Thread _socketReaderThread = null;
        private InternetDataReceived _internetDataReceivedCallback;

        public bool IsAwaitingDataViaUart => _awaitedLength > 0;

        public List<byte> _uartDataBuffer = new List<byte>();

        public Esp8266AtFirmware(InternetDataReceived onDataReceived)
        {
            _internetDataReceivedCallback += onDataReceived;
        }

        private string ExtractCommand(string command)
        {
            while (command.StartsWith("+"))
                command = command.Substring(1);
            if (command.StartsWith("AT+"))
                command = command.Substring(3);

            return command.Trim();
        }

        private string GetResponse(string response)
        {
            return response + "\r\n";
        }

        public string RunCommand(string command)
        {
            command = ExtractCommand(command);
            var parts = command.Split('=');
            switch (parts[0])
            {
                case "CWMODE_DEF":
                case "UART_DEF":
                case "CWJAP_DEF": // Connect to AP, allow always
                case "ATE0": // Disable echo
                case "CIPMUX": // Connection mode (single/multiple)
                case "CIPDINFO": // Show remote IP addr and port
                    return GetResponse(OK);
                case "CWLAP":
                    // One hardcoded access point
                    return GetResponse("+CWLAP:(0,\"ZXMAK2\",-10,\"00:11:22:33:44:55\")\r\n\r\nOK");
                case "RST":
                    return GetResponse(READY);
                case "CIPSTART": // Initiate connection
                    {
                        var arguments = parts[1].Split(',');

                        IPAddress remoteIpAddress = null;
                        try
                        {
                            IPHostEntry remoteHost = Dns.GetHostEntry(arguments[1].Replace("\"", ""));
                            remoteIpAddress = remoteHost.AddressList[0];
                        }
                        catch (SocketException ex)
                        {
                            return GetResponse("DNS Fail\r\n\r\nERROR");
                        }

                        IPEndPoint remoteEP = new IPEndPoint(remoteIpAddress, int.Parse(arguments[2]));

                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, arguments[0].Replace("\"", "") == "TCP" ? ProtocolType.Tcp : ProtocolType.Udp);
                        _socketReaderThread = new Thread(SocketReaderThreadProc);
                        _socket.Connect(remoteEP);
                        _socketReaderThread.Start(_socket);

                        return GetResponse(OK);
                    }
                case "CIPSEND":
                    _awaitedLength = int.Parse(parts[1]);
                    _initialAwaitedLength = _awaitedLength;
                    return "OK\r\n>";
            }

            return GetResponse(OK);
        }

        public string CollectData(params byte[] data)
        {
            _uartDataBuffer.AddRange(data.Take(_awaitedLength));
            _awaitedLength -= data.Length;

            if (_awaitedLength <= 0)
            {
                _socket.Send(_uartDataBuffer.ToArray());
                _uartDataBuffer.Clear();
                return GetResponse($"Recv {_initialAwaitedLength} bytes\r\nSEND OK\r\n");
            }

            return null;
        }

        public void SocketReaderThreadProc(object o)
        {
            var socket = (Socket)o;
            while (true)
            {
                var buf = new byte[0x10000];
                var handler = socket.BeginReceive(buf, 0, 0x10000, SocketFlags.None, null, null);

                WaitHandle.WaitAny(new[] { handler.AsyncWaitHandle });

                var bytesReceived = socket.EndReceive(handler);
                if (bytesReceived == 0 && socket.Connected)
                    socket.Close();

                // Since we have a limit for one +IPD command, split to several blocks
                int skip = 0;
                while (bytesReceived > 0)
                {
                    var portionSize = Math.Min(bytesReceived, 4096);
                    var portion = buf.Skip(skip).Take(portionSize);

                    bytesReceived -= portionSize;
                    skip += portionSize;

                    var response = new List<byte>();
                    if (portionSize > 0)
                    {
                        response.AddRange(Encoding.ASCII.GetBytes($"+IPD,{portionSize}:"));
                        response.AddRange(portion);
                        response.AddRange(Encoding.ASCII.GetBytes("\r\n"));
                    }

                    if (response.Count > 0)
                        _internetDataReceivedCallback(response.ToArray());
                }


                if (!socket.Connected)
                {
                    _internetDataReceivedCallback(Encoding.ASCII.GetBytes("CLOSED\r\n"));
                    return;
                }
            }
        }
    }
}
