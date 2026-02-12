using GlobExpressions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Joins;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Utility;

namespace wri.Interface
{
    using UtilWinApi = global::Utility.WindowsApi;

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class IO
    {
        public FileIf file { get; set; }
        public DirectoryIf directory { get; set; }
        public PathIf path { get; set; }

        public IO()
        {
            file = new FileIf();
            directory = new DirectoryIf();
            path = new PathIf();
        }

        public void Save()
        {

        }

        public string ConvertXml2Html(string path)
        {
            try
            {
                var xml = new XmlLoader();
                xml.Load(path);
                return xml.GetView();
            }
            catch (Exception ex)
            {
                return $@"
<html>
<head><meta charset='UTF-8'></head>
<body>
<h1>XMLの取得に失敗しました。</h1>
<p>
${ex.Message}
</p>
</body>
";
            }
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
    public class PathIf
    {
        public string ExeDirectoryPath
        {
            get
            {
                return GlobalData.ExeDirectoryPath;
            }
        }

        public string MakeExeDirRelPath(string relpath)
        {
            return System.IO.Path.Combine(GlobalData.ExeDirectoryPath, relpath);
        }

        public string Combine(string path, string file)
        {
            return System.IO.Path.Combine(path, file);
        }

        public string GetExtension(string path)
        {
            return System.IO.Path.GetExtension(path);
        }

        public string GetFullPath(string path)
        {
            return System.IO.Path.GetFullPath(path);
        }
                public string GetDirectoryName(string path)
        {
            return System.IO.Path.GetDirectoryName(path);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class DirectoryIf
    {

        public bool Exists(string path)
        {
            return System.IO.Directory.Exists(path);
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

        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public string OpenDialog(string title = "Select File", string filter = "すべてのファイル(*.*)|*.*")
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    FileName = "",
                    InitialDirectory = "",
                    Filter = filter,
                    Title = title,
                    RestoreDirectory = true,
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return null;
        }

        public string Read(string path, string encoding = "utf8")
        {
            try
            {
                Encoding enc = Encoding.UTF8;
                switch (encoding.ToLower())
                {
                    case "utf8":
                    case "utf-8":
                        // BOMなしUTF-8
                        enc = new System.Text.UTF8Encoding(false, false);
                        break;
                    case "932":
                    case "cp932":
                    case "shift_jis":
                    case "sjis":
                        enc = Encoding.GetEncoding(932);
                        break;
                    default:
                        enc = Encoding.UTF8;
                        break;
                }

                using (var sr = new System.IO.StreamReader(path, enc))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return null;
        }

        public FileDescriptorIf Open(string path)
        {
            try
            {
                // dirチェック
                var dir = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                // file open
                var fd = new FileDescriptorIf(path);
                if (fd.IsOpen)
                {
                    FileDescQueue.AddLast(fd);
                    return fd;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return null;
        }

        public void SaveTo(string path, string text)
        {
            try
            {
                using (var sw = new System.IO.StreamWriter(path, false))//, Encoding.UTF8
                {
                    sw.Write(text);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
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
        System.IO.FileStream fs;
        public bool IsOpen { get; set; }

        public FileDescriptorIf(string path)
        {
            IsOpen = false;
            try
            {
                fs = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
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
