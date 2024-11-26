using System;
using System.IO;
using System.Net.Http.Headers;

namespace Program
{
    class Program
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

        public static String getParamValue(String param) {
            return param.Split("=").Last();
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
            } else
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

                if(timeIntervalNumber <= 0)
                {
                    Console.WriteLine("The parameter '--timeInterval' should not be negative", ConsoleColor.Red);
                    return -1;
                } else
                {
                    return timeIntervalNumber;
                }
            }
            else
            {
                Console.WriteLine("The parameter '--timeInterval' is not a valid number", ConsoleColor.Red);
                return -1;
            }
        }

        public static void Main(string[] args)
        {
            String sourcePath = "", replicaPath = "", timeInterval = "", logPath = "";

            String commandLineArgs = Environment.CommandLine.ToString();      

            if (isValidArgs(commandLineArgs))
            {
                List<String> listArgs = new List<String>();

                String[] commandArgs = commandLineArgs.Split(" ");

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
                    Console.WriteLine("working"); // inplement the code
                }

            } else
            {
                Console.WriteLine("ERROR_MESSAGE", ConsoleColor.Red);
            }
        }
    }
}
