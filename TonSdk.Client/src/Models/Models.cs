using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Client
{
    public enum TonClientType
    {
        HTTP_TONCENTERAPIV2,
        HTTP_TONCENTERAPIV3,
        HTTP_TONWHALESAPI,
        LITECLIENT,
        
    }
    
    public enum ProxyType
    {
        Socks5,
        Socks4,
        HTTP,
        HTTPS
    }
    
    public interface ITonClientOptions {}

    public class HttpParameters : ITonClientOptions
    {
        public string Endpoint { get; set; }
        public int? Timeout { get; set; }
        public string ApiKey { get; set; }
        public Proxy? Proxy { get; set; }
    }
    
    public class LiteClientParameters : ITonClientOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string PeerPublicKey { get; set; }

        public LiteClientParameters(string host, int port, string peerPublicKey)
        {
            Host = host;
            Port = port;
            PeerPublicKey = peerPublicKey;
        }
    }
    
    public partial class Proxy : ITonClientOptions
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ProxyType ProxyType { get; set; }
    }
}