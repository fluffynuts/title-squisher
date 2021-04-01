using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PeanutButter.TrayIcon;
using static PeanutButter.RandomGenerators.RandomValueGen;
using Timer = System.Threading.Timer;

namespace TitleSquisher
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var icoStream = TryLoadIcon();
            if (icoStream is null)
            {
                MessageBox.Show("Can't load icon");
                Application.Exit();
                return;
            }

            LoadTitles();

            var icon = new TrayIcon();
            icon.Init(icoStream);
            icon.AddMenuItem("E&xit", Stop);

            icon.Show();

            _timer = new System.Threading.Timer(o => TrySetTotalCommanderTitle(), null, 0, 1000);

            Application.Run();
        }

        private static string[] _titles = new string[0];
        private static Timer _timer;

        private static void LoadTitles()
        {
            var asm = typeof(Program).Assembly;
            var asmFile = new Uri(asm.Location).LocalPath;
            var asmFolder = Path.GetDirectoryName(asmFile);
            var search = Path.Combine(asmFolder, "titles.txt");
            if (File.Exists(search))
            {
                _titles = File.ReadAllLines(search)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .ToArray();
                if (_titles.Length == 0)
                {
                    
                }
            }
        }

        static void Stop()
        {
            _timer.Dispose();
            Application.Exit();
        }

        private static void TrySetTotalCommanderTitle()
        {
            try
            {
                var tcmdProcess = Process.GetProcesses().FirstOrDefault(p =>
                {
                    try
                    {
                        return p.MainModule?.FileName?.ToLowerInvariant()?.Contains("totalcmd") ?? false;
                    }
                    catch
                    {
                        return false;
                    }
                });
                if (tcmdProcess is not null)
                {
                    var hwnd = tcmdProcess.MainWindowHandle;
                    var builder = new StringBuilder();
                    builder.Append($"{GenerateTitle()} {DateTime.Now}");
                    SendMessage(hwnd, 0x000C, IntPtr.Zero, builder);
                }
            }
            catch (Exception ex)
            {
                var foo = ex;
            }
        }

        private static string GenerateTitle()
        {
            return _titles.Length == 0
                ? "Total Commander"
                : GetRandomFrom(_titles);
        }

        private static Stream TryLoadIcon()
        {
            var asm = typeof(Program).Assembly;
            var streams = asm.GetManifestResourceNames();
            var icoStream = typeof(Program).Assembly.GetManifestResourceStream($"{asm.GetName().Name}.icon.ico");
            return icoStream;
        }
    }
}