using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OneClickClearScreen
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClearScreenContext());
        }
    }

    internal sealed class ClearScreenContext : ApplicationContext
    {
        private readonly List<OverlayForm> forms = new List<OverlayForm>();
        private bool closing;

        public ClearScreenContext()
        {
            Screen[] screens = Screen.AllScreens;
            if (screens.Length == 0)
            {
                screens = new[] { Screen.PrimaryScreen };
            }

            for (int index = 0; index < screens.Length; index++)
            {
                bool primary = screens[index].Primary || index == 0;
                OverlayForm form = new OverlayForm(screens[index].Bounds, primary, CloseAll);
                forms.Add(form);
                form.Show();
            }
        }

        private void CloseAll()
        {
            if (closing)
            {
                return;
            }

            closing = true;
            foreach (OverlayForm form in forms.ToArray())
            {
                if (!form.IsDisposed)
                {
                    form.Close();
                }
            }

            ExitThread();
        }
    }

    internal sealed class OverlayForm : Form
    {
        private readonly Action closeAll;

        public OverlayForm(Rectangle bounds, bool showHint, Action closeAll)
        {
            this.closeAll = closeAll;

            Bounds = bounds;
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            ForeColor = Color.White;
            TopMost = true;
            ShowInTaskbar = showHint;
            KeyPreview = true;
            Cursor = Cursors.Default;

            if (showHint)
            {
                Label hint = new Label();
                hint.AutoSize = true;
                hint.Text = "Esc or click to restore";
                hint.ForeColor = Color.FromArgb(180, 180, 180);
                hint.BackColor = Color.Black;
                hint.Font = new Font("Segoe UI", 10.0f, FontStyle.Regular);
                hint.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                hint.Location = new Point(bounds.Width - 180, bounds.Height - 36);
                hint.Click += delegate { closeAll(); };
                Controls.Add(hint);
            }

            Click += delegate { closeAll(); };
            KeyDown += OnKeyDown;
            Deactivate += delegate { TopMost = true; };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            WindowState = FormWindowState.Maximized;
            Activate();
            Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                closeAll();
            }
        }
    }
}
