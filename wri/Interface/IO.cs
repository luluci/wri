using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Joins;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using GlobExpressions;

namespace wri.Interface
{
    using UtilWinApi = global::Utility.WindowsApi;

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class IO
    {
        public FileIf file { get; set; }
        public DirectoryIf directory { get; set; }

        public IO()
        {
            file = new FileIf();
            directory = new DirectoryIf();
        }

        public void Save()
        {

        }

        public string[] GlobDirectories(string path, string pattern, bool fullpath = true)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
            }
            var root = new DirectoryInfo(path);
            //return root.GlobDirectories(pattern).Select(x => x.FullName).ToArray();
            var files = root.GlobDirectories(pattern);
            if (fullpath)
            {
                // フルパスに変換
                return files.Select(x => x.FullName).ToArray();
            }
            else
            {
                // 相対パスのまま返す
                return files.Select(x => x.FullName.Replace(root.FullName, "")).ToArray();
            }
        }

        public string[] GlobFiles(string path, string pattern, bool fullpath = true)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
            }
            var root = new DirectoryInfo(path);
            //return root.GetFiles(pattern).Select(x => x.FullName).ToArray();

            var files = new Microsoft.Extensions.FileSystemGlobbing.Matcher()
              .AddInclude(pattern) // 検索パターンを指定
              .Execute(new Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoWrapper(root)) // 検索のルートディレクトリを指定
              .Files;
            if (fullpath)
            {
                // フルパスに変換
                return files.Select(x => Path.Combine(path, x.Path)).ToArray();
            }
            else
            {
                // 相対パスのまま返す
                return files.Select(x => x.Path).ToArray();
            }
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class DirectoryIf
    {


        public bool Exists(string path)
        {
            var dir = global::System.IO.Path.GetDirectoryName(path);
            return global::System.IO.Directory.Exists(dir);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FileIf
    {
        public LinkedList<FileDescriptorIf> FileDescQueue;

        public FileIf()
        {
            FileDescQueue = new LinkedList<FileDescriptorIf>();
            //global::System.Console.WriteLine("File construct.");
        }

        public FileDescriptorIf Open(string path)
        {
            // dirチェック
            var dir = global::System.IO.Path.GetDirectoryName(path);
            if (!global::System.IO.Directory.Exists(dir))
            {
                global::System.IO.Directory.CreateDirectory(dir);
            }
            // file open
            var fd = new FileDescriptorIf(path);
            if (fd.IsOpen)
            {
                FileDescQueue.AddLast(fd);
                return fd;
            }
            return null;
        }

        public void SaveTo(string path, string text)
        {
            try
            {
                using (var sw = new global::System.IO.StreamWriter(path, false))//, Encoding.UTF8
                {
                    sw.Write(text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Dispose()
        {
            foreach (var elem in FileDescQueue)
            {
                elem.Dispose();
            }
            FileDescQueue.Clear();
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FileDescriptorIf
    {
        global::System.IO.FileStream fs;
        public bool IsOpen { get; set; }

        public FileDescriptorIf(string path)
        {
            IsOpen = false;
            try
            {
                fs = new global::System.IO.FileStream(path, global::System.IO.FileMode.OpenOrCreate);
                IsOpen = true;
            }
            finally
            {

            }
        }

        public void Dispose()
        {
            fs.Dispose();
            fs = null;
        }
    }
}
