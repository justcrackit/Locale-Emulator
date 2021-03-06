﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace LEUpdater
{
    internal static class ApplicationUpdater
    {
        private static string url = string.Empty;

        internal static void CheckApplicationUpdate(string version, NotifyIcon notifyIcon)
        {
            notifyIcon.ShowBalloonTip(0,
                                      "Locale Emulator V" + Application.ProductVersion,
                                      "Checking for application updates ...",
                                      ToolTipIcon.Info);

            string url = string.Format(@"http://service.watashi.me/le/check.php?ver={0}&lang={1}",
                                       version,
                                       CultureInfo.CurrentUICulture.LCID);

            try
            {
                var webFileUri = new Uri(url);
                WebRequest webRequest = WebRequest.Create(webFileUri);
                webRequest.Timeout = 10 * 1000;

                WebResponse response = webRequest.GetResponse();
                var xmlContent = new XmlDocument();
                xmlContent.Load(response.GetResponseStream());

                ProcessUpdate(xmlContent, notifyIcon);
            }
            catch (Exception ex)
            {
                notifyIcon.ShowBalloonTip(0,
                                          "Locale Emulator V" + Application.ProductVersion,
                                          "Error occurs when checking new application version: \r\n" + ex.Message,
                                          ToolTipIcon.Error);

                Thread.Sleep(5000);
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        private static void ProcessUpdate(XmlDocument xmlContent, NotifyIcon notifyIcon)
        {
            string newVer = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;

            if (CompareVersion(Application.ProductVersion, newVer))
            {
                try
                {
                    string version = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;
                    string data = xmlContent.SelectSingleNode(@"/VersionInfo/Date/text()").Value;
                    url = xmlContent.SelectSingleNode(@"/VersionInfo/Url/text()").Value;
                    string note = xmlContent.SelectSingleNode(@"/VersionInfo/Note/text()").Value;

                    notifyIcon.BalloonTipClicked += notifyIcon_BalloonTipClicked;

                    notifyIcon.ShowBalloonTip(0,
                                              "New version available",
                                              String.Format("Current version: {0}\r\n" +
                                                            "New version: {1}\r\n" +
                                                            "Released on: {2}\r\n" +
                                                            "\r\n" +
                                                            "Please click on me to go to the download page.",
                                                            Application.ProductVersion,
                                                            version,
                                                            data),
                                              ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    notifyIcon.ShowBalloonTip(0,
                                              "Locale Emulator V" + Application.ProductVersion,
                                              "Error occurs when checking new application version: \r\n" + ex.Message,
                                              ToolTipIcon.Error);

                    Thread.Sleep(5000);
                    notifyIcon.Visible = false;
                    Environment.Exit(0);
                }
            }
            else
            {
                notifyIcon.ShowBalloonTip(0,
                                          "Locale Emulator V" + Application.ProductVersion,
                                          "You are using the lastest version :)",
                                          ToolTipIcon.Info);

                Thread.Sleep(5000);
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        private static void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            Process.Start(url);

            var notifyIcon = (NotifyIcon)sender;

            notifyIcon.Visible = false;
            Environment.Exit(0);
        }

        /// <summary>
        ///     If ver2 is bigger than ver1, return true.
        /// </summary>
        /// <param name="oldVer"></param>
        /// <param name="newVer"></param>
        /// <returns></returns>
        private static bool CompareVersion(string oldVer, string newVer)
        {
            var versionOld = new Version(oldVer);
            var versionNew = new Version(newVer);

            return versionOld < versionNew;
        }
    }
}