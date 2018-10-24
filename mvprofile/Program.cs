using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace mvprofile
{
    /// <summary>
    /// A command line tool to rename a user profile by updating all references to the profile folder in the registry
    /// and renaming the folder.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string oldDir = args[0];
            string newDir = args[1];
            string tempUserHiveName = $"__tmpMoveUserProfile";

            // Even running as administrator, the functionality to load and unload registry hives
            // is restricted without requesting extra privilges.
            if (Privileges.EnablePrivileges("SeRestorePrivilege"))
            {
                int loadCode = RegistryUtils.LoadUserHive($"{oldDir}\\NTUSER.dat", tempUserHiveName);

                using (RegistryKey userHive = Registry.Users.OpenSubKey(tempUserHiveName))
                {
                    // Only need to deal with two trees - HKEY_LOCAL_MACHINE and HKEY_USERS.
                    // HKEY_CURRENT_USER won't be the user being modified (that hive is mounted under
                    // HKEY_USER, and other trees that may contain the path are apparently reflections of
                    // subtrees of one of the trees being modified.
                    Console.Out.WriteLine("==========Updating HKLM==========");
                    Registry.LocalMachine.IterateKeys((key) => UpdateKey(key, oldDir, newDir));
                    Console.Out.WriteLine("==========Updating HKU==========");
                    Registry.Users.IterateKeys((key) => UpdateKey(key, oldDir, newDir));
                }

                RegistryUtils.UnloadUserHive(tempUserHiveName);
                Directory.Move(oldDir, newDir);
            }
            else
            {
                Console.Error.WriteLine("Load key privilege not granted.");
            }
        }

        static public void UpdateKey(RegistryKey key, string oldDir, string newDir)
        {
            StringBuilder sb = new StringBuilder(300);
            int n = NativeMethods.FileSystem.GetShortPathName(oldDir, sb, 300);
            string shortOldDir = sb.ToString();

            string pattern = $"({Regex.Escape(oldDir)}|{Regex.Escape(shortOldDir)})";

            string[] names = key.GetValueNames();
            foreach (string name in names)
            {
                if (key.GetValueKind(name) == RegistryValueKind.MultiString)
                {
                    string[] oldValues = (string[])key.GetValue(name);
                    string[] newValues = oldValues.Select(value => Regex.Replace(value, pattern, newDir, RegexOptions.IgnoreCase)).ToArray();

                    if (!newValues.SequenceEqual(oldValues))
                    {
                        key.SetValue(name, newValues);
                        Console.Out.WriteLine(String.Join(":", newValues));
                    }
                }

                else if (key.GetValueKind(name) == RegistryValueKind.String || key.GetValueKind(name) == RegistryValueKind.ExpandString)
                {
                    string oldValue = (string)key.GetValue(name);
                    string newValue = Regex.Replace(oldValue, pattern, newDir, RegexOptions.IgnoreCase);

                    if (oldValue != newValue)
                    {
                        key.SetValue(name, newValue);
                        Console.Out.WriteLine(newValue);
                    }
                }

                string newValueName = Regex.Replace(name, pattern, newDir, RegexOptions.IgnoreCase);

                if (newValueName != name)
                {
                    key.RenameValue(name, newValueName);
                    Console.Out.WriteLine($"Rename: {newValueName}");
                }
            }

            string keypattern = pattern.Replace(@"\", @"%5C");
            string keyNewDir = newDir.Replace(@"\", @"%5C");
            foreach (string keyName in key.GetSubKeyNames())
            {
                string newKeyName = Regex.Replace(keyName, keypattern, keyNewDir, RegexOptions.IgnoreCase);
                if (keyName != newKeyName)
                {
                    key.RenameSubKey(keyName, newKeyName);
                    Console.Out.WriteLine($"Update key: {newKeyName}");
                }
            }
        }
    }
}
