namespace ZXMAK2.Hardware.Quorum
{
	public class MemoryQuorum128: MemoryQuorum256
	{
		public MemoryQuorum128() : base(8)
		{
		}
		protected override int GetRamPage() => CMR0 & 7;
	}
}