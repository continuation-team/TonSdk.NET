---
description: TonSdk.Core.Address
---

# Address

To create Address instance you can use constructor with different overloads:

<pre class="language-csharp"><code class="lang-csharp"><strong>string addrStr = ""; // base 64 or raw address string representation
</strong>int workchain = 0; // address workchain
byte[] hash = /* 32 bytes of the address hash value */
StateInit stateInit = /* address state init */

<strong>// Initializes a new instance of the Address class based on a string representation of the address
</strong>Address addr1 = new Address(addrStr);

// Initializes a new instance of the Address class based on an existing address.
Address addr2 = new Address(addr1);

// Initializes a new instance of the Address class based on address workchain and hash.
Address addr3 = new Address(workchain, hash);

// Initializes a new instance of the Address class based on address workchain and state init.
Address addr4 = new Address(workchain, stateInit);
</code></pre>
