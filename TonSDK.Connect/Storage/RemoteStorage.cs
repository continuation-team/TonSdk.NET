using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonSdk.Connect
{
    public delegate string RemoteStorageGetItem(string key, string defaultValue);
    public delegate void RemoteStorageSetItem(string key, string value);
    public delegate bool RemoteStorageHasItem(string key);
    public delegate void RemoteStorageRemoveItem(string key);

    public class RemoteStorage
    {
        public static readonly string KEY_LAST_EVENT_ID = "last_event_id";
        public static readonly string KEY_CONNECTION = "connection";
        public static readonly string STORAGE_PREFIX = "sharptonconnect_";

        private RemoteStorageGetItem _remoteStorageGetItem;
        private RemoteStorageSetItem _remoteStorageSetItem;
        private RemoteStorageRemoveItem _remoteStorageRemoveItem;
        private RemoteStorageHasItem _remoteStorageHasItem;

        public RemoteStorage(RemoteStorageGetItem getItem, RemoteStorageSetItem setItem, RemoteStorageRemoveItem removeItem, RemoteStorageHasItem hasItem)
        {
            _remoteStorageGetItem = getItem;
            _remoteStorageSetItem = setItem;
            _remoteStorageRemoveItem = removeItem;
            _remoteStorageHasItem = hasItem;
        }

        public void SetItem(string key, string value)
        {
            string storageKey = GetStorageKey(key);
            _remoteStorageSetItem(storageKey, value);
        }

        public string GetItem(string key, string defaultValue = null)
        {
            string storageKey = GetStorageKey(key);
            return _remoteStorageGetItem(storageKey, defaultValue);
        }

        public void RemoveItem(string key)
        {
            string storageKey = GetStorageKey(key);
            if (_remoteStorageHasItem(storageKey)) _remoteStorageRemoveItem(storageKey);
        }

        private static string GetStorageKey(string key)
        {
            return STORAGE_PREFIX + key;
        }
    }
}
