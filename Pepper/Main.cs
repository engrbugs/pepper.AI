using System;
using System.Collections.Generic;
using System.IO;

using System.Windows.Forms;

using System.Threading;

using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using OpenPop.Common.Logging;
using Message = OpenPop.Mime.Message;


using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;

using NLog;
using NLog.Config;
using NLog.Targets;



namespace Pepper
{
    public partial class Main : Form
    {
        //This is for Logs
        FileTarget filetg = new FileTarget();

        NLog.Targets.ConsoleTarget logconsole = new NLog.Targets.ConsoleTarget("logconsole");
        NLog.Config.LoggingConfiguration config = new NLog.Config.LoggingConfiguration();

        private Logger logger = LogManager.GetLogger("Main");

        public string Balloon;

        private pepThread Th = new pepThread();

        public static Main _Main;

        private int TimerCheckMailTick;
        private int TimerCheckGoogleDriveTick;

        //public Variables.ContactDetails scc = new Variables.ContactDetails();

        public Main()
        {
            InitializeComponent();
            _Main = this;

            Variables.Contacts[0].ID = 0;
            Variables.Contacts[0].FirstName = "USER1";
            Variables.Contacts[0].LastName = "LASTNAME";
            Variables.Contacts[0].PetName = "Boss";
            Variables.Contacts[0].Email = "bugs@gmail.com";
            Variables.Contacts[0].MobileNumber = "+1234567890";
            Variables.Contacts[0].Closeness = 100;


            Variables.Contacts[1].ID = 1;
            Variables.Contacts[1].FirstName = "USER2";
            Variables.Contacts[1].LastName = "LASTNAME";
            Variables.Contacts[1].PetName = "Cookie";
            Variables.Contacts[1].Email = "user@gmail.com";
            Variables.Contacts[1].MobileNumber = "+1234567890";
            Variables.Contacts[1].Closeness = 50;

            Variables.Contacts[2].ID = 2;
            Variables.Contacts[2].FirstName = "USER3";
            Variables.Contacts[2].LastName = "LASTNAME";
            Variables.Contacts[2].PetName = "My Princess";
            Variables.Contacts[2].Email = "kylie@gmail.com";
            Variables.Contacts[2].MobileNumber = "+1234567890";
            Variables.Contacts[2].Closeness = 100;

            Variables.Contacts[3].ID = 3;
            Variables.Contacts[3].FirstName = "USER4";
            Variables.Contacts[3].LastName = "LASTNAME";
            Variables.Contacts[3].PetName = "Lord USER";
            Variables.Contacts[3].Email = "zedd@gmail.com";
            Variables.Contacts[3].MobileNumber = "";
            Variables.Contacts[3].Closeness = 100;

           
        }

    
        private void Main_Load(object sender, EventArgs e)
        {
            //NLog Preparations
            filetg.FileName = "${basedir}/logs/pepperlogfile.txt";
            filetg.ArchiveFileName = "${basedir}/logs/peplog.{#}.txt";
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

            logger.Trace("Pepper Start");
            
            
            Th.checkGDfiles();
            Thread.Sleep(5000);

            Th.CheckMail();
            Thread.Sleep(5000);
            Timer.Enabled = true;


        }

        public void ShowBalloonTip(string Message)
        {
            if (Variables.QuietPepper == true) return;
            
            notifyIcon.BalloonTipText = Message;
            notifyIcon.ShowBalloonTip(1000);
        }
       

        
        private void button1_Click(object sender, EventArgs e)
        {
            Th.sendWeatherToday();
        }

        /* #### NO MORE MODULE FOR PEPPER ####
        
            #### NO MORE MODULE FOR PEPPER #### */

        private void TimerCheckMail_Tick(object sender, EventArgs e)
        {
            TimerCheckMailTick++;
            if (TimerCheckMailTick>=Variables.Timer_int_Check_Mail)
            {
                TimerCheckMailTick = 0;
                Th.CheckMail();

            }
            TimerCheckGoogleDriveTick++;
            if (TimerCheckGoogleDriveTick >= Variables.Timer_int_Check_GD)
            {
                TimerCheckGoogleDriveTick = 0;
                Th.checkGDfiles();

            }
            if (Variables.NeedtoRestart==true)
            {
                if (Variables.NeedtoRestartTik == 0)
                {
                    Variables.NeedtoRestart = false;
                    Th.Restart();
                }
                Variables.NeedtoRestartTik--;
            }
            Schedule();
        }
        void Schedule()
        {
            //reset all started modules
            if ((DateTime.Now.TimeOfDay.Hours == 0)
                && (DateTime.Now.TimeOfDay.Minutes == 11)
                && (Variables.RESETstartedmodule == false)) 
            {
                Variables.RESETstartedmodule = true;
                Variables.Modules = new Variables.StartedModule();
            }
            if ((DateTime.Now.TimeOfDay.Hours == 0)
                && (DateTime.Now.TimeOfDay.Minutes == 13)
                && (Variables.RESETstartedmodule == true))
            {
                Variables.RESETstartedmodule = false;
            }

            /* #### DELETED MODULE FOR PEPPER ####
             #### DELETED FOR PEPPER #### */


            //Check Drive Space turn on
            if ((DateTime.Now.TimeOfDay.Hours == Variables.DriveSpaceStart)
                && (Variables.Modules.DriveFreeStarted == false))
            {
                Variables.Modules.DriveFreeStarted = true;
                Th.CheckDriveFreeSpace();
            }

            //Weather Daily Send Email List
            if ((DateTime.Now.TimeOfDay.Hours == Variables.WeatherStarttime)
                && (Variables.Modules.WeatherDailySend == false))
            {
                Variables.Modules.WeatherDailySend = true;
                Th.sendWeatherToday();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (Variables.ShowPepper == true) return;
            Hide();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
