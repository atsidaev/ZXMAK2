using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.Quorum
{
	public class MemoryQuorum1024 : MemoryQuorum256
	{
		private UlaQuorum1024Plus _ula;

		public MemoryQuorum1024() : base(64)
		{
			OnCmr1Change += OnOnCmr1Change;
		}

		private void OnOnCmr1Change()
		{
			if (_ula != null)
				_ula.IsModePentagon = (CMR1 & Q_PENTAGON) != 0;
		}

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);
			_ula = bmgr.FindDevice<UlaQuorum1024Plus>();
		}

		protected override int GetRamPage()
		{
			var ramPage = CMR0 & 7;
			ramPage |= ((CMR0 & 0xE0) >> 2);
			if ((CMR0 & Q_BLK_128) != 0)
				ramPage &= 0x07; // 128 kb mode
			else
				ramPage &= 0x3F; // Use all 1024 Kb
			return ramPage;
		}
	}
}