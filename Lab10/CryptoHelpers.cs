using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Lab10;

internal static class CryptoHelpers
{
    private static readonly byte[] SBox = BuildSBox();

    public static NewDesResult RunNewDes(string text, string key, CancellationToken token, IProgress<int> progress)
    {
        var source = Encoding.UTF8.GetBytes(text);
        var keyBytes = DeriveNewDesKey(key);
        var padded = Pad(source);
        var encrypted = new byte[padded.Length];
        var decrypted = new byte[padded.Length];
        var roundKeys = BuildRoundKeys(keyBytes);
        var started = DateTime.Now;

        for (var i = 0; i < padded.Length; i += 8)
        {
            token.ThrowIfCancellationRequested();
            var block = padded.Skip(i).Take(8).ToArray();
            EncryptBlock(block, roundKeys);
            Array.Copy(block, 0, encrypted, i, 8);
            progress.Report((i + 8) * 45 / padded.Length);
            Thread.Sleep(18);
        }

        for (var i = 0; i < encrypted.Length; i += 8)
        {
            token.ThrowIfCancellationRequested();
            var block = encrypted.Skip(i).Take(8).ToArray();
            DecryptBlock(block, roundKeys);
            Array.Copy(block, 0, decrypted, i, 8);
            progress.Report(45 + (i + 8) * 45 / encrypted.Length);
            Thread.Sleep(18);
        }

        progress.Report(100);
        return new NewDesResult(
            Convert.ToHexString(keyBytes),
            Convert.ToHexString(encrypted),
            Encoding.UTF8.GetString(Unpad(decrypted)),
            padded.Length / 8,
            Environment.CurrentManagedThreadId,
            DateTime.Now - started);
    }

    public static HashResult RunMd5(string text, CancellationToken token, IProgress<int> progress)
    {
        var started = DateTime.Now;

        for (var i = 1; i <= 12; i++)
        {
            token.ThrowIfCancellationRequested();
            progress.Report(i * 7);
            Thread.Sleep(28);
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = MD5.HashData(bytes);
        progress.Report(100);
        return new HashResult(Convert.ToHexString(hash).ToLowerInvariant(), bytes.Length, Environment.CurrentManagedThreadId, DateTime.Now - started);
    }

    public static ElGamalResult RunElGamal(string text, CancellationToken token, IProgress<int> progress)
    {
        var started = DateTime.Now;
        var p = new BigInteger(2147483647);
        var g = new BigInteger(5);
        var privateKey = RandomNumberGenerator.GetInt32(2, int.MaxValue - 2);
        var publicKey = BigInteger.ModPow(g, privateKey, p);
        var bytes = Encoding.UTF8.GetBytes(text);
        var encrypted = new List<(BigInteger C1, BigInteger C2)>();
        var decrypted = new byte[bytes.Length];

        for (var i = 0; i < bytes.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            var k = RandomNumberGenerator.GetInt32(2, int.MaxValue - 2);
            var c1 = BigInteger.ModPow(g, k, p);
            var shared = BigInteger.ModPow(publicKey, k, p);
            var c2 = bytes[i] * shared % p;
            encrypted.Add((c1, c2));
            progress.Report(bytes.Length == 0 ? 50 : (i + 1) * 45 / bytes.Length);
            Thread.Sleep(12);
        }

        for (var i = 0; i < encrypted.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var inverse = BigInteger.ModPow(encrypted[i].C1, p - 1 - privateKey, p);
            decrypted[i] = (byte)(encrypted[i].C2 * inverse % p);
            progress.Report(45 + (i + 1) * 45 / encrypted.Count);
            Thread.Sleep(12);
        }

        progress.Report(100);
        var cipher = string.Join(" ", encrypted.Select(pair => $"{pair.C1}:{pair.C2}"));
        return new ElGamalResult(
            p.ToString(),
            g.ToString(),
            privateKey.ToString(),
            publicKey.ToString(),
            cipher,
            Encoding.UTF8.GetString(decrypted),
            Environment.CurrentManagedThreadId,
            DateTime.Now - started);
    }

    private static byte[] DeriveNewDesKey(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return hash.Take(15).ToArray();
    }

    private static byte[] Pad(byte[] data)
    {
        var add = 8 - data.Length % 8;
        var result = new byte[data.Length + add];
        Array.Copy(data, result, data.Length);
        Array.Fill(result, (byte)add, data.Length, add);
        return result;
    }

    private static byte[] Unpad(byte[] data)
    {
        if (data.Length == 0)
        {
            return data;
        }

        var add = data[^1];

        if (add <= 0 || add > 8 || add > data.Length)
        {
            return data;
        }

        return data.Take(data.Length - add).ToArray();
    }

    private static byte[][] BuildRoundKeys(byte[] key)
    {
        var keys = new byte[17][];

        for (var round = 0; round < keys.Length; round++)
        {
            keys[round] = new byte[7];
            var shift = round / 2 * 7;

            for (var i = 0; i < 7; i++)
            {
                keys[round][i] = key[(i + shift) % key.Length];
            }
        }

        return keys;
    }

    private static void EncryptBlock(byte[] block, byte[][] keys)
    {
        foreach (var key in keys)
        {
            block[4] ^= F(block[0], key[0]);
            block[5] ^= F(block[1], key[1]);
            block[6] ^= F(block[2], key[2]);
            block[7] ^= F(block[3], key[3]);
            block[1] ^= F(block[4], key[4]);
            block[2] ^= F(block[5], key[5]);
            block[3] ^= F(block[6], key[6]);
            block[0] ^= F(block[7], (byte)(key[0] ^ key[6]));
        }
    }

    private static void DecryptBlock(byte[] block, byte[][] keys)
    {
        for (var round = keys.Length - 1; round >= 0; round--)
        {
            var key = keys[round];
            block[0] ^= F(block[7], (byte)(key[0] ^ key[6]));
            block[3] ^= F(block[6], key[6]);
            block[2] ^= F(block[5], key[5]);
            block[1] ^= F(block[4], key[4]);
            block[7] ^= F(block[3], key[3]);
            block[6] ^= F(block[2], key[2]);
            block[5] ^= F(block[1], key[1]);
            block[4] ^= F(block[0], key[0]);
        }
    }

    private static byte F(byte value, byte key)
    {
        return SBox[value ^ key];
    }

    private static byte[] BuildSBox()
    {
        var values = Enumerable.Range(0, 256).Select(number => (byte)number).ToArray();
        var seed = Encoding.UTF8.GetBytes("When in the Course of human events NewDES educational s-box");
        var state = SHA256.HashData(seed);
        var index = 0;

        for (var i = values.Length - 1; i > 0; i--)
        {
            if (index >= state.Length)
            {
                state = SHA256.HashData(state);
                index = 0;
            }

            var j = state[index++] % (i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }
}

internal sealed record NewDesResult(string KeyHex, string CipherHex, string PlainText, int Blocks, int ThreadId, TimeSpan Duration);
internal sealed record HashResult(string HashHex, int Bytes, int ThreadId, TimeSpan Duration);
internal sealed record ElGamalResult(string P, string G, string PrivateKey, string PublicKey, string CipherText, string PlainText, int ThreadId, TimeSpan Duration);
