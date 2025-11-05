using System.IO;

namespace DarkestDungeonEditor.Services
{
    public static class FileManager
    {
        public static void CreateLocalBackup(string filePath)
        {
            string backupPath = filePath + ".bak";
            if (!File.Exists(backupPath))
                File.Copy(filePath, backupPath);
        }

        public static void RestoreBackup(string filePath)
        {
            string backupPath = filePath + ".bak";
            if (File.Exists(backupPath))
                File.Copy(backupPath, filePath, overwrite: true);
        }
    }
}
