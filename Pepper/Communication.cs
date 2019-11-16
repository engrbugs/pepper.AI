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
using NLog;



namespace Pepper
{
    class Communication
    {
        private static Dictionary<int, Message> messages = new Dictionary<int, Message>();
        private ManualResetEvent _doneEvent;

        private Logger logger = LogManager.GetLogger("Communication");


        public static void InitializePOP()
        { 
        }
        public Communication(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }
        public void ReceiveMails(Object threadContext)
        {
            logger.Trace("Checking Mail");
            try
            {
                using (Pop3Client client = new Pop3Client())
                {
                    logger.Trace("Connect to Server");
                    client.Connect("pop.gmail.com", 995, true);

                    logger.Trace("Authenticate Username Password");
                    client.Authenticate(Variables.MyEmail, Variables.MyEmailPassword);

                    List<string> Uids = client.GetMessageUids();
                    List<Message> newMessages = new List<Message>();

                    // All the new messages not seen by the POP3 client
                    for (int i = 0; i < Uids.Count; i++)
                    {
                        string currentUidOnServer = Uids[i];
                        if (!Variables.seenUids.Contains(currentUidOnServer))
                        {
                            // We have not seen this message before.
                            // Download it and add this new uid to seen uids

                            // the uids list is in messageNumber order - meaning that the first
                            // uid in the list has messageNumber of 1, and the second has 
                            // messageNumber 2. Therefore we can fetch the message using
                            // i + 1 since messageNumber should be in range [1, messageCount]
                            Message unseenMessage = client.GetMessage(i + 1);

                            // Add the message to the new messages
                            newMessages.Add(unseenMessage);

                            // Add the uid to the seen uids, as it has now been seen
                            Variables.seenUids.Add(currentUidOnServer);
                        }
                    }

                    //PURGE email, since openpop.net and gmail delete option is broken
                    //all email check by pepper automatically deleted and send it to
                    //THRASH on the web, for THIRTY DAYS (30 days) only. --BUGS

                    if (newMessages.Count == 0)
                    {
                        logger.Trace("No Mail");
                        Main._Main.ShowBalloonTip("No mails...");
                    }
                    else
                    {
                        logger.Trace("Got New {0} Mail(s)", newMessages.Count);
                        Main._Main.ShowBalloonTip("I got NEW mail!!!");
                        Array.Resize<ManualResetEvent>(ref Variables.MailReaders, newMessages.Count);
                        for (int i = 0; i < newMessages.Count; i++)
                        {
                            Variables.MailReaders[i] = new ManualResetEvent(false);
                            NewMail NM = new NewMail(Variables.MailReaders[i], newMessages[i]);
                            ThreadPool.QueueUserWorkItem(NM.ReadMail);
                        }
                    }
                    //FileSystem.saveUIDs(Uids); no need  to save also...
                }
            }
            catch (PopServerNotFoundException)
            {
                Main._Main.ShowBalloonTip("No internet!");
                //MessageBox.Show(this, "Error occurred retrieving mail. " + e.Message, "POP3 Retrieval");
            }
            catch (Exception e)
            {
                Main._Main.ShowBalloonTip("Error occurred retrieving mail. " + e.Message);
                //MessageBox.Show(this, "Error occurred retrieving mail. " + e.Message, "POP3 Retrieval");
            }
            finally
            {
                _doneEvent.Set();
            }
        }
        public static string readEmail()
        {
            return "empty";
        }

        #region ##CHEAT SHEET CODES
        /*public static List<Message> FetchUnseenMessages(string hostname, int port, bool useSsl, string username, string password, List<string> seenUids)
http://hpop.sourceforge.net/exampleDownloadUnread.php
// The client disconnects from the server when being disposed
using(Pop3Client client = new Pop3Client())
{
// Connect to the server
client.Connect(hostname, port, useSsl);

// Authenticate ourselves towards the server
client.Authenticate(username, password);

// Fetch all the current uids seen
List<string> uids = client.GetMessageUids();

// Create a list we can return with all new messages
List<Message> newMessages = new List<Message>();

// All the new messages not seen by the POP3 client
for(int i = 0; i<uids.Count; i++)
{
    string currentUidOnServer = uids[i];
    if (!seenUids.Contains(currentUidOnServer))
    {
        // We have not seen this message before.
        // Download it and add this new uid to seen uids

        // the uids list is in messageNumber order - meaning that the first
        // uid in the list has messageNumber of 1, and the second has 
        // messageNumber 2. Therefore we can fetch the message using
        // i + 1 since messageNumber should be in range [1, messageCount]
        Message unseenMessage = client.GetMessage(i + 1);

        // Add the message to the new messages
        newMessages.Add(unseenMessage);

        // Add the uid to the seen uids, as it has now been seen
        seenUids.Add(currentUidOnServer);
    }
}

// Return our new found messages
return newMessages;
}
}
*/

        /*private void ReceiveMails()
        {
            try
            {
                if (pop3Client.Connected)
                    pop3Client.Disconnect();
                pop3Client.Connect("pop.gmail.com", 995, true);
                pop3Client.Authenticate("your@gmail.com", "yourpassword");
                int count = pop3Client.GetMessageCount();
                messages.Clear();
                int success = 0;
                int fail = 0;
                for (int i = count; i >= 1; i -= 1)
                {
                    try
                    {
                        Application.DoEvents();
                        string body;
                        Message message = pop3Client.GetMessage(i);
                        MessagePart plainTextPart = message.FindFirstPlainTextVersion();
                        if (plainTextPart != null)
                        {
                            // The message had a text/plain version - show that one
                            body = plainTextPart.GetBodyAsText();
                        }
                        else
                        {
                            // Try to find a body to show in some of the other text versions
                            List<MessagePart> textVersions = message.FindAllTextVersions();
                            if (textVersions.Count >= 1)
                                body = textVersions[0].GetBodyAsText();
                            else
                                body = "<<OpenPop>> Cannot find a text version body in this message to show <<OpenPop>>";
                        }
                        // Build up the attachment list
                        List<MessagePart> attachments = message.FindAllAttachments();
                        foreach (MessagePart attachment in attachments)
                        { }

                        // Add the message to the dictionary from the messageNumber to the Message
                        messages.Add(i, message);


                        success++;
                    }
                    catch (Exception e)
                    {

                        fail++;
                    }
                }
            }
            catch (InvalidLoginException)
            {
                //MessageBox.Show(this, "The server did not accept the user credentials!", "POP3 Server Authentication");
            }
            catch (PopServerNotFoundException)
            {
                //MessageBox.Show(this, "The server could not be found", "POP3 Retrieval");
            }
            catch (PopServerLockedException)
            {
                //MessageBox.Show(this, "The mailbox is locked. It might be in use or under maintenance. Are you connected elsewhere?", "POP3 Account Locked");
            }
            catch (LoginDelayException)
            {
                //MessageBox.Show(this, "Login not allowed. Server enforces delay between logins. Have you connected recently?", "POP3 Account Login Delay");
            }
            catch (Exception e)
            {
                //MessageBox.Show(this, "Error occurred retrieving mail. " + e.Message, "POP3 Retrieval");
            }
            finally
            {

            }
        }
        */

        #endregion


    }
}
