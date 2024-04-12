---
description: TonSdk.Core.Address
---

# Address

To create Address instance you can use constructor with different overloads:

<pre class="language-csharp"><code class="lang-csharp"><strong>string addrStr = ""; // base 64 or raw address string representation
</strong>int workchain = 0; // address workchain
byte[] hash = /* 32 bytes of the address hash value */;
StateInit stateInit = /* address state init */;

<strong>// Initializes a new instance of the Address class based on a string representation of the address
</strong>Address addr1 = new Address(addrStr);

// Initializes a new instance of the Address class based on an existing address.
Address addr2 = new Address(addr1);

// Initializes a new instance of the Address class based on address workchain and hash.
Address addr3 = new Address(workchain, hash);

// Initializes a new instance of the Address class based on address workchain and state init.
Address addr4 = new Address(workchain, stateInit);
</code></pre>



You can use `ToString()` method with different parameters to convert Address instance into string representation:

<pre class="language-csharp" data-full-width="false"><code class="lang-csharp">Address address = /* address instance */;

// ex, 0:83...a8
string raw = address.ToString(AddressType.Raw);

// ex, EQC...2N
<strong>string base64Bounceable = address.ToString(AddressType.Base64, new AddressStringifyOptions(true, false, false);
</strong><strong>
</strong><strong>// ex, UQC...BI
</strong>string base64NonBounceable = address.ToString(AddressType.Base64, new AddressStringifyOptions(false, false, false);

// ex, kQC...YH
string base64BounceableTestOnly = address.ToString(AddressType.Base64, new AddressStringifyOptions(true, true, false);

// ex, 0QC...vC
string base64NonBounceableTestOnly = address.ToString(AddressType.Base64, new AddressStringifyOptions(false, true, false);
</code></pre>
