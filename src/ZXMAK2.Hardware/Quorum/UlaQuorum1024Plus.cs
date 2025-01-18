using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.Pentagon;

namespace ZXMAK2.Hardware.Quorum
{
    public class UlaQuorum1024Plus : UlaQuorum
    {
        private bool _isModePentagon;
        private BusManager _busManager;
        public bool IsModePentagon
        {
            get => _isModePentagon;
            set
            {
                if (_isModePentagon != value)
                {
                    _busManager.Disconnect();
                    _isModePentagon = value;
                    OnRendererInit();
                    _busManager.Connect();
                }
            }
        }

        public UlaQuorum1024Plus()
        {
            Name = "QUORUM 1024+";
        }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            _busManager = (BusManager)bmgr;
        }

        protected override SpectrumRendererParams CreateSpectrumRendererParams()
        {
            if (_isModePentagon)
                return UlaPentagon.GetTimings();
            return base.CreateSpectrumRendererParams();
        }
    }
}
