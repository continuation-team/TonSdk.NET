using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using TonSdk.Core.Boc;

namespace TonSdk.Client.Stack
{
    [Serializable]
    public class Stack
    {
        [JsonProperty("stack_items")] public List<IStackItem> StackItems;

        public Stack(IStackItem[] stackItems)
        {
            StackItems = stackItems.ToList();
        }
    }

    public interface IStackItem
    {
    }

    [Serializable]
    public struct VmStackNull : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackNull";
    }

    [Serializable]
    public struct VmStackTinyInt : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackTinyInt";
        [JsonProperty("value")] public long Value { get; set; }
    }

    [Serializable]
    public struct VmStackInt : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackInt";
        [JsonProperty("value")] public BigInteger Value { get; set; }
    }

    [Serializable]
    public struct VmStackCell : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackCell";
        [JsonProperty("cell")] public Cell Value { get; set; }
    }

    [Serializable]
    public struct VmStackSlice : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackSlice";
        [JsonProperty("slice")] public CellSlice Value { get; set; }
    }

    [Serializable]
    public struct VmStackBuilder : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackBuilder";
        [JsonProperty("builder")] public CellBuilder Value { get; set; }
    }

    [Serializable]
    public struct VmStackTuple : IStackItem
    {
        [JsonProperty("key")] public const string Key = "VmStackTuple";
        [JsonProperty("tuple")] public IStackItem[] Value { get; set; }
    }
}