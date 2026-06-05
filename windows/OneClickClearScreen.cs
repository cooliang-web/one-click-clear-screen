using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

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
        private readonly Image wallpaperImage;

        public OverlayForm(Rectangle bounds, bool showInterface, Action closeAll)
        {
            this.closeAll = closeAll;
            wallpaperImage = WallpaperRenderer.CreateScreenImage(bounds);

            Bounds = bounds;
            Text = "一键清屏";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = WallpaperRenderer.GetDesktopColor();
            BackgroundImage = wallpaperImage;
            BackgroundImageLayout = ImageLayout.Stretch;
            ForeColor = Color.FromArgb(80, 80, 80);
            TopMost = true;
            ShowInTaskbar = showInterface;
            KeyPreview = true;
            Cursor = Cursors.Default;

            if (showInterface)
            {
                AddCompactInterface(bounds.Size);
            }

            Click += delegate { closeAll(); };
            KeyDown += OnKeyDown;
            Deactivate += delegate { TopMost = true; };
        }

        private void AddCompactInterface(Size screenSize)
        {
            const double scale = 0.55;
            int width = Math.Max(360, (int)Math.Round(screenSize.Width * scale));
            int height = Math.Max(240, (int)Math.Round(screenSize.Height * scale));
            width = Math.Min(width, screenSize.Width - 24);
            height = Math.Min(height, screenSize.Height - 24);

            Panel panel = new Panel();
            panel.Size = new Size(width, height);
            panel.Location = new Point((screenSize.Width - width) / 2, (screenSize.Height - height) / 2);
            panel.BackColor = Color.FromArgb(246, 246, 246);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Click += delegate { closeAll(); };
            Controls.Add(panel);
            panel.BringToFront();

            Label title = new Label();
            title.Dock = DockStyle.Top;
            title.Height = 86;
            title.Text = "一键清屏";
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.ForeColor = Color.FromArgb(25, 25, 25);
            title.BackColor = Color.FromArgb(246, 246, 246);
            title.Font = new Font("Microsoft YaHei UI", 24.0f, FontStyle.Bold);
            title.Click += delegate { closeAll(); };
            panel.Controls.Add(title);

            Label body = new Label();
            body.Dock = DockStyle.Fill;
            body.Text = "桌面图标已隐藏在清屏界面后面";
            body.TextAlign = ContentAlignment.MiddleCenter;
            body.ForeColor = Color.FromArgb(80, 80, 80);
            body.BackColor = Color.FromArgb(246, 246, 246);
            body.Font = new Font("Microsoft YaHei UI", 12.0f, FontStyle.Regular);
            body.Click += delegate { closeAll(); };
            panel.Controls.Add(body);
            body.BringToFront();

            Label hint = new Label();
            hint.Dock = DockStyle.Bottom;
            hint.Height = 46;
            hint.Text = "Esc 或点击窗口恢复";
            hint.TextAlign = ContentAlignment.MiddleCenter;
            hint.ForeColor = Color.FromArgb(120, 120, 120);
            hint.BackColor = Color.FromArgb(246, 246, 246);
            hint.Font = new Font("Microsoft YaHei UI", 10.0f, FontStyle.Regular);
            hint.Click += delegate { closeAll(); };
            panel.Controls.Add(hint);
            hint.BringToFront();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && wallpaperImage != null)
            {
                wallpaperImage.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    internal static class WallpaperRenderer
    {
        private const int SpiGetDesktopWallpaper = 0x0073;

        private enum WallpaperMode
        {
            Center,
            Tile,
            Stretch,
            Fit,
            Fill,
            Span
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int action, int param, StringBuilder value, int update);

        public static Color GetDesktopColor()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors"))
                {
                    string value = key == null ? null : key.GetValue("Background") as string;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            return Color.FromArgb(
                                ClampColor(parts[0]),
                                ClampColor(parts[1]),
                                ClampColor(parts[2]));
                        }
                    }
                }
            }
            catch
            {
            }

            return SystemColors.Desktop;
        }

        public static Image CreateScreenImage(Rectangle windowBounds)
        {
            Bitmap image = new Bitmap(Math.Max(1, windowBounds.Width), Math.Max(1, windowBounds.Height));
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(GetDesktopColor());
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                string wallpaperPath = GetWallpaperPath();
                if (string.IsNullOrWhiteSpace(wallpaperPath) || !File.Exists(wallpaperPath))
                {
                    return image;
                }

                try
                {
                    using (Image wallpaper = Image.FromFile(wallpaperPath))
                    {
                        WallpaperMode mode = GetWallpaperMode();
                        Rectangle monitorBounds = GetContainingScreenBounds(windowBounds);
                        GraphicsState state = graphics.Save();
                        graphics.TranslateTransform(-windowBounds.Left, -windowBounds.Top);

                        if (mode == WallpaperMode.Span)
                        {
                            DrawSpan(graphics, wallpaper);
                        }
                        else if (mode == WallpaperMode.Tile)
                        {
                            DrawTiled(graphics, wallpaper, monitorBounds);
                        }
                        else
                        {
                            RectangleF destination = GetDestinationRectangle(wallpaper, monitorBounds, mode);
                            graphics.DrawImage(wallpaper, destination);
                        }

                        graphics.Restore(state);
                    }
                }
                catch
                {
                    graphics.Clear(GetDesktopColor());
                }
            }

            return image;
        }

        private static Rectangle GetContainingScreenBounds(Rectangle bounds)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Bounds.Contains(bounds.Location))
                {
                    return screen.Bounds;
                }
            }

            return Screen.PrimaryScreen == null ? SystemInformation.VirtualScreen : Screen.PrimaryScreen.Bounds;
        }

        private static void DrawSpan(Graphics graphics, Image wallpaper)
        {
            Rectangle virtualBounds = SystemInformation.VirtualScreen;
            RectangleF destination = GetDestinationRectangle(wallpaper, virtualBounds, WallpaperMode.Fill);
            graphics.DrawImage(wallpaper, destination);
        }

        private static void DrawTiled(Graphics graphics, Image wallpaper, Rectangle bounds)
        {
            for (int y = 0; y < bounds.Height; y += wallpaper.Height)
            {
                for (int x = 0; x < bounds.Width; x += wallpaper.Width)
                {
                    graphics.DrawImage(wallpaper, x, y, wallpaper.Width, wallpaper.Height);
                }
            }
        }

        private static RectangleF GetDestinationRectangle(Image wallpaper, Rectangle bounds, WallpaperMode mode)
        {
            if (mode == WallpaperMode.Stretch)
            {
                return bounds;
            }

            if (mode == WallpaperMode.Center)
            {
                return new RectangleF(
                    bounds.Left + (bounds.Width - wallpaper.Width) / 2.0f,
                    bounds.Top + (bounds.Height - wallpaper.Height) / 2.0f,
                    wallpaper.Width,
                    wallpaper.Height);
            }

            float xScale = bounds.Width / (float)wallpaper.Width;
            float yScale = bounds.Height / (float)wallpaper.Height;
            float scale = mode == WallpaperMode.Fit ? Math.Min(xScale, yScale) : Math.Max(xScale, yScale);
            float width = wallpaper.Width * scale;
            float height = wallpaper.Height * scale;
            return new RectangleF(
                bounds.Left + (bounds.Width - width) / 2.0f,
                bounds.Top + (bounds.Height - height) / 2.0f,
                width,
                height);
        }

        private static WallpaperMode GetWallpaperMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    string style = key == null ? null : key.GetValue("WallpaperStyle") as string;
                    string tile = key == null ? null : key.GetValue("TileWallpaper") as string;
                    if (tile == "1")
                    {
                        return WallpaperMode.Tile;
                    }

                    switch (style)
                    {
                        case "0":
                            return WallpaperMode.Center;
                        case "2":
                            return WallpaperMode.Stretch;
                        case "6":
                            return WallpaperMode.Fit;
                        case "22":
                            return WallpaperMode.Span;
                        case "10":
                        default:
                            return WallpaperMode.Fill;
                    }
                }
            }
            catch
            {
                return WallpaperMode.Fill;
            }
        }

        private static string GetWallpaperPath()
        {
            try
            {
                StringBuilder builder = new StringBuilder(1024);
                if (SystemParametersInfo(SpiGetDesktopWallpaper, builder.Capacity, builder, 0) != 0)
                {
                    string path = builder.ToString();
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch
            {
            }

            try
            {
                string transcoded = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Themes\TranscodedWallpaper");
                if (File.Exists(transcoded))
                {
                    return transcoded;
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private static int ClampColor(string value)
        {
            int parsed;
            if (!int.TryParse(value, out parsed))
            {
                return 0;
            }

            return Math.Max(0, Math.Min(255, parsed));
        }
    }
}
