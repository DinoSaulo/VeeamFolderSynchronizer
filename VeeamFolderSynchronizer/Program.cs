using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VeeamFolderSynchronizer
{
    internal class Program
    {
        public static Boolean isValidArgs(string commandArgs)
        {
            if (commandArgs.Contains("--sourcePath=") && commandArgs.Contains("--replicaPath=")
                && commandArgs.Contains("--timeInterval=") && commandArgs.Contains("--logPath="))
            {
                return true;
            }
            return false;
        }

        public static String getParamValue(String param)
        {
            return param.Split('=').Last();
        }

        public static Boolean validatePaths(String sourcePath, String replicaPath, String logPath)
        {
            if (!Directory.Exists(sourcePath) || !Directory.Exists(replicaPath) || !Directory.Exists(logPath))
            {
                String invalidPath = "";
                if (!Directory.Exists(sourcePath))
                {
                    invalidPath = "--sourcePath";
                }
                else if (!Directory.Exists(replicaPath))
                {
                    invalidPath = "--replicaPath";
                }
                else if (!Directory.Exists(logPath))
                {
                    invalidPath = "--logPath";
                }

                if (invalidPath == "")
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"The {invalidPath} folder was not found. Please try again.");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        public static int validateAndConvertTimeInterval(String timeInterval)
        {
            int val = 0;
            int timeIntervalNumber = -1; //if number is not valid will return -1

            if (Int32.TryParse(timeInterval, out val))
            {
                timeIntervalNumber = Int32.Parse(timeInterval);

                if (timeIntervalNumber <= 0)
                {
                    Console.WriteLine("The parameter '--timeInterval' should not be negative.");
                    return -1;
                }
                else
                {
                    return timeIntervalNumber;
                }
            }
            else
            {
                Console.WriteLine("The parameter '--timeInterval' is not a valid number");
                return -1;
            }
        }

        public static void logAndPrintAction(String logPath, String logMessage)
        {
            var utcNow = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss");

            String message =  $"{logMessage} - {utcNow}";
            
            File.AppendAllLines(Path.Combine(logPath, "log.txt"), [ message ]);

            Console.WriteLine(message);
        }

        private static void checkCopyOrCreateFiles(DirectoryInfo sourceFolder, String replicaPath, String logPath)
        {
            foreach (FileInfo file in sourceFolder.GetFiles())
            {
                string replicaFilePath = Path.Combine(replicaPath, file.Name);
                Boolean overwriteFile = true;
                String logAndPrintMessage = "";
                if (File.Exists(replicaFilePath))
                {
                    if (File.GetLastWriteTime(replicaFilePath) < file.LastWriteTime)
                    {
                        overwriteFile = true;
                        logAndPrintMessage = $"The '{file.FullName}' was modified. Replicating the modification in '{replicaFilePath}'";
                        file.CopyTo(replicaFilePath, overwriteFile);
                        logAndPrintAction(logPath, logAndPrintMessage);
                    }
                }
                else
                {
                    overwriteFile = false;
                    logAndPrintMessage = $"The '{file.FullName}' was copied to '{replicaFilePath}'";
                    file.CopyTo(replicaFilePath, overwriteFile);
                    logAndPrintAction(logPath, logAndPrintMessage);
                }
            }
        }

        private static void checkAndDeleteFiles(String sourcePath, String replicaPath, String logPath){
            foreach (FileInfo file in new DirectoryInfo(replicaPath).GetFiles())
            {
                string sourceFilePath = Path.Combine(sourcePath, file.Name);
                if (!File.Exists(sourceFilePath))
                {
                    file.Delete();
                    logAndPrintAction(logPath, $"The '{file.FullName}' was deleted in '{replicaPath}'");
                }
            }
        }

        private static void workWithSubfolders(DirectoryInfo[] subFolders, String replicaPath, String logPath)
        {
            foreach (DirectoryInfo subFolder in subFolders)
            {
                string destSubDirectory = Path.Combine(replicaPath, subFolder.Name);
                DirectoryInfo destSubDirectoryInfo = new DirectoryInfo(destSubDirectory);
                if (!destSubDirectoryInfo.Exists)
                {
                    destSubDirectoryInfo.Create();
                    logAndPrintAction(logPath, $"The subfolder '{subFolder.Name}' was created in '{replicaPath}'");
                }
                syncDirectories(subFolder.FullName, destSubDirectory, logPath);
            }
        }

        public static void syncDirectories(String sourcePath, String replicaPath, String logPath)
        {
            DirectoryInfo sourceFolder = new DirectoryInfo(sourcePath);
            DirectoryInfo[] subFolders = sourceFolder.GetDirectories();

            // copy
            checkCopyOrCreateFiles(sourceFolder, replicaPath, logPath);  

            // delete
            checkAndDeleteFiles(sourcePath, replicaPath, logPath);

            //work in subfolders
            workWithSubfolders(subFolders, replicaPath, logPath);
        }

        static void Main(string[] args)
        {
            String sourcePath = "", replicaPath = "", timeInterval = "", logPath = "";

            //String commandLineArgs = "run --sourcePath=C:\\sourcePath --replicaPath=C:\\replicaPath --timeInterval=1 --logPath=C:\\logPath";

            String commandLineArgs = Environment.CommandLine.ToString();

            String helpMessage = $"Usage: dotnet run --sourcePath=[sourcePath]  --replicaPath=[replicaPath] --timeInterval=[timeInterval] --logPath=[logPath]\n"+
                    "\t\tsourcePath\tRoot path that willl be synchronized\n" +
                    "\t\treplicaPath\tPath to the folder that will be receive the modifications\n" +
                    "\t\ttimeInterval\tTime in munutes to synchronize the folder again\n"+
                    "\t\tlogPath\t\tPath what will register the logs of synchronizer execution";

            if (isValidArgs(commandLineArgs))
            {
                List<String> listArgs = new List<String>();

                String[] commandArgs = commandLineArgs.Split(' ');

                foreach (String command in commandArgs)
                {
                    if (command.Contains("--sourcePath")) { sourcePath = getParamValue(command); }
                    if (command.Contains("--replicaPath")) { replicaPath = getParamValue(command); }
                    if (command.Contains("--timeInterval")) { timeInterval = getParamValue(command); }
                    if (command.Contains("--logPath")) { logPath = getParamValue(command); }
                }

                int timeIntervalNumber = validateAndConvertTimeInterval(timeInterval);

                if (validatePaths(sourcePath, replicaPath, logPath) && timeIntervalNumber != -1)
                {
                    logAndPrintAction(logPath, $"Staring to monitoring the folder: '{sourcePath}'");

                    while (true)
                    {
                        try
                        {
                            syncDirectories(sourcePath, replicaPath, logPath);
                        }
                        catch (Exception exception)
                        {
                            logAndPrintAction(logPath, $"Sync failed: {exception.Message}");
                        }
                        System.Threading.Thread.Sleep(timeIntervalNumber * 60 * 1000);
                    }

                    logAndPrintAction(logPath, "Sync completed!");
                } else
                {
                    Console.WriteLine(helpMessage);
                }
            }
            else
            {
                Console.WriteLine(helpMessage);
            }
        }
    }
}
