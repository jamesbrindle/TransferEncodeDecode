using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferEncodeDecode.Business;

namespace TransferEncodeDecode
{
    public partial class MainForm : Form
    {
        #region Presentation

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(
            IntPtr hwnd,
            DWMWINDOWATTRIBUTE attribute,
            ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
            uint cbAttribute);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    if (m.Result == (IntPtr)HTCLIENT)
                    {
                        m.Result = (IntPtr)HTCAPTION;
                    }
                    break;
            }
        }

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        public void SetFormRoundCorners()
        {
            try
            {
                var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                DwmSetWindowAttribute(this.Handle, attribute, ref preference, sizeof(uint));
            }
            catch { }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                var cp = base.CreateParams;
                cp.Style &= ~0xC00000 /* No title bar */ | 0x00800000 /* fixed single border */;
                return cp;
            }
        }

        #endregion

        private readonly EncodeDecode EncodeDecode;
        private readonly TransferType TransferType;

        public MainForm(TransferType transferType)
        {
            EncodeDecode = new EncodeDecode(this);

            TransferType = transferType;

            InitializeComponent();
            SetFormRoundCorners();
#if DEBUG
            CheckForIllegalCrossThreadCalls = true;
#else
            CheckForIllegalCrossThreadCalls = false;
#endif
            this.BringToFront();
            this.Activate();
            this.Focus();
            SetForegroundWindow(this.Handle.ToInt32());

            new Thread((ThreadStart)delegate
            {
                while (!Program.StartProcess)
                    Thread.Sleep(50);

            }).Start();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (TransferType == TransferType.Encode)
            {
                SetLabelText(Program.InputFiles.Count > 1 ? "Encoding" : Path.GetFileName(Program.InputFiles[0]));
                Task.Factory.StartNew(() =>
                {
                    EncodeDecode.Encode();
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
               .ContinueWith(task => { }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(50);
                    EncodeDecode.Decode(Program.InputFiles[0]);
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
               .ContinueWith(task => { }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public delegate void SetLabelTextDelegate(string text);
        public void SetLabelText(string text)
        {
            if (lblLabel.InvokeRequired)
            {
                var d = new SetLabelTextDelegate(SetLabelText);
                Invoke(d, new object[] { text });
            }
            else
            {
                lblLabel.Text = text;
                lblLabel.Visible = true;
            }
        }

        private void PbPreloader_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
