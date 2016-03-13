using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.Collections;

/// <summary> 
/// 适用与ZIP压缩 
/// </summary> 
public class SharpZipUtil
{
    public static void ZipFile(string fileToZip, string zipedFile, int compressionLevel = 9)
    {
        if (!File.Exists(fileToZip))
        {
            LogUtil.LogError("The specified file " + fileToZip + " could not be found.");
            return;
        }
        string directoryName = Path.GetDirectoryName(zipedFile);
        if (!string.IsNullOrEmpty(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipedFile)))
        {
            string fileName = Path.GetFileName(fileToZip);
            ZipEntry zipEntry = new ZipEntry(fileName);
            zipStream.PutNextEntry(zipEntry);
            zipStream.SetLevel(compressionLevel);
            byte[] buffer = new byte[2048];
            using (FileStream streamToZip = new FileStream(fileToZip, FileMode.Open, FileAccess.Read))
            {
                int size = streamToZip.Read(buffer, 0, buffer.Length);
                zipStream.Write(buffer, 0, size);

                while (size < streamToZip.Length)
                {
                    int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }
        }
    }

    public static ArrayList GetFileList(string directory)
    {
        ArrayList fileList = new ArrayList();
        bool isEmpty = true;
        foreach (string file in Directory.GetFiles(directory))
        {
            fileList.Add(file);
            isEmpty = false;
        }
        if (isEmpty)
        {
            if (Directory.GetDirectories(directory).Length == 0)
            {
                fileList.Add(directory + @"/");
            }
        }
        foreach (string dirs in Directory.GetDirectories(directory))
        {
            foreach (object obj in GetFileList(dirs))
            {
                fileList.Add(obj);
            }
        }
        return fileList;
    }

    public static void ZipDerctory(string directoryToZip, string zipedDirectory,int compressionLevel = 9)
    {
        string directoryName = Path.GetDirectoryName(zipedDirectory);       
        if (!string.IsNullOrEmpty(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipedDirectory)))
        {
            ArrayList fileList = GetFileList(directoryToZip);
            int directoryNameLength = (Directory.GetParent(directoryToZip)).ToString().Length;

            zipStream.SetLevel(compressionLevel);
            ZipEntry zipEntry = null;
            
            foreach (string fileName in fileList)
            {
                zipEntry = new ZipEntry(fileName.Remove(0, directoryNameLength));
                zipStream.PutNextEntry(zipEntry);

                
                if (!fileName.EndsWith(@"/"))
                {
                    byte[] buffer = new byte[2048];
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        int size = fileStream.Read(buffer, 0, buffer.Length);
                        zipStream.Write(buffer, 0, size);

                        while (size < fileStream.Length)
                        {
                            int sizeRead = fileStream.Read(buffer, 0, buffer.Length);
                            zipStream.Write(buffer, 0, sizeRead);
                            size += sizeRead;
                        }
                        fileStream.Close();
                    }
                }
            }
        }
    }

    public static void UnZipFile(string zipFilePath, string unZipFilePatah)
    {
        using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipFilePath)))
        {
            ZipEntry zipEntry = null;
            byte[] buffer = new byte[2048];
            while ((zipEntry = zipStream.GetNextEntry()) != null)
            {
                string fileName = Path.GetFileName(zipEntry.Name);
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (zipEntry.CompressedSize == 0)
                        break;
                    using (FileStream stream = File.Create(unZipFilePatah + fileName))
                    {
                        while (true)
                        {
                            int size = zipStream.Read(buffer, 0, buffer.Length);
                            if (size > 0)
                            {
                                stream.Write(buffer, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public static void UnZipDirectory(string zipDirectoryPath, string unZipDirecotyPath)
    {
        byte[] buffer = new byte[2048];
        using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipDirectoryPath)))
        {
            ZipEntry zipEntry = null;
            while ((zipEntry = zipStream.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(zipEntry.Name);
                string fileName = Path.GetFileName(zipEntry.Name);

                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    if (zipEntry.CompressedSize == 0)
                        break;
                    if (zipEntry.IsDirectory)
                    {
                        directoryName = Path.GetDirectoryName(unZipDirecotyPath + zipEntry.Name);
                        Directory.CreateDirectory(directoryName);
                    }

                    using (FileStream stream = File.Create(unZipDirecotyPath + zipEntry.Name))
                    {
                        while (true)
                        {
                            int size = zipStream.Read(buffer, 0, buffer.Length);
                            if (size > 0)
                            {
                                stream.Write(buffer, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    //zip压缩
    public static void SimpleZipFile(string filename, string directory)
    {
        try
        {
            ZipDerctory(directory, filename);
//             FastZip fz = new FastZip();
//             fz.CreateEmptyDirectories = true;
//             fz.CreateZip(filename, directory, true, "");
        }
        catch (Exception e)
        {
            LogUtil.LogException(e);
        }
    }
    //zip解压
    public static bool SimpleUnZipFile(string file, string dir)
    {
        try
        {
            UnZipDirectory(file, dir);
            //FastZip fastZip = new FastZip();
            //fastZip.ExtractZip(file, dir, "");
            return true;
        }
        catch (Exception e)
        {
            LogUtil.LogException(e);
            return false;
        }
    }
}
