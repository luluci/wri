using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Interface
{
    using UtilWinApi = Utility.WindowsApi;

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
