using UnityEngine;
using System.Collections;
using Ionic.Zip;
using System.IO;
using System.Text;

public class GUnZip  {

    public static void Zip(string zipFileName, string[] files, string releative = "",string password = null)
    {
        try
        {
            string directoryName = Path.GetDirectoryName(zipFileName);
            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            int directoryNameLength = releative.Length;

            using (ZipFile zip = new ZipFile(Encoding.Default))
            {
                // This is just a sample, provided to illustrate the DotNetZip interface.  
                // This logic does not recurse through sub-directories.
                // If you are zipping up a directory, you may want to see the AddDirectory() method, 
                // which operates recursively. 
                foreach (string filename in files)
                {
                    using (FileStream streamToZip = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[2048];
                        int size = streamToZip.Read(buffer, 0, buffer.Length);
                        MemoryStream ms = new MemoryStream();
                        ms.Position = 0;
                        BinaryWriter writer = new BinaryWriter(ms);
                        writer.Write(buffer,0,size);
                        while (size < streamToZip.Length)
                        {
                            int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                            writer.Write(buffer,0,sizeRead);
                            size += sizeRead;
                        }

                        zip.AddEntry(filename.Remove(0, directoryNameLength), ms.ToArray());
                    }
                }

                //zip.Comment = string.Format("This zip archive was created by the CreateZip example application on machine '{0}'",
                //   System.Net.Dns.GetHostName());
                if (!string.IsNullOrEmpty(password))
                    zip.Password = password;
                zip.Save(zipFileName);
            }
        }
        catch (System.Exception ex1)
        {
            LogUtil.LogError(ex1.Message);
        }
    }

    public static void Zip(string zipFileName,string filename,string password = null)
    {
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(filename);
            if (dirInfo.Attributes != FileAttributes.Directory)
            {
                ZipFile zip = new ZipFile(Encoding.Default);
                using (FileStream streamToZip = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[2048];
                    int size = streamToZip.Read(buffer, 0, buffer.Length);
                    MemoryStream ms = new MemoryStream();
                    ms.Position = 0;
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write(buffer, 0, size);
                    while (size < streamToZip.Length)
                    {
                        int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                        writer.Write(buffer, 0, sizeRead);
                        size += sizeRead;
                    }

                    zip.AddEntry(streamToZip.Name, ms.ToArray());
                }
                if (!string.IsNullOrEmpty(password))
                    zip.Password = password;
                zip.Save(zipFileName);
            }
            else
            {
                string[] files = Directory.GetFiles(filename, "*.*", SearchOption.AllDirectories);
                Zip(zipFileName, files, filename, password);
            }
        }
        catch (System.Exception ex1)
        {
            LogUtil.LogError(ex1.Message);
        }
    }

    public static void UnZip(string zipFileName,string directory,string password = null)
    {
        try
        {
            // Specifying Console.Out here causes diagnostic msgs to be sent to the Console
            // In a WinForms or WPF or Web app, you could specify nothing, or an alternate
            // TextWriter to capture diagnostic messages.
            using (ZipFile zip = ZipFile.Read(zipFileName))
            {
                // This call to ExtractAll() assumes:
                //   - none of the entries are password-protected.
                //   - want to extract all entries to current working directory
                //   - none of the files in the zip already exist in the directory;
                //     if they do, the method will throw.
                if (!string.IsNullOrEmpty(password))
                    zip.Password = password;
                zip.ExtractAll(directory, ExtractExistingFileAction.OverwriteSilently);
            }
        }
        catch (System.Exception ex1)
        {
            LogUtil.LogError(ex1.Message);
        }
    }
}
