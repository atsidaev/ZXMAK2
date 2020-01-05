using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Host.Presentation
{
    public class HostFileSystem : IHostFileSystem
    {
        #region File methods
        public bool FileExists(string fileName) => File.Exists(fileName);
        public string ReadAllText(string fileName) => File.ReadAllText(fileName);
        public byte[] ReadAllBytes(string fileName) => File.ReadAllBytes(fileName);
        public void ReadBytes(string fileName, byte[] buffer, int offset, int count)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fs.Read(buffer, offset, count);
            }
        }
        public void WriteBytes(string fileName, byte[] buffer, int offset, int count)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Write(buffer, offset, count);
            }
        }
        public FileAttributes GetAttributes(string fileName) => File.GetAttributes(fileName);
        public Stream OpenFile(string fileName, FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read)
        {
            return new FileStream(fileName, mode, access, share);
        }
        public long GetFileLength(string fileName) => new FileInfo(fileName).Length;
        #endregion

        #region Directory methods
        public void CreateDirectory(string directoryName) => Directory.CreateDirectory(directoryName);
        public bool DirectoryExists(string directoryName) => Directory.Exists(directoryName);
        #endregion

        #region Path methods
        public string CombinePath(params string[] paths) => Path.Combine(paths);
        public string GetFullPathFromRelativePath(string relFileName, string rootPath)
        {
            // TODO: rewrite with safe version http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
            string current = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(rootPath);
                return Path.GetFullPath(relFileName);
            }
            finally
            {
                Directory.SetCurrentDirectory(current);
            }
        }
        public string ChangeExtensionInPath(string fileName, string newExtension) => Path.ChangeExtension(fileName, newExtension);
        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        public string GetFullPath(string path) => Path.GetFullPath(path);
        public string GetFileName(string fileName) => Path.GetFileName(fileName);
        public string GetExtension(string fileName) => Path.GetExtension(fileName);
        public bool IsPathRooted(string fileName) => Path.IsPathRooted(fileName);
        #endregion
    }
    }
