using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using System;
using System.IO;

namespace NiceHashMiner.Configs.ConfigJsonFile
{
    public abstract class ConfigFile<T> where T : class
    {
        // statics/consts
        private const string TagFormat = "ConfigFile<{0}>";

        private readonly string _confFolder; // = @"configs\";
        private readonly string _tag;

        private void CheckAndCreateConfigsFolder()
        {
            try
            {
                if (Directory.Exists(_confFolder) == false)
                {
                    Directory.CreateDirectory(_confFolder);
                }
            }
            catch { }
        }

        // member stuff
        protected string FilePath;

        protected string FilePathOld;

        protected ConfigFile(string iConfFolder, string fileName, string fileNameOld)
        {
            _confFolder = iConfFolder;
            if (fileName.Contains(_confFolder))
            {
                FilePath = fileName;
            }
            else
            {
                FilePath = _confFolder + fileName;
            }
            if (fileNameOld.Contains(_confFolder))
            {
                FilePathOld = fileNameOld;
            }
            else
            {
                FilePathOld = _confFolder + fileNameOld;
            }
            _tag = string.Format(TagFormat, typeof(T).Name);
        }

        public bool IsFileExists()
        {
            return File.Exists(FilePath);
        }

        public T ReadFile()
        {
            CheckAndCreateConfigsFolder();
            T file = null;
            try
            {
                if (File.Exists(FilePath))
                {
                    file = JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath), Globals.JsonSettings);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(_tag, $"ReadFile {FilePath}: exception {ex}");
                Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {ex}");
                file = null;
            }
            return file;
        }

        public void Commit(T file)
        {
            CheckAndCreateConfigsFolder();
            if (file == null)
            {
                Logger.Info(_tag, $"Commit for FILE {FilePath} IGNORED. Passed null object");
                Helpers.ConsolePrint(_tag, $"Commit for FILE {FilePath} IGNORED. Passed null object");
                return;
            }
            try
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(file, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.Error(_tag, $"Commit {FilePath}: exception {ex}");
                Helpers.ConsolePrint(_tag, $"Commit {FilePath}: exception {ex}");
            }
        }

        public void CreateBackup()
        {
            Logger.Debug(_tag, $"Backing up {FilePath} to {FilePathOld}..");
            Helpers.ConsolePrint(_tag, $"Backing up {FilePath} to {FilePathOld}..");
            try
            {
                if (File.Exists(FilePathOld))
                    File.Delete(FilePathOld);
                File.Copy(FilePath, FilePathOld, true);
            }
            catch (Exception ex)
            {
                Logger.Error(_tag, $"CreateBackup {FilePath}: exception {ex}");
            }
        }
    }
}
