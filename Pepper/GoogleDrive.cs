using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Download;
using System.IO;
using System.Threading;

using NLog;
using NLog.Config;
using NLog.Targets;

using System.IO.Compression;

namespace Pepper
{
    class GoogleDrive
    {
        static private ManualResetEvent _doneEvent;
        static private Logger logger = LogManager.GetLogger("GoogleDrive");
        private NewMail NM = new NewMail(null, null);


        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "pepper";

        // The file token.json stores the user's access and refresh tokens, and is created
        // automatically when the authorization flow completes for the first time.
        static string credPath = "token.json";
        static FileStream stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);

        static UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
        // Create Drive API service.
        static DriveService service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        static IList<Google.Apis.Drive.v3.Data.File> files;
        static string GDriveToken;

        public GoogleDrive(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }

        public void checkContent(Object threadContext)
        {
            string token;
            try
            {
                token = service.Changes.GetStartPageToken().Execute().StartPageTokenValue;
                logger.Debug("Google Drive Current Token:{0}", token);
            }
            catch
            {
                return;
            }
            if (token != GDriveToken)
            {
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 10;
                listRequest.Fields = "nextPageToken, files(id, name)";

                // List files.
                files = listRequest.Execute().Files;

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        logger.Debug("File: {0} {1}", file.Name, file.Id);
                        switch (file.Name)
                        {
                            case (Variables.FilenameUpdateBios): //download & update Bios
                                //download bios update
                                //send an email to boss that recieved an update
                                //extract the update
                                //need to restart
                                DownloadFile(file.Id, Variables.PathUpdateBios);
                                logger.Trace("Got bios Update file.");
                                string MSGbio = "Got the Bios Update " + Variables.Contacts[0].PetName + "!!!"; //<-- First contact is the BOSS!
                                NM.sendEmailHTML(Variables.Contacts[0].Email, 
                                    Variables.Contacts[0].FirstName + " " + Variables.Contacts[0].LastName, 
                                    "New Bios Update", 
                                    MSGbio, 
                                    true);
                                BIOSupdate();
                                Variables.NeedtoRestart = true;
                                
                                break;
                            case (Variables.FilenameUpdatePepper):
                                //download pepper update only
                                //send an email to boss that recieved an update
                                //need to restart
                                DownloadFile(file.Id, Variables.PathUpdatePepper);
                                logger.Trace("Got pepper update.");
                                string MSG = "Got the New Dress " + Variables.Contacts[0].PetName + "!!!"; //<-- First contact is the BOSS!

                                NM.sendEmailHTML(Variables.Contacts[0].Email, 
                                    Variables.Contacts[0].FirstName + " " + Variables.Contacts[0].LastName, 
                                    "New Pepper Update", 
                                    MSG, 
                                    true);

                                Variables.NeedtoRestart = true;
                                break;
                        }
                    }
                    //delete everything.
                    deleteAllfileinGD();
                }
                else
                {
                    logger.Debug("No files found.");
                }
            }
            
            GDriveToken = service.Changes.GetStartPageToken().Execute().StartPageTokenValue;
            _doneEvent.Set();
        }
        private void DownloadFile(string fileId, string dest)
        {
            logger.Debug("file downloading...{0}", fileId);

            Google.Apis.Drive.v3.FilesResource.GetRequest request = service.Files.Get(fileId);
            var stream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
                (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                logger.Debug(progress.BytesDownloaded);
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                logger.Debug("Download complete.");
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                logger.Debug("Download failed.");
                                break;
                            }
                    }
                };
            request.Download(stream);
            using (var fileStream = System.IO.File.Create(dest))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
            logger.Debug("file downloaded:{0}", dest);
        }
        private void deleteAllfileinGD()
        {
            logger.Trace("Deleting Everything in Google Drive.");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    service.Files.Delete(file.Id).Execute();

                    logger.Debug("File Deleted: {0} {1}", file.Name, file.Id);
                }
            }
            else
            {
                logger.Warn("!!!No files found!!!");
            }
        }
        public void BIOSupdate()
        {
            if (System.IO.File.Exists(Variables.PathUpdateBios))
            {
                using (ZipArchive archive = ZipFile.OpenRead(Variables.PathUpdateBios))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        try
                        {
                            entry.ExtractToFile(Path.Combine(Variables.AppPath, entry.FullName), true);
                        }
                        catch(Exception e)
                        {
                            logger.Debug(e);
                        }
                    }
                }
                System.IO.File.Delete(Variables.PathUpdateBios);
                Main._Main.ShowBalloonTip("Bios Updated.");
                logger.Trace("BIOS UPDATED");
                //need to  restart
                Variables.NeedtoRestart = true;
            }
        }

    }
}
