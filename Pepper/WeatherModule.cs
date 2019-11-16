using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using NLog;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Pepper
{
    class WeatherModule
    {
        private Logger logger = LogManager.GetLogger("Weather");
        static private ManualResetEvent _doneEvent;
        private NewMail NM = new NewMail(null, null);
        public WeatherModule(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }
        public void SendWeatherDaily(Object threadContext)
        {
            downloadweather();
            //Main._Main.Timer.Enabled = false;

            //Variables.MailMan.WaitOne();
            foreach (string email in GetWeatherEmailList())
            {
                int Who = NM.FromWho(email);
                if (Who == -1)
                {
                    NM.sendEmailHTML(email,
                    "Nobody",
                    "", "", true, true, "");
                }
                else
                {
                    string petorfirstname;
                    if (Variables.Contacts[Who].Closeness >= 75)
                        petorfirstname = Variables.Contacts[Who].PetName;
                    else
                        petorfirstname = Variables.Contacts[Who].FirstName;

                    NM.sendEmailHTML(Variables.Contacts[Who].Email,
                        Variables.Contacts[Who].FirstName + " " + Variables.Contacts[Who].LastName,
                        "", "", true, true, petorfirstname);
                }
                
            }
            _doneEvent.Set();

        }
        public void downloadweather()
        {
            DirectoryInfo di = Directory.CreateDirectory(Variables.WeatherFolder);

            string remoteUri = Variables.WeatherSource;

            string fileName = Variables.Weathertempfile;
            WebClient myWebClient = new WebClient();

            int fivesecondtries = 5;
            int minutetries = 5;
            int tenminutetries = 5;
            bool done = false;
            while (!done)
            {
                try
                {
                    myWebClient.DownloadFile(remoteUri, fileName);
                    logger.Trace("Successfully Downloaded File {0}", fileName);
                    done = true;
                }
                catch
                {
                    logger.Debug("Error Downloading Weather File");
                    if (fivesecondtries > 0)
                    {
                        Thread.Sleep(5000);
                        fivesecondtries--;
                    }
                    else if (minutetries > 0)
                    {
                        Thread.Sleep(12 * 5000);
                        minutetries--;
                    }
                    else if (tenminutetries > 0)
                    {
                        Thread.Sleep(10 * 12 * 5000);
                        tenminutetries--;
                    }
                    else
                    {
                        Thread.Sleep(3600000);//an hour
                    }
                }
            }
        }
        private string[] GetWeatherEmailList()
        {
            List<string> myCollection = new List<string>();

            if (Variables.Weather_emaillist == "") return null;

            else if (!Variables.Weather_emaillist.Contains(",")) //only 1 email on the list
            {
                myCollection.Add(Variables.Weather_emaillist.Trim());
            }
            else
            {
                int s = 0;
                int e = Variables.Weather_emaillist.IndexOf(",", s);
                while (s < e)
                {
                    myCollection.Add(Variables.Weather_emaillist.Substring(s, e - s).Trim());
                    s = e + 1;
                    e = Variables.Weather_emaillist.IndexOf(",", s);
                    if (e == -1) //this is the end
                    {
                        myCollection.Add(Variables.Weather_emaillist.Substring(s, Variables.Weather_emaillist.Length - s).Trim());
                        break;
                    }
                }
            }
            return myCollection.ToArray();
        }
    }
}
