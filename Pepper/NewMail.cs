using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Net.Sockets;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using OpenPop.Common.Logging;
using Message = OpenPop.Mime.Message;

using System.Text.RegularExpressions;

using NLog;
using System.Net.Mime;

namespace Pepper
{
    class NewMail
    {
        private ManualResetEvent _doneevent;
        private Message _NewMessage;
        private string _newMessageinPlainText;

        //private string _Email;
        //private string _Name;
        //private string _Subject;
        //private string _Body;
        //private bool _isThisNewMessage = false;

        private Logger logger = LogManager.GetLogger("NewMail");

        public NewMail(ManualResetEvent doneevent, Message newMessage)
        {
            _doneevent = doneevent;
            _NewMessage = newMessage;
        }

        public void ReadMail(Object threadContext)
        {
            StringBuilder builder = new StringBuilder();
            MessagePart plaintext = _NewMessage.FindFirstPlainTextVersion();
            string result = "";

            if (plaintext != null)
            {
                // We found some plaintext!
                builder.Append(plaintext.GetBodyAsText());
                result = builder.ToString();
                _newMessageinPlainText = result;
            }
            else
            {
                // Might include a part holding html instead
                MessagePart html = _NewMessage.FindFirstHtmlVersion();
                if (html != null)
                {
                    // We found some html!
                    builder.Append(html.GetBodyAsText());
                    result = StripHTML(builder.ToString());
                    _newMessageinPlainText = result;
                }
            }

            int Who = FromWho(_NewMessage.Headers.From.Address);
            if (Who == -1) //cannot download attachement from UNKNOWN
            {
                ReadingOrderThruEmail(_NewMessage.Headers.From.Address, _NewMessage.Headers.Subject, result);
                _doneevent.Set();
                return;
            }

            // ### DELETED MODULE FOR PEPPER ####

            string replyMSG;
            foreach (MessagePart attachment in _NewMessage.FindAllAttachments())
            {
                //upgrade one at a time, cannot upgrade both bios and pepper in one time.
                switch (attachment.FileName)
                {
                    case Variables.FilenameUpdateBios: //BIOS Update
                        logger.Trace("Got bios Update file.");
                        File.WriteAllBytes(Variables.PathUpdateBios, attachment.Body);
                        pepThread th = new pepThread();
                        //th.UpdateBios(); <----- need to email
                        logger.Debug("Need to restart pepper.");
                        Variables.NeedtoRestart = true;

                        if (Variables.Contacts[Who].Closeness >= 75)
                            replyMSG = "Got the Bios Update " + Variables.Contacts[Who].PetName + "!!!";
                        else
                            replyMSG = "Got the Bios Update " + Variables.Contacts[Who].FirstName + "!!!";

                        //sendEmailHTML(_NewMessage.Headers.From.Address, Variables.Contacts[Who].FirstName + " " + Variables.Contacts[Who].LastName, "Re: BIOS Update", replyMSG);
                        _doneevent.Set();
                        return;

                    //break;
                    case Variables.FilenameUpdatePepper: //UPDATE PEPPER "newme.pep"
                        logger.Debug("Got newme.pep PEPPER UPDATE FILE");
                        File.WriteAllBytes(Variables.PathUpdatePepper, attachment.Body);
                        logger.Debug("Need to restart pepper.");
                        Variables.NeedtoRestart = true;

                        if (Variables.Contacts[Who].Closeness >= 75)
                            replyMSG = "Got the New Dress " + Variables.Contacts[Who].PetName + "!!!";
                        else
                            replyMSG = "Got the New Dress " + Variables.Contacts[Who].FirstName + "!!!";

                        //sendEmailHTML(_NewMessage.Headers.From.Address, Variables.Contacts[Who].FirstName + " " + Variables.Contacts[Who].LastName, "Re: Pepper Upgrade", replyMSG);
                        _doneevent.Set();
                        return;
                        //break;
                }

                /* ### DELETED MODULE FOR PEPPER ####
                 ### DELETED MODULE FOR PEPPER #### */

            }
            /* ### DELETED MODULE FOR PEPPER####
                ### DELETED MODULE FOR PEPPER #### */
            ReadingOrderThruEmail(_NewMessage.Headers.From.Address, _NewMessage.Headers.Subject, result);
            _doneevent.Set();
        }

        public int FromWho(string Email)
        {
            for (int index = 0; index <= Variables.Contacts.Length - 1; index++)
            {
                if (Email == Variables.Contacts[index].Email) return index;
            }
            return -1;
        }

