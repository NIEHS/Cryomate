using System;
using System.IO;

namespace SprayingSystem.Utility
{
    public class FileUtil
    {
        public static void CreateFolderIfNotExist(string filename)
        {
            var path = Path.GetDirectoryName(filename);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Creates a file name with a Date and Time stamp.
        /// </summary>
        public static string CreateFilenameWithDateTime(string folder, string prefix, string extension)
        {
            var localTime = DateTime.Now;

            var dateTime = localTime.ToString("yyyy-MM-dd_HH-mm-ss");

            var filename = $"{prefix}{dateTime}.{extension}";
            var fullname = Path.Combine(folder, filename);
            return fullname;
        }
    }
}
