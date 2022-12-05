using System.Collections;
using System.Security.Cryptography;

namespace ByteChain.Core.Models
{
    public interface IBlock
    {
        public byte[] Data { get; }
        public byte[] Hash { get; set; }
        public int Nonce { get; set; }
        public byte[] PreviousHash { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class Block : IBlock
    {
        public byte[] Data { get; }
        public byte[] Hash { get; set; }
        public int Nonce { get; set; }
        public byte[] PreviousHash { get; set; }
        public DateTime TimeStamp { get; set; }

        public Block(byte[] data) {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Nonce = 0;
            PreviousHash = new byte[] { 0x00 };
            TimeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{BitConverter.ToString(Hash).Replace("-", "")} :\n{BitConverter.ToString(PreviousHash).Replace("-", "")}\n{Nonce} {TimeStamp}";
        }
    }

    public static class BlockExtension
    {
        public static byte[] GenerateHash(this IBlock block)
        {
            using var sha = SHA512.Create();
            using var st = new MemoryStream();
            using var br = new BinaryWriter(st);
            br.Write(block.Data);
            br.Write(block.Nonce);
            br.Write(block.PreviousHash);
            br.Write(block.TimeStamp.ToString());
            var s = st.ToArray();
            return sha.ComputeHash(s);
        }

        public static byte[] MineHash(this IBlock block, byte[] difficult)
        {
            if (difficult == null) throw new ArgumentNullException(nameof(difficult));
            byte[] hash = new byte[0];
            while (!hash.Take(2).SequenceEqual(difficult))
            {
                block.Nonce++;
                hash = block.GenerateHash();
            }
            return hash;
        }

        public static bool IsValid(this IBlock block)
        {
            var bk = block.GenerateHash();
            return block.Hash.SequenceEqual(bk);
        }

        public static bool IsPreviousBlock(this IBlock block, IBlock previousBlock)
        {
            if (previousBlock == null) throw new ArgumentNullException();
            return previousBlock.IsValid() && block.PreviousHash.SequenceEqual(previousBlock.Hash);
        }

        public static bool IsValid(this IEnumerable<IBlock> items)
        {
            var enums = items.ToList();
            return enums.Zip(enums.Skip(1), Tuple.Create).All(block => block.Item2.IsValid() && block.Item2.IsPreviousBlock(block.Item1));
        }
    }

    public class BlockChain : IEnumerable<IBlock>
    {
        private List<IBlock> _items = new List<IBlock>();

        public BlockChain(byte[] difficult, IBlock genesis)
        {
            Difficult = difficult;
            genesis.Hash = genesis.MineHash(difficult);
            Items.Add(genesis);
        }

        public byte[] Difficult { get; }

        public void Add(IBlock item)
        {
            if (_items.LastOrDefault() != null)
            {
                item.PreviousHash = _items.LastOrDefault().Hash;
            }
            item.Hash = item.MineHash(Difficult);
            Items.Add(item);
        }

        public List<IBlock> Items
        {
            get => _items;
            set => _items = value;
        }

        public int Count => _items.Count;

        public IBlock this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
