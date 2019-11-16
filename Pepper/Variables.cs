using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Pepper
{
    class Variables
    {
        //version
        public const string VersionPepper = "1.01";
        public const string BOSSadminEMAIL = "admin@gmail.com";

        //email and password
        public const string MyEmail = "PEPPER_EMAIL@GMAIL.com";
        public const string MyEmailPassword = "PEPPER PASSWORD";
        public const string MyEmailDisplayName = "pepper";
        public const string MyEmailSignature = "<br/>pepper";
        public const int NewMailMaxRetries = 10; //10 tries send email;
        public const int NewMailWaitforRetry = 600; //(seconds) after 10 tries wait for 10 mins 600 seconds



        //pepper tooltips
        public const string MAX_THREAD_REACHED_string = "I'm tired. Need some shower. Be right back! (Max thread reached.)";

        public static bool NeedtoRestart = false;
        public static int NeedtoRestartTik = 5; //1 tik = 1 checkmail
        
        //data
        public static int Timer_int_Check_Mail = 15; //seconds (defualt: 15s)
        public static int Timer_int_Check_GD = 20; //seconds (Default: 20s)
        public static bool QuietPepper = true; //no sound and notifications
        public static bool ShowPepper = false;
        public const int WaitBeforeExit = 2000; //1000=1 sec
        


        public static int MAX_EMAIL_COUNT = 1000; // (default 1000)
        //PURGE email, since openpop.net and gmail delete option is broken
        //all email check by pepper automatically deleted and send it to
        //THRASH on the web, for THIRTY DAYS (30 days) only.

        public static List<string> seenUids = new List<string>();

        //files
        public static string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString();

        public static string PathBIOS = AppPath + @"\bios.exe";
        public static string PathPepper = AppPath + @"\pepper.exe";

        public const string FilenameUpdateBios = "bios.zip";
        public static string PathUpdateBios = AppPath + @"\Workroom\" + FilenameUpdateBios;
        public const string FilenameUpdatePepper = "pepper.zip";
        public static string PathUpdatePepper = AppPath + @"\Workroom\" + FilenameUpdatePepper;

        public static string PathNotes = AppPath + @"\Notes";
        //public static string PathUID = AppPath + @"\Notes\Uids.ini";

        //threads
        public static ManualResetEvent MailMan = new ManualResetEvent(true);
        public static ManualResetEvent[] MailReaders;
        public static ManualResetEvent Updater = new ManualResetEvent(true);
        public static ManualResetEvent DriveFreeSpaceCheck = new ManualResetEvent(true);
        public static ManualResetEvent GDthread = new ManualResetEvent(true);
        public static ManualResetEvent WeatherThread = new ManualResetEvent(true);



        //contacts
        public struct ContactDetails
        {
            public int ID;
            public string FirstName;
            public string LastName;
            public string PetName;
            public string Email;
            public string MobileNumber;
            public short Closeness;


        }
        public static ContactDetails[] Contacts = new ContactDetails[5];

        public static bool RESETstartedmodule = false;

        public struct StartedModule
        {
            //### DELETED MODULE FOR PEPPER #####

            public bool DriveFreeStarted;
            /*### DELETED MODULE FOR PEPPER #####
            ### DELETED MODULE FOR PEPPER #####*/
            public bool WeatherDailySend;
        }
        public static StartedModule Modules;

        //module #1 DELETED MODULE
        /*  ### DELETED MODULE FOR PEPPER #####
        ### DELETED MODULE FOR PEPPER #####   */

        //module #2 checking drive space
        public static string DriveSpace_emaillist = "admin@gmail.com"; //"," as seperator
        public static int DriveSpaceStart = 8; //defualt value 8am
        public const double DriveSpaceFreePercent = 0.15; //default value is 15%

        //module #3 Weather Email
        public const string Weather_emaillist = "admin@gmail.com, user2@gmail.com"; //"," as seperator
        public const int WeatherStarttime = 6; //defualt value 6am
        public static string WeatherFolder = AppPath + @"\Modules\Weather\";
        public static string Weathertempfile = WeatherFolder + "TodaysWeather.gif";
        public const string WeatherSource = @"http://121.58.193.148/repo/himawari/24hour/irbig/latestHIM_irbig.gif";
        public const string WeatherSource2 = @"http://121.58.193.148/repo/fy2e-ir/24hour/latest_CROPfy.gif";
        //public static string WeatherBODY = string.Format(@"Good day & God Bless {0}!<br><br>

    }
}
