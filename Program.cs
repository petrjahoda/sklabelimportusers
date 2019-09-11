using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace sklabelimportusers {
    class Program {
        private const double InitialDownloadInMilliseconds = 1000;
        private const double DownloadCycleInMilliseconds = 86400000;
        private const string BuildDate = "2019.3.3.10";
        private const string DataFolder = "Logs";
        private const string ConfigFolder = "Config";
        public static bool _osIsLinux;
        public static bool _databaseIsAvailable;
        private static string _customerName;
        public static string _databaseIpAddress;
        public static string _databasePort;
        public static string _databaseName;
        public static string _databaseUsername;
        public static string _databasePassword;
        public static string _databaseType;
        private static string _email;
        private static string _deleteLogFilesAfterDays;
        private static string _smtpClient;
        private static string _smtpPort;
        private static string _smtpUsername;
        private static string _smtpPassword;
        public static string _importDatabaseType;
        public static string _importDatabaseIpAddress;
        public static string _importDatabaseName;
        public static string _importDatabasePort;
        public static string _importDatabaseUsername;
        public static string _importDatabasePassword;
        public static string _importDatabaseUserIdColumn;
        public static string _importDatabaseUserFirstNameColumn;
        public static string _importDatabaseUserSurnameColumn;
        public static string _importDatabaseUserRfidColumn;
        public static string _importDatabaseUserBarcodeColumn;
        public static string _importDatabaseUserPinColumn;
        public static string _importDatabaseTable;
        private static bool _importDatabaseIsAvailable;
        private static bool _systemIsActivated;
        private static bool _processIsRunning;
        private static string _importDatabaseUserTypeColumn;
        private static string _importDatabaseUserEmailColumn;
        private static string _importDatabaseUserPhoneColumn;
        public const string RedColor = "\u001b[31;1m";
        private const string YellowColor = "\u001b[33;1m";
        private const string CyanColor = "\u001b[36;1m";

        static void Main(string[] args) {
            PrintSoftwareLogo();
            var outputPath = CreateLogFileIfNotExists("0-main.txt");
            using (CreateLogger(outputPath, out var logger)) {
                CheckOsPlatform(logger);
                CreateConfigFileIfNotExists(logger);
                LoadSettingsFromConfigFile(logger);
                LogInfo($"[ MAIN ] --INF-- Program version: {BuildDate}", logger);
                var timer = new System.Timers.Timer(InitialDownloadInMilliseconds);
                timer.Elapsed += (sender, e) => {
                    timer.Interval = Convert.ToDouble(DownloadCycleInMilliseconds);
                    RunDevices(logger);
                };
                RunTimer(timer);
            }
        }

        private static void RunDevices(ILogger logger) {
            CheckZapsiDatabaseConnection(logger);
            CheckImportDatabaseConnection(logger);
            DeleteOldLogFiles(logger);
            if (_databaseIsAvailable && _importDatabaseIsAvailable && !_processIsRunning) {
                _processIsRunning = true;
                LogInfo("[ MAIN ] --INF-- Import started", logger);
                var usersToImport = new List<Fask_logins>();
                usersToImport = DownloadUserFromImportDatabase(logger, usersToImport);
                if (usersToImport.Count > 0) {
                    using (var databaseContext = new AppDbContext()) {
                        try {
                            var usersInDatabase = databaseContext.user.ToList();
                            LogInfo(
                                $"[ MAIN ] --INF-- List of {usersInDatabase.Count} users downloaded from actual database",
                                logger);
                            foreach (var userToImport in usersToImport) {
                                var userId = userToImport.ID.ToString();
                                if (!usersInDatabase.Select(user => user.Login).Contains(userId)) {
                                    var userToAdd = new user {
                                        Login = userToImport.ID.ToString(),
                                        FirstName = userToImport.surname,
                                        Name = userToImport.firstname,
                                        Pin = userToImport.psswd,
                                        Barcode = userToImport.barcode,
                                        Rfid = userToImport.rfid,
                                        UserRoleId = 2
                                    };
                                    databaseContext.Add(userToAdd);
                                    LogInfo($"[ MAIN ] --INF-- Added new user: [{userToImport.surname} {userToImport.firstname}], actual size is {usersInDatabase.Count}", logger);
                                }
                            }
                            LogInfo($"[ MAIN ] --INF-- All new users added", logger);
                            char[] charsToTrim = {' '};
                            foreach (var user in usersInDatabase) {
                                foreach (var importedUser in usersToImport) {
                                    if (importedUser.ID.Equals(user.Login)) {
                                        LogInfo(
                                            $"[ MAIN ] --INF-- Updating user: [{user.FirstName} {user.Name}] with Rfid: {importedUser.rfid}, Barcode: {importedUser.barcode}, Pin: {importedUser.psswd}",
                                            logger);
                                        if (importedUser.rfid != null) {
                                            user.Rfid = importedUser.rfid.Trim(charsToTrim);
                                        } else {
                                            if (importedUser.barcode != null) {
                                                user.Rfid = importedUser.barcode.Trim(charsToTrim);
                                            } else {
                                                user.Rfid = importedUser.barcode;
                                            }
                                        }
                                        if (importedUser.psswd != null) {
                                            user.Pin = importedUser.psswd.Trim(charsToTrim);
                                        } else {
                                            user.Pin = importedUser.psswd;
                                        }
                                        if (importedUser.barcode != null) {
                                            user.Barcode = importedUser.barcode.Trim(charsToTrim);
                                        } else {
                                            user.Barcode = importedUser.barcode;
                                        }
                                        databaseContext.SaveChanges();
                                        break;
                                    }
                                }
                            }
                            LogInfo($"[ MAIN ] --INF-- All actual users were updated with actual Rfid data", logger);
                        } catch (Exception error) {
                            LogError(
                                $"[ MAIN ] --INF-- Cannot download list of users from actual database: {error.Message}",
                                logger);
                        }
                    }
                    LogInfo($"[ MAIN ] --INF-- No users to import", logger);
                }
                LogInfo($"[ MAIN ] --INF-- Import ended", logger);
                _processIsRunning = false;
            }
        }

        private static List<Fask_logins> DownloadUserFromImportDatabase(ILogger logger, List<Fask_logins> usersToImport) {
            using (var importDatabaseContext = new AppImportDbContext()) {
                try {
                    usersToImport = importDatabaseContext.UserImport.ToList();
                    LogInfo($"[ MAIN ] --INF-- List of {usersToImport.Count} users downloaded for import", logger);
                } catch (Exception error) {
                    LogError($"[ MAIN ] --INF-- Cannot download list of users for import: {error.Message}", logger);
                }
            }
            return usersToImport;
        }

        private static void CheckImportDatabaseConnection(ILogger logger) {
            using (var databaseContext = new AppImportDbContext()) {
                try {
                    databaseContext.Database.CanConnect();
                    LogInfo("[ MAIN ] --INF-- Helios database is available", logger);
                    _importDatabaseIsAvailable = true;
                } catch (Exception error) {
                    LogError($"[ MAIN ] --ERR-- Helios database is unavailable: {error.Message}", logger);
                    _importDatabaseIsAvailable = false;
                }
            }
        }


        public static void SendEmail(string dataToSend, ILogger logger) {
            if (_smtpClient.Length > 0) {
                ServicePointManager.ServerCertificateValidationCallback = RemoteServerCertificateValidationCallback;
                var client = new SmtpClient(_smtpClient) {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    Port = int.Parse(_smtpPort)
                };
                var mailMessage = new MailMessage {From = new MailAddress(_smtpUsername)};
                mailMessage.To.Add(_email);
                mailMessage.Subject = $"ZAPSI SERVER >> {_customerName}";
                if (dataToSend == null) return;
                mailMessage.Body = dataToSend;
                client.EnableSsl = true;
                try {
                    client.Send(mailMessage);
                    LogInfo($"[ MAIN ] --INF-- Email sent: {dataToSend}", logger);
                } catch (Exception error) {
                    LogError($"[ MAIN ] --ERR-- Cannot send email: {dataToSend}: {error.Message}", logger);
                }
            } else {
                LogError(
                    $"[ MAIN ] --ERR-- Cannot send email: bad SMTP settings: {_smtpClient}, {_smtpPort}, {_smtpUsername}, {_smtpPassword}",
                    logger);
            }
        }

        private static bool RemoteServerCertificateValidationCallback(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors) {
            return true;
        }

        private static bool CheckKey(string name, string key) {
            var keyIsCorrect = false;
            var hash = CreateMd5Hash(name);
            hash = hash.Remove(0, 10);
            hash += "zapsi";
            hash = CreateMd5Hash(hash);
            if (hash.Equals(key)) {
                keyIsCorrect = true;
            }

            return keyIsCorrect;
        }

        private static string CreateMd5Hash(string input) {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var t in hashBytes) {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }


        private static void DeleteOldLogFiles(ILogger logger) {
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), DataFolder);
            try {
                Directory.GetFiles(outputPath)
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < DateTime.Now.AddDays(Convert.ToDouble($"-{_deleteLogFilesAfterDays}")))
                    .ToList()
                    .ForEach(f => f.Delete());
            } catch (Exception error) {
                LogError($"[ MAIN ] --ERR-- Problem deleting old log files: {error.Message}", logger);
            }
        }

        private static void CheckZapsiDatabaseConnection(ILogger logger) {
            using (var databaseContext = new AppDbContext()) {
                try {
                    databaseContext.Database.CanConnect();
                    CreateDatabaseWithTablesIfNotExist(logger);
                    CreateOnlyTablesIfNotExists(logger);
                    LogInfo("[ MAIN ] --INF-- Zapsi database is available", logger);
                    _databaseIsAvailable = true;
                } catch (Exception error) {
                    LogError($"[ MAIN ] --ERR-- Zapsi database is unavailable: {error.Message}", logger);
                    _databaseIsAvailable = false;
                }
            }
        }

        private static void CreateDatabaseWithTablesIfNotExist(ILogger logger) {
            using (var context = new AppDbContext()) {
                var databaseCreator = (RelationalDatabaseCreator) context.Database.GetService<IDatabaseCreator>();
                if (databaseCreator == null) throw new ArgumentNullException(nameof(databaseCreator));
                try {
                    var databaseCreated = context.Database.EnsureCreated();
                    LogInfo(
                        databaseCreated
                            ? $"[ MAIN ] --INF-- Database check: database created/updated: {_databaseName}"
                            : $"[ MAIN ] --INF-- Database check: {_databaseName} database already exists",
                        logger);
                } catch (Exception error) {
                    LogError($"[ MAIN ] --ERR-- Cannot check database: {error.Message}", logger);
                }
            }
        }

        private static void CreateOnlyTablesIfNotExists(ILogger logger) {
            using (var databaseContext = new AppDbContext()) {
                var databaseCreator =
                    (RelationalDatabaseCreator) databaseContext.Database.GetService<IDatabaseCreator>();
                try {
                    databaseCreator.CreateTables();
                    LogInfo("[ MAIN ] --INF-- Tables check: tables in database created", logger);
                } catch (Exception error) {
                    if (error.Message.Contains("Unable")) {
                        LogError($"[ MAIN ] --ERR-- Cannot check tables: {error.Message}", logger);
                    } else {
                        LogInfo("[ MAIN ] --INF-- Tables check: tables in database already exist", logger);
                    }
                }
            }
        }

        private static void RunTimer(System.Timers.Timer timer) {
            timer.Start();
            while (timer.Enabled) {
                Thread.Sleep(Convert.ToInt32(DownloadCycleInMilliseconds));
                const string text = "[ MAIN ] --INF-- Program still running";
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{DateTime.Now} {text}");
            }

            timer.Stop();
            timer.Dispose();
        }

        private static void LoadSettingsFromConfigFile(ILogger logger) {
            try {
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), ConfigFolder))
                    .AddJsonFile("config.json");
                var configuration = configBuilder.Build();
                _databaseType = configuration["databasetype"];
                _databaseIpAddress = configuration["databaseipaddress"];
                _databaseName = configuration["databasename"];
                _databasePort = configuration["databaseport"];
                _databaseUsername = configuration["databaseusername"];
                _databasePassword = configuration["databasepassword"];
                _deleteLogFilesAfterDays = configuration["deletelogfilesafterdays"];
                _customerName = configuration["customername"];
                _email = configuration["email"];
                _smtpClient = configuration["smtpclient"];
                _smtpPort = configuration["smtpport"];
                _smtpUsername = configuration["smtpusername"];
                _smtpPassword = configuration["smtppassword"];
                _importDatabaseType = configuration["importdatabasetype"];
                _importDatabaseIpAddress = configuration["importdatabaseipaddress"];
                _importDatabaseName = configuration["importdatabasename"];
                _importDatabasePort = configuration["importdatabaseport"];
                _importDatabaseUsername = configuration["importdatabaseusername"];
                _importDatabasePassword = configuration["importdatabasepassword"];
                _importDatabaseUserIdColumn = configuration["importdatabaseuseridcolumn"];
                _importDatabaseUserFirstNameColumn = configuration["importdatabaseuserfirstnamecolumn"];
                _importDatabaseUserSurnameColumn = configuration["importdatabaseusersurnamecolumn"];
                _importDatabaseUserRfidColumn = configuration["importdatabaseuserrfidcolumn"];
                _importDatabaseUserBarcodeColumn = configuration["importdatabaseuserbarcodecolumn"];
                _importDatabaseUserPinColumn = configuration["importdatabaseuserpincolumn"];
                _importDatabaseUserTypeColumn = configuration["importdatabaseusertypecolumn"];
                _importDatabaseUserEmailColumn = configuration["importdatabaseuseremailcolumn"];
                _importDatabaseUserPhoneColumn = configuration["importdatabaseuserphonecolumn"];
                _importDatabaseTable = configuration["importdatabasetable"];
                LogInfo($"[ MAIN ] --INF-- Config loaded from file", logger);
            } catch (Exception error) {
                LogError($"[ MAIN ] --ERR-- Cannot load config from file: {error.Message}", logger);
            }
        }

        private static void CreateConfigFileIfNotExists(ILogger logger) {
            var currentDirectory = Directory.GetCurrentDirectory();
            var configDirectory = Path.Combine(currentDirectory, ConfigFolder);
            CreateDirectoryIfNotExists(configDirectory);
            var outputPath = Path.Combine(currentDirectory, ConfigFolder, "config.json");
            var config = new Config();
            if (!File.Exists(outputPath)) {
                var dataToWrite = JsonConvert.SerializeObject(config);
                try {
                    File.WriteAllText(outputPath, dataToWrite);
                    LogInfo("[ MAIN ] --INF-- Config file created.", logger);
                } catch (Exception error) {
                    LogError($"[ MAIN ] --ERR-- Cannot create config or backup file: {error.Message}", logger);
                }
            } else {
                LogInfo("[ MAIN ] --INF-- Config file already exists.", logger);
            }
        }

        private static void CheckOsPlatform(ILogger logger) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                _osIsLinux = true;
                LogInfo("[ MAIN ] --INF-- OS Linux, disable logging to file", logger);
            } else {
                _osIsLinux = false;
            }
        }

        private static void LogInfo(string text, ILogger logger) {
            logger.LogInformation(text);
            if (_osIsLinux) {
                Console.Write(CyanColor);
            } else {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            Console.WriteLine($"{DateTime.Now} {text}");
        }

        private static void LogError(string text, ILogger logger) {
            logger.LogError(text);
            if (_osIsLinux) {
                Console.Write(YellowColor);
            } else {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"{DateTime.Now} {text}");
        }

        private static LoggerFactory CreateLogger(string outputPath, out ILogger logger) {
            var factory = new LoggerFactory();
            logger = factory.CreateLogger("User Import Server EF Core");
            factory.AddFile(outputPath, LogLevel.Debug);
            return factory;
        }

        private static string CreateLogFileIfNotExists(string fileName) {
            var currentDirectory = Directory.GetCurrentDirectory();
            var logFilename = fileName;
            var outputPath = Path.Combine(currentDirectory, DataFolder, logFilename);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            CreateDirectoryIfNotExists(outputDirectory);
            return outputPath;
        }

        private static void CreateDirectoryIfNotExists(string outputDirectory) {
            if (!Directory.Exists(outputDirectory)) {
                try {
                    Directory.CreateDirectory(outputDirectory);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                        Console.Write(CyanColor);
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{DateTime.Now} [ MAIN ] --INF-- Directory created: {outputDirectory}");
                } catch (Exception error) {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                        Console.Write(RedColor);
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"{DateTime.Now} [ MAIN ] --ERR-- Directory not created: {outputDirectory}, {error.Message}");
                }
            }
        }


        private static void PrintSoftwareLogo() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Console.Write(CyanColor);
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("");
            Console.WriteLine(
                "��+   ��+�������+�������+������+     ��+���+   ���+������+  ������+ ������+ ��������+    �������+�������+������+ ��+   ��+�������+������+     �������+�������+     ������+ ������+ ������+ �������+");
            Console.WriteLine(
                "���   �����+----+��+----+��+--��+    �������+ �������+--��+��+---��+��+--��++--��+--+    ��+----+��+----+��+--��+���   �����+----+��+--��+    ��+----+��+----+    ��+----+��+---��+��+--��+��+----+");
            Console.WriteLine(
                "���   ����������+�����+  ������++    �����+����+���������++���   ���������++   ���       �������+�����+  ������++���   ��������+  ������++    �����+  �����+      ���     ���   ���������++�����+  ");
            Console.WriteLine(
                "���   ���+----�����+--+  ��+--��+    ������+��++�����+---+ ���   �����+--��+   ���       +----�����+--+  ��+--��++��+ ��++��+--+  ��+--��+    ��+--+  ��+--+      ���     ���   �����+--��+��+--+  ");
            Console.WriteLine(
                "+������++���������������+���  ���    ������ +-+ ������     +������++���  ���   ���       ���������������+���  ��� +����++ �������+���  ���    �������+���         +������++������++���  ����������+");
            Console.WriteLine(
                " +-----+ +------++------++-+  +-+    +-++-+     +-++-+      +-----+ +-+  +-+   +-+       +------++------++-+  +-+  +---+  +------++-+  +-+    +------++-+          +-----+ +-----+ +-+  +-++------+");
            Console.WriteLine("");
        }
    }
}