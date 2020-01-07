using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Host.Entities
{
    public class IconDescriptor : IIconDescriptor
	{
        public string Name { get; private set; }
        public Size Size { get; private set; }
		public bool Visible { get; set; }

        public IconDescriptor(string iconName, Image iconImage)
        {
            Name = iconName;
            Size = new Size(0, 0);
        }
        
		public Stream GetImageStream()
		{
			return new MemoryStream(new byte[0]);
		}
	}
}
