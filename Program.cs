using System;
using System.IO;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string filePath = "ファイルパス～sample.txt";
        uint totalSum = 0; // 符号なし変数で合計を保持します

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int byteValue;

                while ((byteValue = fileStream.ReadByte()) != -1)
                {
                    byte byteData = (byte)byteValue;
                    totalSum += byteData;
                }
            }
       
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var crc32 = new CRC32())
                {
                    byte[] buffer = new byte[4096]; // ファイルの読み込みバッファ
                    int bytesRead;

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        crc32.Update(buffer, 0, bytesRead); // バッファのデータをチェックサムに追加
                    }

                    uint checksum = crc32.GetChecksum(); // チェックサムを取得

                    Console.WriteLine("sum: " + totalSum + " (" + checksum.ToString("X8") + ")"); // 16進数表記で表示
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("指定されたファイルが見つかりませんでした。");
        }
        catch (Exception ex)
        {
            Console.WriteLine("エラーが発生しました: " + ex.Message);
        }
    }
}

public class CRC32 : HashAlgorithm
{
    private static readonly uint[] crcTable;

    private uint crc;

    static CRC32()
    {
        crcTable = new uint[256];

        const uint poly = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;

            for (int j = 0; j < 8; j++)
            {
                crc = (crc & 1) == 1 ? (crc >> 1) ^ poly : crc >> 1;
            }

            crcTable[i] = crc;
        }
    }

    public CRC32()
    {
        crc = 0xFFFFFFFF;
        HashSizeValue = 32;
    }

    public void Update(byte[] buffer, int offset, int count)
    {
        HashCore(buffer, offset, count);
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        for (int i = ibStart; i < ibStart + cbSize; i++)
        {
            crc = crcTable[(crc ^ array[i]) & 0xFF] ^ (crc >> 8);
        }
    }

    protected override byte[] HashFinal()
    {
        crc ^= 0xFFFFFFFF;
        byte[] hashBytes = BitConverter.GetBytes(crc);
        Array.Reverse(hashBytes);
        return hashBytes;
    }

    public override void Initialize()
    {
        crc = 0xFFFFFFFF;
    }

    public uint GetChecksum()
    {
        byte[] hashBytes = HashFinal();
        return BitConverter.ToUInt32(hashBytes, 0);
    }
}
