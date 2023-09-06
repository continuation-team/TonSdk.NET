using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace TonSdk.Connect
{
    public class DefaultStorage
    {
        public static readonly string KEY_LAST_EVENT_ID = "last_event_id";
        public static readonly string KEY_CONNECTION = "connection";
        public static readonly string STORAGE_PREFIX = "sharptonconnect_";

        private static IsolatedStorageFile GetIsolatedStorage()
        {
            return IsolatedStorageFile.GetUserStoreForAssembly();
        }

        public static async Task SetItem(string key, string value)
        {
            using IsolatedStorageFile isolatedStorage = GetIsolatedStorage();
            string storageKey = GetStorageKey(key);
            using StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(storageKey, FileMode.Create, isolatedStorage));
            await writer.WriteAsync(value);
        }

        public static async Task<string?> GetItem(string key, string? defaultValue = null)
        {
            using IsolatedStorageFile isolatedStorage = GetIsolatedStorage();
            string storageKey = GetStorageKey(key);
            if (isolatedStorage.FileExists(storageKey))
            {
                using StreamReader reader = new StreamReader(new IsolatedStorageFileStream(storageKey, FileMode.Open, isolatedStorage));
                return await reader.ReadToEndAsync();
            }
            return defaultValue;
        }

        public static void RemoveItem(string key)
        {
            using IsolatedStorageFile isolatedStorage = GetIsolatedStorage();
            string storageKey = GetStorageKey(key);
            if (isolatedStorage.FileExists(storageKey))
            {
                isolatedStorage.DeleteFile(storageKey);
            }
        }

        private static string GetStorageKey(string key)
        {
            return STORAGE_PREFIX + key;
        }
    }
}