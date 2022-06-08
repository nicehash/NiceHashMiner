using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetectionTests
{
    internal static class Paths
    {
        internal static string LoadTextFile(string pathRelativeUnitTestingFile) => File.ReadAllText(GetFullPathToFile(pathRelativeUnitTestingFile));

        #region load data region https://stackoverflow.com/questions/23826773/how-do-i-make-a-data-file-available-to-unit-tests/53004985
        internal static string GetFullPathToFile(string pathRelativeUnitTestingFile)
        {
            string folderProjectLevel = GetPathToCurrentUnitTestProject();
            string final = System.IO.Path.Combine(folderProjectLevel, pathRelativeUnitTestingFile);
            return final;
        }
        /// <summary>
        /// Get the path to the current unit testing project.
        /// </summary>
        /// <returns></returns>
        private static string GetPathToCurrentUnitTestProject()
        {
            string pathAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string folderAssembly = System.IO.Path.GetDirectoryName(pathAssembly);
            if (folderAssembly.EndsWith("\\") == false) folderAssembly = folderAssembly + "\\";
            string folderProjectLevel = System.IO.Path.GetFullPath(folderAssembly + "..\\..\\");
            return folderProjectLevel;
        }
        #endregion load data region https://stackoverflow.com/questions/23826773/how-do-i-make-a-data-file-available-to-unit-tests/53004985
    }
}
