using System.IO;

namespace ApplicationSetupWizardLite.Paths
{
    public class PathManager
    {
        public string baseDirPath { get; }
        public string xmlFilePath { get; }
        // public string excelBookDirPath { get; }
        // public string logDirPath { get; }
        public string defaultInstallPath { get; }
        public string payloadPath { get; }

        public PathManager(string baseDir)
        {
            baseDirPath = baseDir;
            xmlFilePath = Path.Combine(baseDir, "Conf", "config.xml");
            // excelBookDirPath = Path.Combine(baseDir, "workbook");
            // logDirPath = Path.Combine(baseDir, "log");
            defaultInstallPath = 
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                // Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            payloadPath = Path.Combine(baseDir, "Payload");
        }
    }
}
