using System;

using ZXMAK2.Hardware.Circuits.Fdd;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Host.WinForms.Views;


namespace ZXMAK2.Hardware.WinForms
{
    public partial class dbgWD1793 : FormView, IFddDebugView
    {
        private IWd1793Wrapper _wd;

        public dbgWD1793(IWd1793Wrapper debugTarget)
        {
            _wd = debugTarget;
            InitializeComponent();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (_wd != null)
                label1.Text = _wd.DumpState();
            else
                label1.Text = "Beta Disk interface not found";
        }
    }
}