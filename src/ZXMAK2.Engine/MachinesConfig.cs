using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Reflection;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Engine
{
    public class MachinesConfig
    {
        private XmlDocument m_config = new XmlDocument();

        public void Load()
        {
            const string configFileName = "machines.config";
            var fileSystem = Locator.TryResolve<IHostFileSystem>();
            if (fileSystem.FileExists(configFileName))
            {
                Load(configFileName);
            }
        }

        public void Load(string fileName)
        {
            var fileSystem = Locator.TryResolve<IHostFileSystem>();
            m_config.LoadXml(fileSystem.ReadAllText(fileName));
        }

        public IEnumerable<string> GetNames()
        {
            var names = m_config.DocumentElement.ChildNodes
                .OfType<XmlNode>()
                .Where(node => node.Name == "Bus")
                .Select(node => GetAttrString(node, "name"))
                .Where(v => !string.IsNullOrEmpty(v));
            return names;
        }

        public XmlNode GetConfig(string name)
        {
            var configNode = m_config.DocumentElement.ChildNodes
                .OfType<XmlNode>()
                .FirstOrDefault(node => node.Name=="Bus" && GetAttrString(node, "name")==name);
            return configNode;
        }

        public XmlNode GetDefaultConfig()
        {
            var configNode = m_config.DocumentElement.ChildNodes
                .OfType<XmlNode>()
                .FirstOrDefault(node => GetAttrBool(node, "isDefault", false));
            if (configNode != null)
            {
                return configNode;
            }
            var firstName = GetNames().FirstOrDefault();
            if (firstName == null)
            {
                return null;
            }
            return GetConfig(firstName);
        }

        private static string GetAttrString(XmlNode node, string name)
        {
            var attr = node.Attributes[name];
            if (attr == null)
            {
                return null;
            }
            return attr.InnerText;
        }

        private static bool GetAttrBool(XmlNode node, string name, bool defValue)
        {
            var attr = node.Attributes[name];
            if (attr == null)
            {
                return defValue;
            }
            return string.Compare(attr.InnerText, "true", true)==0;
        }
    }
}
