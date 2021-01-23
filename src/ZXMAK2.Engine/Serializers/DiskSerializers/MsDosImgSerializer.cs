// IMG serializer
// Author: zx.pk.ru\Дмитрий
using ZXMAK2.Model.Disk;


namespace ZXMAK2.Serializers.DiskSerializers
{
    /// <summary>
    /// SPRINTER disk image
    /// </summary>
    public class MsDosImgSerializer : SectorImageSerializerBase
    {
        public MsDosImgSerializer(DiskImage diskImage)
            : base(diskImage)
        {
        }
        
        public override string FormatExtension
        {
            get { return "IMG"; }
        }
        
        protected override int SectorSizeCode
        {
            get { return 2; }
        }

        protected override int[] GetSectorMap(int cyl, int head)
        {
            return new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        }
    }
}
