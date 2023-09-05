using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TonSdk.Connect;

public class InjectedProvider : IInternalProvider
{
    [DllImport("__Internal")]
    public static extern bool IsWalletInjected(string jsBridgeKey);

    [DllImport("__Internal")]
    public static extern bool IsInsideWalletBrowser(string jsBridgeKey);

    [DllImport("__Internal")]
    public static extern void CallConnect(string connectRequest, int protocolVersion, string JsBridgeKey);

    [DllImport("__Internal")]
    public static extern void CallDisconnect(string JsBridgeKey);

    [DllImport("__Internal")]
    public static extern void CallListenEvents(string JsBridgeKey);

    [DllImport("__Internal")]
    public static extern void CallRestoreConnection(string JsBridgeKey, string requestId);

    [DllImport("__Internal")]
    public static extern void CallSendRequest(string request, string JsBridgeKey, string requestId);

    private bool isStarted = false;

    public RemoteStorage _storage;
    private WalletConfig? _wallet;

    private string _bridgeKey;
    private List<WalletEventListener> _listeners;

    private Dictionary<string, TaskCompletionSource<object>> _pendingRequests;

    public InjectedProvider(WalletConfig? walletConfig = null, RemoteStorage storage = null)
    {
        if(walletConfig != null && !IsWalletInjected(walletConfig?.JsBridgeKey)) throw new WalletNotInjectedError();
        if(walletConfig != null) _bridgeKey = walletConfig?.JsBridgeKey;

        _wallet = walletConfig;
        _storage = storage;
        
        _listeners = new List<WalletEventListener>();
        _pendingRequests = new Dictionary<string, TaskCompletionSource<object>>();
    }

    public void Connect(ConnectRequest connectRequest, int protocolVersion)
    {
        Console.WriteLine($"Injected Provider connect request: protocolVersion: ${protocolVersion}, message: {connectRequest}");
        CallConnect(JsonConvert.SerializeObject(connectRequest), protocolVersion, _bridgeKey);
    }

    public async Task<JObject> SendRequest(IRpcRequest request, OnRequestSentHandler onRequestSent = null)
    {
        string connectionJsonString = _storage.GetItem(RemoteStorage.KEY_CONNECTION, "{}");
        ConnectionInfo connection = JsonConvert.DeserializeObject<ConnectionInfo>(connectionJsonString);
        int id = connection.NextRpcRequestId ?? 0;
        connection.NextRpcRequestId = id + 1;

        string jsonString = JsonConvert.SerializeObject(connection);
        _storage.SetItem(RemoteStorage.KEY_CONNECTION, jsonString);

        request.id = id.ToString();

        string key = GenerateRandomString(10);
        TaskCompletionSource<object> resolve = new TaskCompletionSource<object>();
        _pendingRequests.Add(key, resolve);
        System.Console.WriteLine("Again request: " + JsonConvert.SerializeObject(request));
        CallSendRequest(JsonConvert.SerializeObject(request), _bridgeKey, key);
        JObject result = (JObject)await resolve.Task;
        return new JObject();
    }

    public void Disconnect()
    {
        CallDisconnect(_bridgeKey);
        CloseListeners();
        RemoveSession();
    }

    public async Task<bool> RestoreConnection(string key)
    {
        string id = GenerateRandomString(10);
        TaskCompletionSource<object> resolve = new TaskCompletionSource<object>();
        _pendingRequests.Add(id, resolve);
        CallRestoreConnection(key, id);
        bool result = (bool)await resolve.Task;
        _bridgeKey = key;
        
        return result;
    }

    public void ParseMessage(string message)
    {
        JObject fullData = JsonConvert.DeserializeObject<JObject>(message);

        if(fullData["type"] != null)
        {
            JObject data = JsonConvert.DeserializeObject<JObject>(fullData["data"].ToString());

            if(fullData["type"].ToString() == "restore")
            {
                string id = fullData["id"].ToString();
                if (!_pendingRequests.ContainsKey(id))
                {
                    Console.WriteLine($"Response id {id} doesn't match any request's id");
                    return;
                }

                if (data["event"] != null && (string)data["event"] == "connect")
                {
                    _pendingRequests[id].SetResult(true);
                    _pendingRequests.Remove(id);
                    
                    
                    foreach (WalletEventListener listener in _listeners)
                    {
                        listener(data);
                    }
                    ListenEvents();
                }
                else 
                {
                    _pendingRequests[id].SetResult(false);
                    _pendingRequests.Remove(id);
                    RemoveSession();
                }
                return;
            }

            if(fullData["type"].ToString() == "send")
            {
                string id = fullData["id"].ToString();
                if (!_pendingRequests.ContainsKey(id))
                {
                    Console.WriteLine($"Response id {id} doesn't match any request's id");
                    return;
                }

                _pendingRequests[id].SetResult(data);
                _pendingRequests.Remove(id);
                return;
            }

            if (data["event"] != null)
            {
                if (data["event"].ToString() == "connect")
                {
                    Console.WriteLine("Connected.");
                    foreach (WalletEventListener listener in _listeners)
                    {
                        listener(data);
                    }
                    UpdateSession();
                    ListenEvents();
                }
                else if(data["event"].ToString() == "disconnect")
                {
                    foreach (WalletEventListener listener in _listeners)
                    {
                        listener(data);
                    }
                    RemoveSession();
                    Console.WriteLine("disconnected.");
                }
            }  
        }
        return;
    }

    public void CloseConnection()
    {
        if(isStarted) CallDisconnect(_bridgeKey);
        CloseListeners();
    }
    
    public void Listen(WalletEventListener listener) => _listeners.Add(listener);

    private void UpdateSession()
    {
        ConnectionInfo connection = new ConnectionInfo();
        connection.Type = "injected";
        connection.JsBridgeKey = _bridgeKey;
        connection.NextRpcRequestId = 0;

        string jsonString = JsonConvert.SerializeObject(connection);
        _storage.SetItem(RemoteStorage.KEY_CONNECTION, jsonString);
    }

    private void RemoveSession()
    {
        _storage.RemoveItem(RemoteStorage.KEY_CONNECTION);
        _storage.RemoveItem(RemoteStorage.KEY_LAST_EVENT_ID);
    }

    private void CloseListeners()
    {
        isStarted = false;
        _listeners.Clear();
    }
    private void ListenEvents()
    {
        isStarted = true;
        System.Console.WriteLine("JsBridgeKey: " + _bridgeKey);
        CallListenEvents(_bridgeKey);
    }

    private static string GenerateRandomString(int length)
    {
        const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();
        return new string(Enumerable.Repeat(characters, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
