using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Security.Principal;
using System.Diagnostics;

namespace VRCXMetadata {
    internal class Program {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {
            if (args.Length == 0) {
                ShowInstaller();
                return;
            }
            if (!args[0].EndsWith(".png")) {
                return;
            }

            IEnumerable <Directory> directories = ImageMetadataReader.ReadMetadata(args[0]);

            bool foundData = false;

            foreach (var directory in directories) {
                foreach (var tag in directory.Tags) {
                    if (!tag.Description.Contains("{") || !tag.Description.StartsWith("Description:")) {
                        continue;
                    }

                    string jsonString = tag.Description;
                    jsonString = jsonString.Substring(jsonString.IndexOf("{"));

                    try {
                        foundData = true;
                        JObject obj = JObject.Parse(jsonString);
                        ShowOutput(obj, false);
                    }
                    catch {

                    }
                }
            }

            if (!foundData) {
                Console.WriteLine("---- VRCX Metadata ----");
                Console.WriteLine("No metadata found, maybe this photo wasn't taken with VRCX running?");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Show install/uninstall screen.
        /// </summary>
        static void ShowInstaller() {
            bool isElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            if (!isElevated) {
                Console.WriteLine("---- VRCX Metadata ----");
                Console.WriteLine("Must be running as admin to install/uninstall!");

                ProcessStartInfo startInfo = new ProcessStartInfo(Environment.CurrentDirectory + @"\VRCXMetadata.exe");
                startInfo.Verb = "runas";
                Process.Start(startInfo);
                Environment.Exit(0);
            }

            Console.WriteLine("---- VRCX Metadata ----");
            Console.WriteLine("1 - Install");
            Console.WriteLine("2 - Uninstall");
            Console.WriteLine("Escape - Quit");

            var key = Console.ReadKey();
            // Install.
            if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1) {
                RegistryKey regKey = Registry.ClassesRoot;
                RegistryKey subKey = regKey.CreateSubKey(@"*\shell\View VRC Info");
                RegistryKey commandKey = subKey.CreateSubKey("command");
                commandKey.SetValue("", Environment.CurrentDirectory + @"\VRCXMetadata.exe ""%1""");

                Console.Clear();
                Console.WriteLine("---- VRCX Metadata ----");
                Console.WriteLine("Installed!");
                Console.WriteLine("");
                Console.WriteLine("Usage:");
                Console.WriteLine("- Right click a VRChat photo taken with VRCX open");
                Console.WriteLine("- Click 'View VRC Info'");
                Console.WriteLine("(On Windows 11 click 'Show more options')");
                Console.WriteLine("");
                Console.WriteLine("Thanks for checking out this software!");
                Console.ReadKey();
            }
            // Uninstall.
            else if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2) {
                RegistryKey regKey = Registry.ClassesRoot;
                regKey.DeleteSubKeyTree(@"*\shell\View VRC Info");

                Console.Clear();
                Console.WriteLine("---- VRCX Metadata ----");
                Console.WriteLine("Uninstalled, you can now delete these files!");
                Console.WriteLine("");
                Console.WriteLine("Thanks for checking out this software!");
                Console.ReadKey();
            }
        }
        
        /// <summary>
        /// Show VRCX data output.
        /// </summary>
        /// <param name="obj">VRCX data</param>
        /// <param name="showPlayers">If we should show the players in instance</param>
        static void ShowOutput(JObject obj, bool showPlayers) {
            Console.Clear();

            Console.WriteLine("---- VRCX Metadata ----");
            Console.WriteLine("Author: " + obj["author"]["displayName"]);
            Console.WriteLine("");
            Console.WriteLine("World: " + obj["world"]["name"]);
            Console.WriteLine("");

            if (showPlayers) {
                Console.WriteLine("Players in instance:");
                foreach (JToken entry in obj["players"]) {
                    Console.WriteLine(entry["displayName"]);
                }
                Console.WriteLine("");
            }

            Console.WriteLine("Commands:");
            Console.WriteLine("1 - Open world in browser");
            Console.WriteLine("2 - View players in instance");
            Console.WriteLine("Escape - Quit");

            var key = Console.ReadKey();
            // Open world in browser.
            if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1) {
                System.Diagnostics.Process.Start("https://vrchat.com/home/world/" + obj["world"]["id"]);
                Environment.Exit(0);
            }
            // View playters in instance.
            else if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2) {
                ShowOutput(obj, true);
            }
        }
    }
}
