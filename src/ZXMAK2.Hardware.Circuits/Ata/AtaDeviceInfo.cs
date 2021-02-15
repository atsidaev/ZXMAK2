using System;
using System.Xml;
using System.IO;
using System.Reflection;


namespace ZXMAK2.Hardware.Circuits.Ata
{
    public class AtaDeviceInfo
    {
        public enum Mode { None, Chs, Lba, LbaAuto };

        private const int DefaultSectorSize = 512;
        private const int DefaultHeadsCount = 255;
        private const int DefaultSectorsPerTrack = 63;

        private const string DefaultSerial = "00000000001234567890";
        private const string DefaultModel = "ZXMAK2 HDD IMAGE";
        
        public string FileName { get; private set; }
        public uint Cylinders { get; private set; }
        public uint Heads { get; private set; }
        public uint Sectors { get; private set; }
        public uint Lba { get; private set; }
        public Mode AddressingMode { get; private set; }
        public bool ReadOnly { get; private set; }
        public bool IsCdrom { get; private set; }

        public string SerialNumber { get; private set; }        // 20 chars
        public string FirmwareRevision { get; private set; }    // 8 chars
        public string ModelNumber { get; private set; }         // 40 chars

        
        public AtaDeviceInfo()
        {
            AddressingMode = Mode.Chs;
            Cylinders = 20; 
            Heads = 16;
            Sectors = 63; 
            Lba = 20160;
            ReadOnly = true;
            IsCdrom = false;
            SerialNumber = DefaultSerial;
            FirmwareRevision = GetVersion();
            ModelNumber = DefaultModel;
        }
        
        public void Save(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.AppendChild(xml.CreateElement("IdeDiskDescriptor"));
            XmlNode imageNode = root.AppendChild(xml.CreateElement("Image"));
            string imageFile = FileName ?? string.Empty;
            if (imageFile != string.Empty &&
                Path.GetDirectoryName(imageFile) == Path.GetDirectoryName(fileName))
            {
                imageFile = Path.GetFileName(imageFile);
            }
            Utils.SetXmlAttribute(imageNode, "fileName", imageFile);
            Utils.SetXmlAttribute(imageNode, "isReadOnly", ReadOnly);
            Utils.SetXmlAttribute(imageNode, "isCdrom", IsCdrom);
            Utils.SetXmlAttribute(imageNode, "serial", SerialNumber);
            Utils.SetXmlAttribute(imageNode, "revision", FirmwareRevision);
            Utils.SetXmlAttribute(imageNode, "model", ModelNumber);

            if (AddressingMode != Mode.LbaAuto && AddressingMode != Mode.None)
            {
                XmlNode geometryNode = root.AppendChild(xml.CreateElement("Geometry"));
                switch (AddressingMode)
                {
                    case Mode.Chs:
                        Utils.SetXmlAttribute(geometryNode, "cylinders", Cylinders);
                        Utils.SetXmlAttribute(geometryNode, "heads", Heads);
                        Utils.SetXmlAttribute(geometryNode, "sectors", Sectors);
                        break;
                    case Mode.Lba:
                        Utils.SetXmlAttribute(geometryNode, "lba", Lba);
                        break;
                }
            }

            xml.Save(fileName);
        }

        public void Load(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(fileName);
            XmlNode root = xml["IdeDiskDescriptor"];
            XmlNode imageNode = root["Image"];
            FileName = Utils.GetXmlAttributeAsString(imageNode, "fileName", FileName ?? string.Empty);
            if (FileName != string.Empty && !Path.IsPathRooted(FileName))
                FileName = Utils.GetFullPathFromRelativePath(FileName, Path.GetDirectoryName(fileName));
            SerialNumber = Utils.GetXmlAttributeAsString(imageNode, "serial", SerialNumber);
            FirmwareRevision = Utils.GetXmlAttributeAsString(imageNode, "revision", FirmwareRevision);
            ModelNumber = Utils.GetXmlAttributeAsString(imageNode, "model", ModelNumber);
            IsCdrom = Utils.GetXmlAttributeAsBool(imageNode, "isCdrom", false);
            ReadOnly = Utils.GetXmlAttributeAsBool(imageNode, "isReadOnly", true);

            // If LBA is set, then use LBA mode. If nothing is set (LBA, C,H,S) - use LBA mode and autodetect size
            // Otherwise use CHS mode
            XmlNode geometryNode = root["Geometry"];
            if (geometryNode != null)
            {
                Lba = Utils.GetXmlAttributeAsUInt32(geometryNode, "lba", 0);
                if (Lba > 0)
                    AddressingMode = Mode.Lba;
                else
                {
                    Cylinders = Utils.GetXmlAttributeAsUInt32(geometryNode, "cylinders", 0);
                    Heads = Utils.GetXmlAttributeAsUInt32(geometryNode, "heads", 0);
                    Sectors = Utils.GetXmlAttributeAsUInt32(geometryNode, "sectors", 0);
                    if (Cylinders > 0 || Heads > 0 || Sectors > 0)
                        AddressingMode = Mode.Chs;
                    else
                        AddressingMode = Mode.LbaAuto;
                }
            }

            if (AddressingMode == Mode.LbaAuto)
            {
                // autodetect LBA size by file size
                var fileInfo = new FileInfo(FileName);
                if (fileInfo.Exists)
                {
                    var fileSize = fileInfo.Length;
                    Lba = (uint)(fileSize / DefaultSectorSize + (fileSize % DefaultSectorSize == 0 ? 0 : 1));
                    AddressingMode = Mode.LbaAuto;
                }
                else
                    AddressingMode = Mode.None; // Emulated machine will not be able to detect the drive or will operate with errors
            }

            // Autofill some CHS for proper functioning
            if (AddressingMode == Mode.Lba || AddressingMode == Mode.LbaAuto)
            {
                // From wiki: https://en.wikipedia.org/wiki/Logical_block_addressing
                Cylinders = Lba / (DefaultHeadsCount * DefaultSectorsPerTrack);
                Heads = (Lba / DefaultSectorsPerTrack) % DefaultHeadsCount;
                Sectors = (Lba % DefaultSectorsPerTrack) + 1;
            }
            else // Or autofill LBA from CHS (from same wiki page)
            {
                Lba = (Cylinders * DefaultHeadsCount + Heads) * DefaultSectorsPerTrack + (Sectors - 1);
            }
        }

        private static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString();
        }
    }
}
