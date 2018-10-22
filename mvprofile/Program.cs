using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace mvprofile
{
    class Program
    {
        static void Main(string[] args)
        {
            string oldName = "Christopher Lansing";
            string newName = "clansing";
            string oldDir = $"Users\\{oldName}";
            string newDir = $"Users\\{newName}";

            if (Privileges.EnablePrivileges("SeRestorePrivilege"))
            {
                int loadCode = RegistryUtils.RegLoadKey(RegistryUtils.HKEY_USERS, oldName, @"C:\Users\clansing\clansing.hiv");

                Func<RegistryKey, bool> filter = (key) =>
                    key.ToString().IndexOf(oldDir, StringComparison.OrdinalIgnoreCase) != -1 ||
                    key.GetValueNames().Any((name) =>
                        name.IndexOf(oldDir, StringComparison.OrdinalIgnoreCase) != -1 ||
                        key.GetValueKind(name) == RegistryValueKind.String && ((string)key.GetValue(name)).IndexOf(oldDir, StringComparison.OrdinalIgnoreCase) != -1 ||
                        key.GetValueKind(name) == RegistryValueKind.MultiString && ((string[])key.GetValue(name)).Any((value) => value.IndexOf(oldDir, StringComparison.OrdinalIgnoreCase) != -1)
                    );

                RegistryUtils.IterateKeys(Registry.Users.OpenSubKey(oldName), filter, (key) => UpdateKey(key, oldDir, newDir));
            }
            else
            {
                Console.Error.WriteLine("Load key privilege not granted.");
            }
        }

        static private void UpdateKey(RegistryKey key, string oldDir, string newDir)
        {
            string[] names = key.GetValueNames();
            foreach (string name in names)
            {
                if (key.GetValueKind(name) == RegistryValueKind.MultiString)
                {
                    string[] oldValues = (string[])key.GetValue(name);
                    string[] newValues = (string[])oldValues.Select(value => Regex.Replace(value, Regex.Escape(oldDir), newDir, RegexOptions.IgnoreCase));

                    if (!newValues.SequenceEqual(oldValues))
                    {
                        Console.Out.WriteLine(String.Join(":", newValues));
                    }
                }

                if (key.GetValueKind(name) == RegistryValueKind.String)
                {
                    string oldValue = (string)key.GetValue(name);
                    string newValue = Regex.Replace(oldValue, Regex.Escape(oldDir), newDir, RegexOptions.IgnoreCase);

                    if (oldValue != newValue)
                    {

                        Console.Out.WriteLine(newValue);
                    }
                }

                string newValueName = Regex.Replace(name, Regex.Escape(oldDir), newDir, RegexOptions.IgnoreCase);

                if (newValueName != name)
                {
                    Console.Out.WriteLine($"Rename: {newValueName}");
                }
            }

            string newKeyName = Regex.Replace(key.Name, Regex.Escape(oldDir), newDir, RegexOptions.IgnoreCase);

            if (key.Name != newKeyName)
            {
                Console.Out.WriteLine($"Update key: {newKeyName}");
            }
        }
    }
}
