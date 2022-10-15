using System;
using NBitcoin;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using KeyPath = NBitcoin.KeyPath;

var k = ExtKey.CreateFromSeed(Convert.FromHexString("f8b7cb3a352673be81251fcd04e9e451f7fcafe5605ec8104d86848072c95918ec033e781514c7d70383ffe698826fb316527137074a4bf4fa6263cec3b7466a"));
k = k.Derive(KeyPath.Parse("m/44'/60'/0'/0/1"));

var privateKey = new EthECKey(k.PrivateKey.ToHex());
var account = new Account(privateKey);

Console.WriteLine(privateKey.GetPrivateKeyAsBytes().ToHex(true));
Console.WriteLine(privateKey.GetPubKey(true).ToHex(true));
Console.WriteLine(account.Address);
