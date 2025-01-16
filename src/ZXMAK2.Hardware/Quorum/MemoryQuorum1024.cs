namespace ZXMAK2.Hardware.Quorum
{
	public class MemoryQuorum1024 : MemoryQuorum256
	{
		public MemoryQuorum1024() : base(64)
		{
			
		}
		protected override int GetRamPage()
		{
			var ramPage = CMR0 & 7;
			ramPage |= ((CMR0 & 0xE0) >> 2);
			ramPage &= 0x3F; //1024 Kb
			return ramPage;
		}
	}
}