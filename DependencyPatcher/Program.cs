using System;

namespace DependencyPatcher
{
    internal class Program
    {
        // TODO - Could increase performance with multi threading
        // Folders can be split amongst different cores for increased speed
        // Though this is still somewhat handicapped by the read/write speed of the harddisk

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the path of the directory containing the files to patch in:");
            string? readDirectoryPath = Console.ReadLine();

            if (!Directory.Exists(readDirectoryPath))
            {
                throw new DirectoryNotFoundException(readDirectoryPath + " not found!");
            }

            List<string> foldersToCheck = new List<string>();
            foldersToCheck.Add(readDirectoryPath);
            List<string> filePaths = new List<string>();
            filePaths.Add(readDirectoryPath);
            // Collect all the .smali files
            while (true)
            {
                string folderToCheck = foldersToCheck[0];
                foldersToCheck.RemoveAt(0);
                string[] files = Directory.GetFiles(folderToCheck);
                foreach (string file in files)
                {
                    if (file.EndsWith(".smali"))
                    {
                        filePaths.Add(file);
                    }
                }

                string[] subDirs2 = Directory.GetDirectories(folderToCheck);
                foreach (string subDir in subDirs2)
                {
                    foldersToCheck.Add(subDir);
                }

                if (foldersToCheck.Count <= 0)
                {
                    break;
                }
            }

            Console.WriteLine((filePaths.Count - 1) + " .smali files to patch!");

            List<string> classes = new List<string>();
            // Collect all the classes that need to be changed later
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(".class"))
                        {
                            int capitalLIndex = line.IndexOf('L');
                            if (capitalLIndex > 0)
                            {
                                string className = line.Substring(capitalLIndex, line.Length - capitalLIndex);
                                classes.Add(className);
                                break;
                            }
                        }
                    }
                }
            }

            Console.WriteLine(classes.Count + " classes to patch.");

            // TODO - Rename all the files to their $patched version

            int patchedLineCount = 0;
            List<string> patchedFilePaths = new List<string>(filePaths.Count);
            // Create the $patched versions of the files and edit their contents accordingly
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    string patchedFilePath = filePath;

                    int prefixIndex = filePath.LastIndexOf('\\') + 1;
                    int suffixIndex = filePath.LastIndexOf('.');
                    if (prefixIndex >= 0 && suffixIndex >= 0)
                    {
                        patchedFilePath = filePath.Substring(0, suffixIndex);
                        patchedFilePath += "Patched.smali";
                    }

                    string[] lines = File.ReadAllLines(filePath);
                    List<string> newLines = new List<string>(lines.Length);
                    foreach (string line in lines)
                    {
                        string newLine = line;

                        List<string> foundClasses = classes.FindAll(x => line.Contains(x));
                        foreach(string foundClass in foundClasses)
                        {
                            string prefixClass = foundClass.Substring(0, foundClass.Length - 1);
                            string patchedClass = prefixClass + "Patched;";
                            newLine = newLine.Replace(foundClass, patchedClass);
                            patchedLineCount += 1;
                        }

                        newLines.Add(newLine);
                    }

                    File.WriteAllLines(patchedFilePath, newLines);
                    patchedFilePaths.Add(patchedFilePath);
                }
            }

            Console.WriteLine(patchedLineCount + " lines have been altered.");

            Console.WriteLine("Please enter the directory to patch the files into:");
            string? writeDirectoryPath = Console.ReadLine();

            if (writeDirectoryPath == null || !Directory.Exists(writeDirectoryPath))
            {
                throw new DirectoryNotFoundException(writeDirectoryPath + " is not a valid directory!");
            }

            int smaliCount = 0;
            // Get all the smali directories so we can determine at what number to start
            string[] subDirs = Directory.GetDirectories(writeDirectoryPath);
            foreach (string subDir in subDirs)
            {
                int lastPartIndex = subDir.LastIndexOf('\\');
                if (subDir.Substring(lastPartIndex).Contains("smali"))
                {
                    smaliCount += 1;
                }
            }

            // Copy the $patched files to the target directory
            foreach (string patchedFilePath in patchedFilePaths)
            {
                string fileName = patchedFilePath.Substring(filePaths[0].Length);
                string tempFileName = fileName.Substring(1);
                int endSmaliIndex = tempFileName.IndexOf('\\');
                string tempSmali = tempFileName.Substring(0, endSmaliIndex);
                int numberIndex = tempSmali.IndexOf('_');
                int smaliCounterModifier = 1;
                if (numberIndex >= 0)
                {
                    smaliCounterModifier = Convert.ToInt32(tempSmali.Substring(13));
                }

                int subDirIndex = fileName.LastIndexOf('\\');
                string subDir = fileName.Substring(0, subDirIndex);
                string tempSubDir = subDir.Substring(1);
                int smaliIndex = tempSubDir.IndexOf('\\');
                string realSubDir = tempSubDir.Substring(smaliIndex);

                string newDir = writeDirectoryPath + "\\smali_classes" + (smaliCount + smaliCounterModifier) + realSubDir;
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }

                int lastSlashIndex = fileName.LastIndexOf('\\');
                if (lastSlashIndex > 0)
                {
                    string shortFileName = fileName.Substring(lastSlashIndex);
                    File.Copy(patchedFilePath, newDir + shortFileName);
                    File.Delete(patchedFilePath);
                }
            }
        }
    }
}