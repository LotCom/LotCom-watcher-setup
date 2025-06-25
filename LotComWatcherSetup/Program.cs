using System.Security.Cryptography;
using System;
using System.Linq;
using WixSharp;
using System.Text.RegularExpressions;
using WixToolset.Dtf.WindowsInstaller;

namespace LotComWatcherSetup
{

    // block not needed when passing a version number to cli.
    // /// <summary>
    // /// Provides constant environment variables to the packager.
    // /// </summary>
    // class Constants
    // {
    //     public const string ProgramVersion = "1.0.0";
    // }

    /// <summary>
    /// Packages the LotCom Watcher application's generated release files into an MSI package.
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Accepts the command line arguments args and tests the first as a valid SemVer format Version Number.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool CheckVersionNumber(string[] args)
        {
            // confirm a version is passed in args
            if (args is null || args.Length < 1 || args[0] is null)
            {
                Console.WriteLine("No version number provided; exiting setup.");
                return false;
            }
            // confirm the passed argument is a valid SemVer version number (1.0.2, 0.3.01.21, etc)
            else
            {
                string VersionNumber = args[0];
                Regex SemVer = new Regex(@"[0-9]+.[0-9]+.[0-9]+");
                if (!SemVer.IsMatch(VersionNumber))
                {
                    Console.WriteLine($"'{VersionNumber}' is not a valid version number; exiting setup.");
                    return false;
                }
            }
            // first arg in passed args[] was a valid version number
            return true;
        }

        /// <summary>
        /// Generates a new GUID (unique product ID) from input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static Guid GenerateProductId(string input)
        {
            // create a new SHA256 hash and compute its initial hash value
            SHA256 Sha256 = SHA256.Create();
            byte[] HashBytes = Sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            // ensure the byte array is exactly 16 bytes long, as required for a GUID.
            // SHA256 generates 32 bytes, so we take the first 16 bytes.
            byte[] GuidBytes = new byte[16];
            Array.Copy(HashBytes, GuidBytes, 16);
            // Construct the GUID from the 16-byte array.
            return new Guid(GuidBytes);
        }

        /// <summary>
        /// Creates a new Wix Project and builds an MSI from that Project.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // return; // REMOVE THIS LINE TO ENABLE BUILDING
            // check and capture the version number from cli
            string VersionNumber;
            if (CheckVersionNumber(args))
            {
                VersionNumber = args[0];
            }
            else
            {
                return;
            }
            // set to the directory of the Project folder (side-loaded in <Project>\LotCoMWatcher\Setup)
            Environment.CurrentDirectory = @"..\bin\Release\net9.0-windows10.0.19041.0\win-x64";  // setup project script home
            Environment.SetEnvironmentVariable("LATEST_RELEASE", VersionNumber);
            // generates a new GUID for the specific version installation
            Guid ProductId = GenerateProductId($"LotCom Watcher {VersionNumber}");  // Do not change
            // creates a new WiX# project for the LotComWatcher application
            Project LotComWatcherSetup = new Project
            (
                "LotComWatcherSetup",   // WiX# project name
                new Dir
                (
                    @"C:\ProgramData\Yamada North America\LotCom Watcher", // directory of the new WiX# Project
                    new Files(@"*.*")  // includes all Latest Release files in new WiX# project's folder
                ),
                new CloseApplication
                (
                    new Id($"LotComWatcher_{VersionNumber}"),   // WiX# Id that "stamps" the XML files in the project MSI
                    "LotCoMWatcher.exe",    // targeted .exe file
                    false,                  // show a close message
                    false                   // do not prompt a reboot
                )
                {
                    Timeout = 15    // set the CloseApplication action's timeout to 15 seconds
                }
            );
            // set the Project's properties
            LotComWatcherSetup.Name = "LotCom Watcher";
            LotComWatcherSetup.ProductId = ProductId;
            LotComWatcherSetup.UpgradeCode = new Guid("B8922687-A74A-45D4-96AA-17C91224DB7A"); // Do not change
            LotComWatcherSetup.Version = new Version(VersionNumber);
            LotComWatcherSetup.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;
            // LotComWatcherSetup.GUID = new Guid("99bcb1fa-e3c6-4ba7-b08e-1ea0ce5e4fda");
            // LotComWatcherSetup.LicenceFile = @".\License.rtf";
            LotComWatcherSetup.ControlPanelInfo.Comments = "LotCom Watcher Application";
            // LotComWatcherSetup.ControlPanelInfo.Readme = "https://github.com/LotCoM/LotCom-watcher/blob/stable/README.md";
            // LotComWatcherSetup.ControlPanelInfo.HelpLink = "https://github.com/LotCoM/LotCom-watcher/blob/stable/README.md";
            LotComWatcherSetup.ControlPanelInfo.HelpTelephone = "(937) 260-9790";
            LotComWatcherSetup.ControlPanelInfo.UrlInfoAbout = "https://github.com/LotCoM/LotCom-watcher/blob/stable/README.md";
            // LotComWatcherSetup.ControlPanelInfo.UrlUpdateInfo = "https://github.com/oleg-shilo/wixsharp/update";
            // LotComWatcherSetup.ControlPanelInfo.ProductIcon = @"lotcom_logo.scale-100.png";
            LotComWatcherSetup.ControlPanelInfo.Contact = "YNA IT";
            LotComWatcherSetup.ControlPanelInfo.Manufacturer = "Yamada North America";
            LotComWatcherSetup.ControlPanelInfo.InstallLocation = "[INSTALLDIR]";
            LotComWatcherSetup.ControlPanelInfo.NoModify = true;
            LotComWatcherSetup.ControlPanelInfo.NoRepair = true;
            // LotComWatcherSetup.ControlPanelInfo.NoRemove = true;
            // LotComWatcherSetup.ControlPanelInfo.SystemComponent = true; //if set will not be shown in Control Panel
            LotComWatcherSetup.SourceBaseDir = Environment.CurrentDirectory;
            LotComWatcherSetup.OutDir = @".\Installer";
            LotComWatcherSetup.OutFileName = $"LotComWatcher_{LotComWatcherSetup.Version}";
            LotComWatcherSetup.UI = WUI.WixUI_Minimal;
            LotComWatcherSetup.ResolveWildCards();
            // get the .exe file
            WixSharp.File ExeFile = LotComWatcherSetup.AllFiles.Single(x => x.Name.EndsWith("LotComWatcher.exe"));
            // set the .exe file shortcut wildcards
            ExeFile.Shortcuts = new[]
            {
                new FileShortcut("LotCom Watcher", @"%StartMenuFolder%"),
                new FileShortcut("LotCom Watcher", @"%Desktop%")
            };
            // build the MSI package from the Setup Project
            LotComWatcherSetup.BuildMsi();
        }
    }
}