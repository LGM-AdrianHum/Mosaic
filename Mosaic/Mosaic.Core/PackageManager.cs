using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Mosaic.Core
{
    public static class PackageManager
    {
        public static void Unpack(string file, string targetDir, bool makeBackup = true)
        {
            using (var s = new ZipInputStream(File.OpenRead(file)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    string directoryName = targetDir + "\\" + Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        fileName = targetDir + "\\" + theEntry.Name;
                        if (makeBackup && File.Exists(fileName))
                        {
                            string backupName = fileName;
                            do
                            {
                                backupName = backupName + ".bak";
                            } while (File.Exists(backupName));
                            File.Move(fileName, backupName);
                        }

                        using (FileStream streamWriter = File.Create(fileName))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
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
    }
}
