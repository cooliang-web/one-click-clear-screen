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

        public OverlayForm(Rectangle bounds, bool showHint, Action closeAll)
        {
            this.closeAll = closeAll;
            wallpaperImage = WallpaperRenderer.CreateScreenImage(bounds);

            Bounds = bounds;
            Text = "One Click Clear Screen";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = WallpaperRenderer.GetDesktopColor();
            BackgroundImage = wallpaperImage;
            BackgroundImageLayout = ImageLayout.Stretch;
            ForeColor = Color.FromArgb(80, 80, 80);
            TopMost = true;
            ShowInTaskbar = showHint;
            KeyPreview = true;
            Cursor = Cursors.Default;

            if (showHint)
            {
                Label hint = new Label();
                hint.AutoSize = true;
                hint.Text = "Esc or click to restore";
                hint.ForeColor = Color.FromArgb(130, 130, 130);
                hint.BackColor = Color.Transparent;
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

        public static Image CreateScreenImage(Rectangle screenBounds)
        {
            Bitmap image = new Bitmap(Math.Max(1, screenBounds.Width), Math.Max(1, screenBounds.Height));
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
                        if (mode == WallpaperMode.Span)
                        {
                            DrawSpan(graphics, wallpaper, screenBounds);
                        }
                        else if (mode == WallpaperMode.Tile)
                        {
                            DrawTiled(graphics, wallpaper, new Rectangle(Point.Empty, image.Size));
                        }
                        else
                        {
                            RectangleF destination = GetDestinationRectangle(wallpaper, new Rectangle(Point.Empty, image.Size), mode);
                            graphics.DrawImage(wallpaper, destination);
                        }
                    }
                }
                catch
                {
                    graphics.Clear(GetDesktopColor());
                }
            }

            return image;
        }

        private static void DrawSpan(Graphics graphics, Image wallpaper, Rectangle screenBounds)
        {
            Rectangle virtualBounds = SystemInformation.VirtualScreen;
            RectangleF destination = GetDestinationRectangle(wallpaper, virtualBounds, WallpaperMode.Fill);
            GraphicsState state = graphics.Save();
            graphics.TranslateTransform(-screenBounds.Left, -screenBounds.Top);
            graphics.DrawImage(wallpaper, destination);
            graphics.Restore(state);
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
