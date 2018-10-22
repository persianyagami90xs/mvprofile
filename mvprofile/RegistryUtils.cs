using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mvprofile
{
    class RegistryUtils
    {
        public static void IterateKeys(RegistryKey root, Func<RegistryKey, bool> filter, Action<RegistryKey> processor)
        {
            if (root == null)
            {
                return;
            }

            Parallel.ForEach(root.GetSubKeyNames(), keyname =>
            {
                try
                {
                    using (RegistryKey key = root.OpenSubKey(keyname, true))
                    {
                        IterateKeys(key, filter, processor);

                        if (filter(key))
                        {
                            processor(key);
                        }
                    }
                }
                catch (Exception e)
                {
                }
            });
        }

        public static void RenameValue(RegistryKey key, string oldName, string newName)
        {
            key.SetValue(newName, key.GetValue(oldName), key.GetValueKind(oldName));
            key.DeleteValue(oldName);
        }

        public static void RenameSubKey(RegistryKey root, string oldName, string newName)
        {
            using (RegistryKey subKey = root.OpenSubKey(oldName))
            {
                CloneSubKey(root, subKey, newName);
            }

            root.DeleteSubKeyTree(oldName);
        }

        public static void CloneSubKey(RegistryKey root, RegistryKey source, string newName)
        {
            using (RegistryKey target = root.CreateSubKey(newName))
            {

                foreach (string name in source.GetValueNames())
                {
                    target.SetValue(name, source.GetValue(name), source.GetValueKind(name));
                }

                foreach (string name in source.GetSubKeyNames())
                {
                    using (RegistryKey subKey = source.OpenSubKey(name))
                    {
                        CloneSubKey(target, subKey, name);
                    }
                }
            }

        }

        [DllImport("advapi32.dll")]
        public static extern int RegLoadKey(IntPtr hkey, string lpSubKey, string lpFile);

        internal static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
    }
}
