using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

using NLog;
using NLog.Targets;

namespace Bios
{
    public partial class Bios : Form
    {
        //This is for Logs
        FileTarget filetg = new FileTarget();
        NLog.Targets.ConsoleTarget logconsole = new NLog.Targets.ConsoleTarget("logconsole");
        NLog.Config.LoggingConfiguration config = new NLog.Config.LoggingConfiguration();
        private Logger logger = LogManager.GetLogger("bios");

        static string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();

        //files
        string PathBridge = Application.StartupPath + @"\bridge.ini";
        string PepperLocation = AppPath + @"\pepper.exe";
        string PathUpdatePepper = AppPath + @"\Workroom\pepper.zip";

        const string VersionBios = "0.99";
        int SplashTime = 8; //s before running pepper and quit

        const Boolean isDebug = false;
        Boolean isSilent = false;

        public Bios()
        {
            bridgeWriter();
            Application.DoEvents();
            
            InitializeComponent();
        }

        private void Bios_Load(object sender, EventArgs e)
        {
            //NLog Preparations
            filetg.FileName = "${basedir}/logs/bioslogfile.txt";
            filetg.ArchiveFileName = "${basedir}/logs/bioslog.{#}.txt";
            filetg.ArchiveEvery = FileArchivePeriod.Day;
            filetg.ArchiveNumbering = ArchiveNumberingMode.Date;
            filetg.MaxArchiveFiles = 7;
            filetg.ConcurrentWrites = true;

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, filetg);

            NLog.LogManager.Configuration = config;
            // Example usage & Logging Level
            //logger.Trace("trace log message");
            //logger.Debug("debug log message");
            //logger.Info("info log message");
            //logger.Warn("warn log message");
            //logger.Error("error log message");
            //logger.Fatal("fatal log message");
            //End Preparation

            logger.Trace("bios start");
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length != 1)
            {
                logger.Trace("Run in silent mode");
                if (args[1] == "-silent") isSilent = true;
            }
            if (!isSilent)
            {
                float winScale = getScalingFactor();
                logger.Debug("getScalingFactor={0}", winScale);
                this.Top = Screen.PrimaryScreen.Bounds.Height - this.Size.Height - 40 - 80; //windows taskbar is 40 pixel
                this.Left = Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width) - Convert.ToInt32(this.Width);

                string Msg;
                logger.Trace("Hour.Now={0}", DateTime.Now.Hour);
                switch (DateTime.Now.Hour)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        Msg = "Hello, I'm pepper.  Good Morning!";
                        break;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        Msg = "Hello, I'm pepper.  Good Afternoon!";
                        break;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 0:
                        Msg = "Hello, I'm pepper.  Good Evening!";
                        break;
                    default:
                        Msg = "Hello, I'm pepper.";
                        break;
                }
                logger.Trace(Msg);
                notifyIcon.BalloonTipText = Msg;
                notifyIcon.ShowBalloonTip(SplashTime * 130); //130 is the perfect ratio in 8 secs to 1000ms in balloon
            }
            else
            {
                logger.Trace("Opacity is 0");
                this.Opacity = 0;
            }

            Pepperupdate();
            Application.DoEvents();

            timer.Enabled = true;
        }
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }
        public float getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%

            //to call
            //MouseMoves mm = new MouseMoves();
            //float winScale;
            // private void Form1_Load(object sender, EventArgs e)
            //{
            //winScale = mm.getScalingFactor();
            //Console.WriteLine(winScale);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            SplashTime--;
            if (SplashTime<=0)
            {
                logger.Debug("isDebug={0}", isDebug);
                if (!isDebug) LaunchPepper();
                System.Windows.Forms.Application.Exit();
            }
        }

        private void LaunchPepper()
        {
            // For the example
            //const string ex1 = "C:\\";
            //const string ex2 = "C:\\Dir";

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.CreateNoWindow = false;
            //startInfo.UseShellExecute = false;
            startInfo.FileName = PepperLocation;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.Arguments = "-f j -o \"" + ex1 + "\" -z 1.0 -s y " + ex2;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    //exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }
        void bridgeWriter()
        {
            logger.Trace("Writing in Bridgewriter");
            string[] data = { VersionBios };
            System.IO.File.WriteAllLines(PathBridge, data);
        }

        void Pepperupdate()
        {
            if (File.Exists(PathUpdatePepper))
            {
                using (ZipArchive archive = ZipFile.OpenRead(PathUpdatePepper))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        try
                        {
                            Application.DoEvents();
                            logger.Debug(entry.FullName);
                            if (entry.FullName.ToString().EndsWith("/")) Directory.CreateDirectory(AppPath + @"\" + entry.FullName.ToString().Replace("/", ""));
                            else entry.ExtractToFile(Path.Combine(AppPath, entry.FullName), true);
                        }
                        catch(Exception e)
                        {
                            logger.Debug(e);
                        }
                     }
                }
                File.Delete(PathUpdatePepper);
                logger.Debug("pepper updated.");
                Label.Text = "Pepper updated.";
            }
        }
    }
}
