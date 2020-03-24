using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace GAFStreamTool
{
    public static class RegistryEditor
    {
        private const string _REGISTRY_NAME = "GAFStreamTool";

        public static void Set(string key, int value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.DWord);
        }

        public static void Set(string key, float value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.DWord);
        }

        public static void Set(string key, short value)
        {
            Set(key, (int)value);
        }
        
        public static void Set(string key, byte value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.Binary);
        }

        public static void Set(string key, byte[] value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.Binary);
        }

        public static void Set(string key, long value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.QWord);
        }

        public static void Set(string key, double value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.QWord);
        }

        public static void Set(string key, string value)
        {
            Registry.SetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, value, RegistryValueKind.String);
        }

        public static int GetInt(string key)
        {
            return (int)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, 0);
        }

        public static float GetFloat(string key)
        {
            return (float)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, 0);
        }

        public static short GetShort(string key)
        {
            return (short)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, 0);
        }

        public static byte GetByte(string key)
        {
            return (byte)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, 0);
        }

        public static byte[] GetBytes(string key)
        {
            return (byte[])Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, new byte[] { });
        }

        public static long GetLong(string key)
        {
            return (long)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, 0);
        }

        public static double GetDouble(string key)
        {
            return (double)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, 0);
        }

        public static string GetString(string key)
        {
            return (string)Registry.GetValue($"{Registry.CurrentUser}\\{_REGISTRY_NAME}", key, "");
        }
    }
}
