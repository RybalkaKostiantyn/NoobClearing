using Ionic.Zip;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Xml;

namespace uStoreInstallerCommonLogic
{
    public enum ApplicationType
    {
        Customer = 1,
        Admin = 2
    }

    public class CommonLogic
    {
        public static class uStore
        {
            private const string XMPie_uStore = @"XMPie uStore";

            private const string APP_CUSTOMERAPP_WEB_CONFIG = @"App\CustomerApp\Web.config";
            private const string APP_ADMINAPP_WEB_CONFIG = @"App\AdminApp\Web.config";
            private const string APP_COMMONAPP_WEB_CONFIG = @"App\Common\uStore.CommonControls\Web.config";

            public static string PathToCustomerAppWebConfig
            {
                get
                {
                    return String.Format(@"{0}\{1}", PathToUStore, APP_CUSTOMERAPP_WEB_CONFIG);
                }
            }
            public static string PathToAdminAppWebConfig
            {
                get
                {
                    return String.Format(@"{0}\{1}", PathToUStore, APP_ADMINAPP_WEB_CONFIG);
                }
            }
            public static string PathToCommonWebConfig
            {
                get
                {
                    return String.Format(@"{0}\{1}", PathToUStore, APP_COMMONAPP_WEB_CONFIG);
                }
            }

            private static string _pathToUStore { get; set; }

            /// <summary>
            /// By default the path is taken from register. Can be redefined
            /// </summary>
            public static string PathToUStore
            {
                get
                {
                    if (_pathToUStore == null)
                    {
                        _pathToUStore = GetInstalledFolderByAppName(XMPie_uStore);
                    }
                    return _pathToUStore;
                }
                set
                {
                    _pathToUStore = value;
                }
            }

            public static void ExtractAppZipFile(string from)
            {
                ExtractZipFile(String.Format(@"{0}\App.zip", from), String.Format(@"{0}", PathToUStore));
            }

            public static void ExtractCustomerAppZipFile(string from)
            {
                ExtractZipFile(String.Format(@"{0}\CustomerApp.zip", from), String.Format(@"{0}\App", PathToUStore));
            }

            public static void ExtractAdminAppZipFile(string from)
            {
                ExtractZipFile(String.Format(@"{0}\AdminApp.zip", from), String.Format(@"{0}\App", PathToUStore));
            }
        }

        public static class uProduce
        {
            private const string XMPIE_UPRODUCE_SERVER = @"XMPie uProduce Server";

            private const string DASHBOARD_WEB_CONFIG = @"XMPieDashboard\Web.config";

            public static string CustomPathToUProduceServer { get; set; }

            public static string PathToDashboardWebConfig
            {
                get
                {
                    return String.Format(@"{0}\{1}", PathToUProduceServer, DASHBOARD_WEB_CONFIG);
                }
            }

            public static string PathToUProduceServer
            {
                get
                {
                    return Directory.Exists(CustomPathToUProduceServer) ?
                        CustomPathToUProduceServer : GetInstalledFolderByAppName(XMPIE_UPRODUCE_SERVER);
                }
            }
        }

