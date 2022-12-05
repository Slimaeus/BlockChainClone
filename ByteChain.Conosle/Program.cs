// See https://aka.ms/new-console-template for more information
using ByteChain.Core.Models;


var rand = new Random();
IBlock genesis = new Block(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 });
byte[] difficult = new byte[] { 0x00, 0x00 };
BlockChain chain = new BlockChain(difficult, genesis);
for (int i = 0; i < 200; i++)
{
    var data = Enumerable.Range(0, 255).Select(p => (byte)rand.Next());
    chain.Add(new Block(data.ToArray()));
    Console.WriteLine(chain.LastOrDefault()?.ToString());
    if (chain.IsValid())
        Console.WriteLine("BlockChain is Valid");
}
Console.ReadLine();