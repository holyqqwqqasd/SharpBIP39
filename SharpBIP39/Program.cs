using NBitcoin;
using Bitcoin.BIP39;
using System;

var bip39 = new BIP39("elevator dinosaur switch you armor vote black syrup fork onion nurse illegal trim rocket combine", "", BIP39.Language.English);

Console.WriteLine(bip39.MnemonicSentence);
Console.WriteLine(bip39.SeedBytesHexString);

var masterKey = ExtKey.CreateFromSeed(bip39.SeedBytes).Derive(KeyPath.Parse("m/44'/60'/0'/0/0"));
var pubKey44 = masterKey.Neuter();

Console.WriteLine(masterKey.PrivateKey.PubKey.ToString());
Console.WriteLine(masterKey.GetWif(Network.Main));
