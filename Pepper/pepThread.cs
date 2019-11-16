using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;
using NLog;

namespace Pepper
{
    class pepThread
    {
        //static ManualResetEvent[] doneEvents;

        //List of THREADS reset events

        private Logger logger = LogManager.GetLogger("Thread");

        public void sendWeatherToday()
        {
            logger.Trace("Sending Weather");
            //CHECK if Thread Mailman is done checking mail
            if (Variables.WeatherThread.WaitOne(0) == false) return;

            Variables.WeatherThread = new ManualResetEvent(false);
            WeatherModule w = new WeatherModule(Variables.WeatherThread);
            ThreadPool.QueueUserWorkItem(w.SendWeatherDaily);
        }

        public void checkGDfiles()
        {
            logger.Trace("CheckMail");
            //CHECK if Thread Mailman is done checking mail
            if (Variables.GDthread.WaitOne(0) == false) return;

            Variables.GDthread = new ManualResetEvent(false);
            GoogleDrive g = new GoogleDrive(Variables.GDthread);
            ThreadPool.QueueUserWorkItem(g.checkContent);
        }



        public void CheckMail()
        {
            logger.Trace("CheckMail");
            //CHECK if Thread Mailman is done checking mail
            if (Variables.MailMan.WaitOne(0)==false) return;
            
            Variables.MailMan = new ManualResetEvent(false);
            Communication c = new Communication(Variables.MailMan);
            ThreadPool.QueueUserWorkItem(c.ReceiveMails);
        }

        public void CheckDriveFreeSpace()
        {
            //CHECK if Thread Updater is done updating
            if (Variables.DriveFreeSpaceCheck.WaitOne(0) == false) return;

            Variables.DriveFreeSpaceCheck = new ManualResetEvent(false);
            DriveSpace c = new DriveSpace(Variables.DriveFreeSpaceCheck);
            ThreadPool.QueueUserWorkItem(c.CheckDriveforSystemSpace);
        }



        public void Restart(bool silentarg = true)
        {
            logger.Debug("Restarting pepper...");
            Main._Main.ShowBalloonTip("Restarting pepper...");

            //exiting threads
            Main._Main.Timer.Enabled = false;
            
            Variables.MailMan.WaitOne();
            if (Variables.MailReaders != null) WaitHandle.WaitAll(Variables.MailReaders);
            Variables.Updater.WaitOne();

            System.Threading.Thread.Sleep(Variables.WaitBeforeExit);

            if (silentarg==true)
                LaunchExe(Variables.PathBIOS, "-silent");
            else
                LaunchExe(Variables.PathBIOS, "");

            System.Windows.Forms.Application.Exit();
        }

        public void LaunchExe(string Filename, string Arguments)
        {
            logger.Trace("EXE launch {0} {1}", Filename, Arguments);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.CreateNoWindow = false;
            //startInfo.UseShellExecute = false;
            startInfo.FileName = Filename;

            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (Arguments != "") startInfo.Arguments = Arguments; //" - f j -o \"" + ex1 + "\" -z 1.0 -s y " + ex2;

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
    }
}
