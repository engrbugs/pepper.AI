using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Threading;

namespace Pepper
{
    class DriveSpace
    {
        private NewMail NM = new NewMail(null, null);
        private ManualResetEvent _doneEvent;

        public DriveSpace(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }
        public void CheckDriveforSystemSpace(Object threadContext)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                //Console.WriteLine(d.Name.ToUpper().Trim());
                //Console.WriteLine("BINGO");
                if (d.IsReady == true)
                {
                    //Console.WriteLine(d.TotalFreeSpace);
                    //Console.WriteLine(d.TotalSize);
                    double ratio = Convert.ToDouble(d.TotalFreeSpace) / Convert.ToDouble(d.TotalSize);
                    //Console.WriteLine(ratio);
                    if (ratio < Variables.DriveSpaceFreePercent - 0.02)  //0.01 is a gap
                    {
                        //give us a warning...
                        foreach (string email in GetEmailList())
                        {

                            int Who = NM.FromWho(email);
                            if (Who == -1)
                            {
                                string MSG;
                                MSG = string.Format("{0}, the drive {1} is now {2} availabe space. You need at least {3}MB free space. Please remove some or transfer to another drive. {0}. I'll check it again tomorrow.",
                                        "Nobody!", "I dont Know", ratio.ToString("P", (CultureInfo.InvariantCulture)), (d.TotalSize * Variables.DriveSpaceFreePercent / 1000000000).ToString("F", CultureInfo.InvariantCulture));

                                NM.sendEmailHTML(email, "Nobody", "HD Free Space limit", MSG, true);
                            }
                            else
                            {
                                string MSG;
                                if (Variables.Contacts[Who].Closeness >= 75)
                                    MSG = string.Format("{0}, the drive {1} is now {2} availabe space. You need at least {3}MB free space. Please remove some or transfer to another drive. {0}. I'll check it again tomorrow.",
                                        Variables.Contacts[Who].PetName, d.Name.ToUpper().Trim(), ratio.ToString("P", (CultureInfo.InvariantCulture)), (d.TotalSize * Variables.DriveSpaceFreePercent / 1000000000).ToString("F", CultureInfo.InvariantCulture));
                                else
                                    MSG = string.Format("{0}, the drive {1} is now {2} availabe space. You need at least {3}MB free space. Please remove some or transfer to another drive. {0}. I'll check it again tomorrow.",
                                        Variables.Contacts[Who].FirstName, d.Name.ToUpper().Trim(), ratio.ToString("P", (CultureInfo.InvariantCulture)), (d.TotalSize * Variables.DriveSpaceFreePercent / 1000000000).ToString("F", CultureInfo.InvariantCulture));

                                NM.sendEmailHTML(Variables.Contacts[Who].Email, Variables.Contacts[Who].FirstName + " " + Variables.Contacts[Who].LastName, "HD Free Space limit", MSG, true);
                            }
                            
                        }
                    }
                }
                _doneEvent.Set();
            }

        }
        private string[] GetEmailList()
        {
            List<string> myCollection = new List<string>();

            if (Variables.DriveSpace_emaillist == "") return null;
            //if (Variables.DriveSpace_emaillist.Contains(","))
            

            else if (!Variables.DriveSpace_emaillist.Contains(",")) //only 1 email on the list
            {
                //Console.WriteLine(Variables.DriveSpace_emaillist.Trim());
                myCollection.Add(Variables.DriveSpace_emaillist.Trim());
            }
            else
            {
                int s = 0;
                int e = Variables.DriveSpace_emaillist.IndexOf(",", s);
                while (s < e) 
                {
                    //Console.WriteLine(Variables.DriveSpace_emaillist.Substring(s, e - s).Trim());
                    myCollection.Add(Variables.DriveSpace_emaillist.Substring(s, e - s).Trim());
                    s = e + 1;
                    e = Variables.DriveSpace_emaillist.IndexOf(",", s);
                    //Console.WriteLine(e);
                    if (e == -1) //this is the end
                    {
                        //Console.WriteLine(Variables.DriveSpace_emaillist.Substring(s, Variables.DriveSpace_emaillist.Length - s).Trim());
                        myCollection.Add(Variables.DriveSpace_emaillist.Substring(s, Variables.DriveSpace_emaillist.Length - s).Trim());
                        break;
                    }
                }
            }

            return myCollection.ToArray();

        }
    }
}
