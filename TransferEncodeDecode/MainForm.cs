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
    public partial class MainForm : BaseForm
    {
        #region Messages

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion

        private readonly EncodeDecode EncodeDecode;

        private readonly TransferType TransferType;
        private readonly string InputPath;

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

        public MainForm(TransferType transferType, string inputPath)
        {
            EncodeDecode = new EncodeDecode(this);

            TransferType = transferType;
            InputPath = inputPath;

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
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (TransferType == TransferType.Encode)
            {
                SetLabelText(Path.GetFileName(InputPath));
                Task.Factory.StartNew(() =>
                {
                    EncodeDecode.Encode(InputPath);
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
               .ContinueWith(task => { }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {               
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(50);
                    EncodeDecode.Decode(InputPath);
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
    }
}