        private string StripHTML(string HTML)
        {

            //remove the <Style> first
            while (HTML.Contains("<style>"))
            {
                int start = HTML.IndexOf("<style>");
                int end = HTML.IndexOf("</style>");
                HTML = HTML.Replace(HTML.Substring(start, end - start + 8), "").Trim(); // 8 = "</style>".length
            }

            //change </div> into new line
            while (HTML.Contains("</div>"))
            {
                HTML = HTML.Replace("</div>", Environment.NewLine).Trim();
            }

            //now, remove all the content inside <>
            while (HTML.Contains('<'))
            {
                int start = HTML.IndexOf('<');
                int end = HTML.IndexOf('>');
                HTML = HTML.Replace(HTML.Substring(start, end - start + 1), "").Trim();
            }

            string[] lines = HTML.Split('\n');
            string result = "";

            foreach (string line in lines)
            {
                result += line.Trim() + Environment.NewLine;
            }

            return result;

        }
        private void ReadingOrderThruEmail(string from, string subject, string body)
        {
            string[] tags = body.Split(' ');
            string replyMSG;
            for (int index = 0; index <= Variables.Contacts.Length - 1; index++)
            {
                if (from == Variables.Contacts[index].Email)
                {
                    //found some contacts

                    /*  ### DELETED MODULE FOR PEPPER
                         #### DELETED MODULE FOR PEPPER #### */
                    foreach (string tag in tags)
                    {
                        bool isBreak = false;

                        switch (tag.Trim().ToLower())
                        {

                            case "hi":
                            case "hello":
                                if (Variables.Contacts[index].Closeness >= 75)
                                    replyMSG = "Hello " + Variables.Contacts[index].PetName + "!!!";
                                else
                                    replyMSG = "Hello " + Variables.Contacts[index].FirstName + "!!!";

                                sendEmailHTML(from,
                                    Variables.Contacts[index].FirstName + " " + Variables.Contacts[index].LastName,
                                    "Re: " + subject,
                                    replyMSG);

                                isBreak = true;
                                return;
                        }
                        if (isBreak == true) break;

                    }
                    break;
                }
                else if (index == Variables.Contacts.Length - 1)
                {
                    //Contacts not found
                    replyMSG = "Hello Stranger! I do not know you, my Dad always said that do not talk to strangers.<br/><br/>If you want to talk to me please email my Boss ADMIN@GMAIL.com to introduce me to you, so we can be friends.<br/><br/>Talking to you soon! ciao!";
                    sendEmailHTML(from,
                        _NewMessage.Headers.From.DisplayName.ToString(),
                        "Re: " + subject,
                        replyMSG);



                }
            }
        }


        public void sendEmailHTML(string Email, string Name, string Subject, string Body, 
            bool isThisNewMessage = false, bool isthisWeather = false, string firstnamepetname = "")
        {
            var fromAddress = new MailAddress(Variables.MyEmail, Variables.MyEmailDisplayName);
            var toAddress = new MailAddress(Email, Name);
            const string fromPassword = Variables.MyEmailPassword;
            string subject = Subject;
            string body;

            if (isThisNewMessage == true)
                body = string.Format("<html><head></head><body><font face=\"Arial\"><font size=\"3\"><p>" 
                     + Body + "<p/>" + Variables.MyEmailSignature + "</p></body></html>");
            else
                body = string.Format("<html><head></head><body><font face=\"Arial\"><font size=\"3\"><p>"
                    + Body + "<p/>" + Variables.MyEmailSignature + "<br/><br/><hr color=#1E90FF>" + "Original Message"
                    + "<br/><b>From: </b>" + Name + "[" + Email + "]"
                    + "<br/><b>Date: </b>" + _NewMessage.Headers.Date
                    + "<br/><p>" + _newMessageinPlainText + "</p></body></html>");

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 3600000

            };

            MailMessage message = new MailMessage();
            message.To.Add(Email);
            message.From = new MailAddress(Variables.MyEmail, Variables.MyEmailDisplayName);
            message.IsBodyHtml = true;
            
            if (isthisWeather == true)
            {
                message.AlternateViews.Add(getEmbeddedImage(Variables.Weathertempfile, firstnamepetname));
                message.Subject = String.Format("Weather for Today-{0} {1}", DateTime.Now.ToString("M/d/yyyy"), DateTime.Now.ToString("HH:mm"));
                message.Body = string.Format("<html><head></head><body><font face=\"Arial\"><font size=\"3\"><p>" +
                @"Good day & God Bless {0}!<br><br>
                <img src='cid:" + @"'/> 
                <br>Enjoy your day!!!<br>
                <br>pepper<br><br>
                <hr>source1:<a href={1}> {1}</a>
                <br>source2:<a href={2}> {2}</a>
                </p></body></html>", firstnamepetname,
                Variables.WeatherSource, Variables.WeatherSource2); //carefull this will not shown in email... this is a just in case scenario
                                                                    //the real one is at the bottom getimagewithweather()
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(Variables.Weathertempfile);
                message.Attachments.Add(attachment);
            }
            else
            {
                message.Subject = subject;
                message.Body = body;
            }
            const int MaxRetries = Variables.NewMailMaxRetries;
            const int MaxDelay = Variables.NewMailWaitforRetry;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    // do stuff
                    smtp.Send(message);
                    break; // jump out of for loop if everything succeeded
                }
                catch (Exception e)
                {
                    logger.Debug(e);
                    Task.Delay(1000);
                    if (i == 10)
                    {
                        i = 0;
                        for (int j = 0; j < MaxDelay; j++)
                        {
                            Task.Delay(1000);
                        }
                    }
                }
            }
            //if (_isThisNewMessage==true) _doneevent.Set();
        }

       
        private AlternateView getEmbeddedImage(string filePath, string firstnamepetname)
        {
            LinkedResource res = new LinkedResource(filePath);
            res.ContentId = Guid.NewGuid().ToString();
            string htmlBody = string.Format("<html><head></head><body><font face=\"Arial\"><font size=\"3\"><p>" +
                string.Format("Good day & God Bless {0}", firstnamepetname).Trim()  + "!" +
                "<br><br><img src='cid:" + res.ContentId + @"'/> 
                <br>Enjoy your day!!!<br>
                <br>pepper<br><br>
                <hr>source1:<a href={0}> {0}</a>
                <br>source2:<a href={1}> {1}</a>
                </p></body></html>",
                Variables.WeatherSource, Variables.WeatherSource2);
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }


    }

    


    /* #### DELETED MODULE FOR PEPPER ####
}
     #### DELETED MODULE FOR PEPPER #### */

}

