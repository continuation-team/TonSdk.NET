using System;
using System.Threading.Tasks;
using TonSdk.Client;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;
using TonSdk.DeFi.DeDust.Vault;

namespace TonSdk.DeFi.DeDust
{
    public class DeDustFactory
{
    private readonly Address _address;

    protected DeDustFactory(Address address)
    {
        _address = address;
    }
    
    public Address Address => _address;
    
    public static DeDustFactory CreateFromAddress(Address address) 
        => new DeDustFactory(address);
    
    private async Task<Address> GetPoolAddress(TonClient client, DeDustPoolType type, DeDustAsset[] assets)
    {
        if (assets.Length != 2)
            throw new ArgumentException("Assets array length must be equal to 2.");

        try
        {
            var result = await client.RunGetMethod(
                _address,
                "get_pool_address",
                new IStackItem[]
                {
                    new VmStackInt((int)type),
                    new VmStackSlice(assets[0].ToCell().Parse()),
                    new VmStackSlice(assets[1].ToCell().Parse())
                });
            return ((Cell)result.Value.Stack[0]).Parse().ReadAddress();
        }
        catch
        {
            return await GetPoolAddress(client, type, assets);
        }
    }

    public async Task<DeDustPool> GetPool(TonClient client, DeDustPoolType type, DeDustAsset[] assets)
    {
        var poolAddress = await GetPoolAddress(client, type, assets);
        return DeDustPool.CreateFromAddress(poolAddress);
    }
    
    private async Task<Address> GetVaultAddress(TonClient client, DeDustAsset asset)
    {
        try
        {
            var result = await client.RunGetMethod(
                _address,
                "get_vault_address",
                new IStackItem[]
                {
                    new VmStackSlice()
                    {
                        Value = asset.ToCell().Parse()
                    } 
                });
            return ((Cell)result.Value.Stack[0]).Parse().ReadAddress();
        }
        catch (Exception e)
        {
            return await GetVaultAddress(client, asset);
        }
    }
    
    public async Task<DeDustNativeVault> GetNativeVault(TonClient client)
    {
        var nativeVaultAddress = await GetVaultAddress(client, DeDustAsset.Native());
        return DeDustNativeVault.CreateFromAddress(nativeVaultAddress);
    }
    
    public async Task<DeDustJettonVault> GetJettonVault(TonClient client, Address address)
    {
        var jettonVaultAddress = await GetVaultAddress(client, DeDustAsset.Jetton(address));
        return DeDustJettonVault.CreateFromAddress(jettonVaultAddress);
    }
}
}