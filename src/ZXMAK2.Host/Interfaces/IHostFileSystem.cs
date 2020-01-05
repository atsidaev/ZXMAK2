using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostFileSystem
    {
        bool FileExists(string configName);
        string ReadAllText(string configName);
        void ReadBytes(string fileName, byte[] buffer, int offset, int count);
        void WriteBytes(string fileName, byte[] buffer, int offset, int count);
        FileAttributes GetAttributes(string fileName);
        Stream OpenFile(string fileName, FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.Read);
        bool DirectoryExists(string romsFolderName);
        long GetFileLength(string romsFileName);
        string CombinePath(params string[] paths);
        void CreateDirectory(string folderName);
        string GetFullPathFromRelativePath(string fileName, string v);
        string ReadAllBytes(string oldAsmName);
    }
}
