namespace linaplastimportusers {
    public class Config {
        public string DatabaseType { get; set; }
        public string DatabaseIpAddress { get; set; }
        public string DatabaseName { get; set; }
        public string DatabasePort { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string CustomerName { get; set; }
        public string DeleteLogFilesAfterDays { get; set; }
        public string Email { get; set; }
        public string SmtpClient { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string ImportDatabaseType { get; set; }
        public string ImportDatabaseIpAddress { get; set; }
        public string ImportDatabaseName { get; set; }
        public string ImportDatabasePort { get; set; }
        public string ImportDatabaseUsername { get; set; }
        public string ImportDatabasePassword { get; set; }
        public string ImportDatabaseUserIdColumn { get; set; }
        public string ImportDatabaseUserFirstNameColumn { get; set; }
        public string ImportDatabaseUserSurnameColumn { get; set; }
        public string ImportDatabaseUserRfidColumn { get; set; }
        public string ImportDatabaseUserTypeColumn { get; set; }
        public string ImportDatabaseUserEmailColumn { get; set; }
        public string ImportDatabaseUserPhoneColumn { get; set; }
        public string ImportDatabaseTable { get; set; }

        public Config() {
            DatabaseType = "mariadb";
            DatabaseIpAddress = "zapsidatabase";
            DatabasePort = "3306";
            DatabaseName = "zapsi2";
            DatabaseUsername = "root";
            DatabasePassword = "Zps05.....";
            DeleteLogFilesAfterDays = "10";
            CustomerName = "unactivated software";
            Email = "jahoda@zapsi.eu";
            SmtpClient = "smtp.forpsi.com";
            SmtpPort = "25";
            SmtpUsername = "support@zapsi.eu";
            SmtpPassword = "support01..";
            ImportDatabaseType = "sqlserver";
            ImportDatabaseIpAddress = "sql17.linaplast.local\\sqlheo";
            ImportDatabaseName = "Helios002";
            ImportDatabasePort = "1433";
            ImportDatabaseUsername = "zapsi";
            ImportDatabasePassword = "Zapsi";
            ImportDatabaseTable = "hvw_Zamestnanci";
            ImportDatabaseUserIdColumn = "Cislo";
            ImportDatabaseUserFirstNameColumn = "Prijmeni";
            ImportDatabaseUserSurnameColumn = "Jmeno";
            ImportDatabaseUserRfidColumn = "_EVOLoginZam";
            ImportDatabaseUserTypeColumn = "";
            ImportDatabaseUserEmailColumn = "";
            ImportDatabaseUserPhoneColumn = "";
        }
    }
}