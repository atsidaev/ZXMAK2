using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostFileSystem
    {
        #region File methods
        bool FileExists(string fileName);
        string ReadAllText(string fileName);
        byte[] ReadAllBytes(string fileName);
        void ReadBytes(string fileName, byte[] buffer, int offset, int count);
        void WriteBytes(string fileName, byte[] buffer, int offset, int count);
        FileAttributes GetAttributes(string fileName);
        Stream OpenFile(string fileName, FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read);
        long GetFileLength(string fileName);
        #endregion

        #region Directory methods
        void CreateDirectory(string directoryName);
        bool DirectoryExists(string directoryName);
        #endregion

        #region Path methods
        string CombinePath(params string[] paths);
        string GetFullPathFromRelativePath(string relFileName, string rootPath);
        string ChangeExtensionInPath(string fileName, string newExtension);
        string GetDirectoryName(string path);
        string GetFullPath(string path);
        string GetFileName(string fileName);
        string GetExtension(string fileName);
        bool IsPathRooted(string fileName);
        #endregion
    }
}
