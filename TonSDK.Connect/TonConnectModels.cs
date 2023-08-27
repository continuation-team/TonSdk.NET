using System;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TonSdk.Connect
{

    public struct Wallet
    {
        public DeviceInfo Device { get; set; }
        public string Provider { get; set; }
        public Account Account { get; set; }
        public TonProof TonProof { get; set; }
    }

    public class DeviceInfo
    {
        public string? Platform { get; set; }
        public string? AppName { get; set; }
        public string? AppVersion { get; set; }
        public int MaxProtocolVersion { get; set; }
        public object[]? Features { get; set; }

        public static DeviceInfo Parse(dynamic device)
        {
            DeviceInfo deviceInfo = new DeviceInfo()
            {
                Platform = (string)device.platform,
                AppName = (string)device.appName,
                AppVersion = (string)device.appVersion,
                MaxProtocolVersion = (int)device.maxProtocolVersion,
                Features = device.features.ToObject<object[]>()
            };
            return deviceInfo;
        }
    }

    public class Account
    {
        public Address? Address { get; set; }
        public CHAIN Chain { get; set; }
        public string? WalletStateInit { get; set; }
        public string? PublicKey { get; set; }

        public static Account Parse(dynamic item)
        {
            if (item.address == null) throw new TonConnectError("address not contains in ton_addr");

            Account account = new Account()
            {
                Address = new Address(item.address.ToString()),
                Chain = (CHAIN)(int)item.network,
                WalletStateInit = item.walletStateInit.ToString(),
                PublicKey = item.publicKey?.ToString()
            };
            return account;
        }
    }

    public class TonProof
    {
        public uint Timestamp { get; set; }
        public int DomainLen { get; set; }
        public string? DomainVal { get; set; }
        public string? Payload { get; set; }
        public byte[]? Signature { get; set; }

        public static TonProof Parse(dynamic item)
        {
            if (item.proof == null) throw new TonConnectError("proof not contains in ton_proof");

            dynamic proof = item.proof;

            TonProof tonProof = new TonProof()
            {
                Timestamp = (uint)proof.timestamp,
                DomainLen = (int)proof.domain.lengthBytes,
                DomainVal = (string)proof.domain.value,
                Payload = (string)proof.payload,
                Signature = Convert.FromBase64String((string)proof.signature)
            };
            return tonProof;
        }
    }

    public class SendTransactionRequest
    {
        /// <summary>
        /// Sending transaction deadline in unix epoch seconds.
        /// </summary>
        public long? ValidUntil { get; set; }

        /// <summary>
        /// The network (mainnet or testnet) where DApp intends to send the transaction. If not set, the transaction is sent to the network currently set in the wallet, but this is not safe and DApp should always strive to set the network. If the network parameter is set, but the wallet has a different network set, the wallet should show an alert and DO NOT ALLOW TO SEND this transaction.
        /// </summary>
        public CHAIN? Network { get; set; }

        /// <summary>
        /// The sender address in wc:hex format from which DApp intends to send the transaction. Current account.address by default.
        /// </summary>
        public Address? From { get; set; }

        /// <summary>
        /// Messages to send: min is 1, max is 4.
        /// </summary>
        public Message[] Messages { get; set; }

        public SendTransactionRequest(Message[] messages, long? validUntil = null, CHAIN? network = null, Address? from = null)
        {
            Messages = messages;
            ValidUntil = validUntil;
            Network = network;
            From = from;
        }
    }

    public struct Message
    {
        /// <summary>
        /// Receiver's address.
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Amount to send in nanoTon.
        /// </summary>
        public Coins Amount { get; set; }

        /// <summary>
        /// Contract specific data to add to the transaction.
        /// </summary>
        public Cell? StateInit { get; set; }

        /// <summary>
        /// Contract specific data to add to the transaction.
        /// </summary>
        public Cell? Payload { get; set; }

        public Message(Address receiver, Coins amount, Cell? stateInit = null, Cell? payload = null)
        {
            Address = receiver;
            Amount = amount;
            Payload = payload;
            StateInit = stateInit;
        }
    }

    public struct SendTransactionResult
    {
        public Cell Boc { get; set; }
    }
}