        private static string GetInstalledFolderByAppName(string p_name)
        {
            string displayName;
            string installLocation;
            RegistryKey key;

            // search in: CurrentUser
            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue(@"DisplayName") as string;
                    if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        installLocation = subkey.GetValue(@"InstallLocation") as string;
                        return installLocation;
                    }
                }
            }

            // search in: LocalMachine_32
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue(@"DisplayName") as string;
                    if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        installLocation = subkey.GetValue("InstallLocation") as string;
                        return installLocation;
                    }
                }
            }

            // search in: LocalMachine_64
            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key != null)
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue(@"DisplayName") as string;
                    if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        installLocation = subkey.GetValue(@"InstallLocation") as string;
                        return installLocation;
                    }
                }
            }

            // NOT FOUND
            return String.Empty;
        }

        /// <summary>
        /// use this method when you need to extract app.zip from installation folder
        /// </summary>
        /// <param name="zipFile"></param>
        /// <param name="unpackDirectory"></param>
        public static void ExtractZipFile(string zipFile, string unpackDirectory)
        {
            using (var zip = ZipFile.Read(zipFile))
                FileAlreadyExistsFix(zip, unpackDirectory);
        }

        /// <summary>
        /// use this method when you need to extract app.zip from resources
        /// </summary>
        /// <param name="zipBytes"></param>
        /// <param name="unpackDirectory"></param>
        public static void ExtractZipFile(byte[] zipBytes, string unpackDirectory)
        {
            using (var zipStream = new MemoryStream(zipBytes))
            using (var zip = ZipFile.Read(zipStream))
                FileAlreadyExistsFix(zip, unpackDirectory);
        }

        private static void FileAlreadyExistsFix(ZipFile zip, string unpackDirectory)
        {
            foreach (var e in zip)
            {
                Action extract = () =>
                {
                    e.Extract(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
                };

                try
                {
                    extract();
                }
                catch (IOException)
                {
                    // Sometimes if a previous extraction left .tmp file
                    // Then the next one can't extract it again 

                    var fi = new FileInfo(Path.Combine(unpackDirectory, e.FileName));
                    var fitmp = new FileInfo(fi.FullName + ".tmp");

                    if (fi.Exists)
                        File.Delete(fi.FullName);

                    if (fitmp.Exists)
                        File.Delete(fitmp.FullName);

                    extract();
                }
            }
        }

        public static void WriteEventToWindowsLog(string strMyApp, string strEvent, EventLogEntryType eventLogEntryType)
        {
            if (!EventLog.SourceExists(strMyApp))
                EventLog.CreateEventSource(strMyApp, "Application");

            EventLog MyEventLog = new EventLog();
            MyEventLog.Source = strMyApp;
            MyEventLog.WriteEntry(strEvent, eventLogEntryType);
        }

        public static string WriteEventToWindowsLog(Exception exc, string appName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("InnerException: {0}", exc.InnerException).AppendLine();
            sb.AppendFormat("Message: {0}", exc.Message).AppendLine();
            sb.AppendFormat("Source: {0}", exc.Source).AppendLine();
            sb.AppendFormat("StackTrace: {0}", exc.StackTrace).AppendLine();

            WriteEventToWindowsLog(appName, sb.ToString(), EventLogEntryType.Error);

            return sb.ToString();

        }

        public static void CreateShareFolder(string FolderPath, string ShareName, string Description)
        {
            #region Create folder if not exists

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            #endregion

            #region Set Access Control

            DirectoryInfo dInfo = new DirectoryInfo(FolderPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);

            #endregion

            #region Sharing folder

            ManagementClass mc = new ManagementClass("win32_share");
            ManagementBaseObject inParams = mc.GetMethodParameters("Create");
            inParams["Description"] = Description;
            inParams["Name"] = ShareName;
            inParams["Path"] = FolderPath;
            inParams["Type"] = 0x0;
            inParams["MaximumAllowed"] = null;
            inParams["Password"] = null;
            inParams["Access"] = null; // Make Everyone has full control access.
            ManagementBaseObject outParams = mc.InvokeMethod("Create", inParams, null);

            #endregion

            //// Only in Windows 7 and Vista, upgrade "Everyone" sharing right

            ////user selection
            //NTAccount ntAccount = new NTAccount("Everyone");

            ////SID
            //SecurityIdentifier userSID = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
            //byte[] utenteSIDArray = new byte[userSID.BinaryLength];
            //userSID.GetBinaryForm(utenteSIDArray, 0);

            ////Trustee
            //ManagementObject userTrustee = new ManagementClass(new ManagementPath("Win32_Trustee"), null);
            //userTrustee["Name"] = "Everyone";
            //userTrustee["SID"] = utenteSIDArray;

            ////ACE
            //ManagementObject userACE = new ManagementClass(new ManagementPath("Win32_Ace"), null);
            //userACE["AccessMask"] = 2032127;                                 //Full access
            //userACE["AceFlags"] = AceFlags.ObjectInherit | AceFlags.ContainerInherit;
            //userACE["AceType"] = AceType.AccessAllowed;
            //userACE["Trustee"] = userTrustee;

            //ManagementObject userSecurityDescriptor = new ManagementClass(new ManagementPath("Win32_SecurityDescriptor"), null);
            //userSecurityDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT 
            //userSecurityDescriptor["DACL"] = new object[] { userACE };

            ////UPGRADE SECURITY PERMISSION
            //ManagementClass mc2 = new ManagementClass("Win32_Share");
            //ManagementObject share = new ManagementObject(mc2.Path + ".Name='" + ShareName + "'");
            //share.InvokeMethod("SetShareInfo", new object[] { Int32.MaxValue, Description, userSecurityDescriptor });
        }

        /// <summary>
        /// Insert resources into Resource and ResourcesCulture tables of uStore db
        /// </summary>
        /// <param name="cultures"></param>
        /// <param name="applicationTypeID"></param>
        /// <param name="prefixStringID"></param>
        /// <param name="uStoreConnectionString"></param>
        public static void uStoreInsertResource(Dictionary<string, string> cultures, ApplicationType applicationTypeID, string prefixStringID, string uStoreConnectionString)
        {
            var elements = new Dictionary<string, XmlNodeList>();
            foreach (var cult in cultures)
            {
                var doc = new XmlDocument();
                doc.LoadXml(cult.Value);
                elements.Add(cult.Key, doc.GetElementsByTagName("string"));
            }

            Func<string, string> rmvCDATA = (v) =>
            {
                if (v == null)
                    return v;

                return v.Replace("<![CDATA[", "").Replace("]]>", "");
            };

            Func<string, string, string> value = (culture, id) =>
            {
                if (elements.ContainsKey(culture))
                {
                    foreach (XmlElement el in elements[culture])
                    {
                        if (el.GetAttribute("id") == id)
                        {
                            return rmvCDATA(el.InnerXml.Trim());
                        }
                    }
                }
                return "[no localization]";
            };

            var queries = new List<string>();
            foreach (XmlElement el in elements["en_US"])
            {
                var id = el.GetAttribute("id").Trim();
                queries.Add(String.Format(Resource.uStoreInsertResource,
                    (int)applicationTypeID,
                    String.Format("{0}_{1}", prefixStringID, id),
                    rmvCDATA(el.InnerXml.Trim()),
                    value("fr_FR", id),
                    value("de_DE", id),
                    value("ja_JP", id),
                    value("es_ES", id),
                    value("nl_NL", id),
                    value("pt_BR", id),
                    value("en_GB", id)));
            }

            DbQueries dbQueriesUStore = DbCommand.CreateDbQueries(uStoreConnectionString, queries.ToArray());
            List<DbQueries> dbQueriesList = new List<DbQueries> { dbQueriesUStore };
            DbCommand.ExecuteTransaction(dbQueriesList);
        }

        public static bool WriteRegistryKey(string application, string KeyName, object Value)
        {
            RegistryKey sk1 = Registry.LocalMachine.CreateSubKey(String.Format(@"SOFTWARE\\{0}", application));
            sk1.SetValue(KeyName.ToUpper(), Value);
            return true;
        }

        public static string ReadRegistryKey(string application, string KeyName)
        {
            RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(String.Format(@"SOFTWARE\\{0}", application));
            if (sk1 == null)
                return null;
            return (string)sk1.GetValue(KeyName.ToUpper());
        }

        public static bool DeleteRegistryKey(string application, string KeyName)
        {
            RegistryKey sk1 = Registry.LocalMachine.CreateSubKey(String.Format(@"SOFTWARE\\{0}", application));
            if (sk1 == null)
                return true;
            sk1.DeleteValue(KeyName);

            return true;
        }

        public static void BinarySerialize(string path, object obj)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, obj);
            }
            catch (SerializationException)
            {
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public static object BinaryDeserialize(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(fs);
            }
            catch (SerializationException)
            {
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public static DataRow[] GetSqlServers()
        {
            DataTable dt = SmoApplication.EnumAvailableSqlServers(false);
            DataRow[] rows = dt.Select(string.Empty, "IsLocal desc, Name asc");
            return rows;
        }
    }
}