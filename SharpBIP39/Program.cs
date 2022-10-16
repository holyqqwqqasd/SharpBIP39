using System;
using Nethereum.HdWallet;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
// using NBitcoin;
// using KeyPath = NBitcoin.KeyPath;

// const uint WDA_NONE = 0x00;
// const uint WDA_MONITOR = 0x01;
// const uint WDA_EXCLUDEFROMCAPTURE = 0x11;

// [DllImport("user32.dll")]
// public static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

string Words = "meat brand film page shaft mad output menu bonus stereo record install shine orphan involve borrow recall exotic mountain family hood oval critic initial";
string Password1 = "password";
var wallet1 = new Wallet(Words, Password1);
for (int i = 0; i < 10; i++)
{
    var account = wallet1.GetAccount(i); 
    Console.WriteLine("Account index : " + i +" - Address : " + account.Address + " - Private key : " + account.PrivateKey);
}

// var k = ExtKey.CreateFromSeed(Convert.FromHexString("f8b7cb3a352673be81251fcd04e9e451f7fcafe5605ec8104d86848072c95918ec033e781514c7d70383ffe698826fb316527137074a4bf4fa6263cec3b7466a"));
// k = k.Derive(KeyPath.Parse("m/44'/60'/0'/0/1"));

// var privateKey = new EthECKey(k.PrivateKey.ToHex());
// var account = new Account(privateKey);

// Console.WriteLine(privateKey.GetPrivateKeyAsBytes().ToHex(true));
// Console.WriteLine(privateKey.GetPubKey(true).ToHex(true));
// Console.WriteLine(account.Address);
