using UnityEngine;

public class Patch
{
    public static readonly int NORMAL_PATCH_SIZE = 16;
    public static readonly int LARGE_PATCH_SIZE = 32;

    protected static int CurrentDecompressorSize;
    protected static float[] PatchDequantizeTable = new float [LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];
    protected static float[] PatchICosines        = new float [LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];
    protected static int[]   DeCopyMatrix         = new int   [LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];

    protected static GroupHeader GroupHeader;

    public static void InitPatchDecompressor (int size)
    {
        if (size == CurrentDecompressorSize)
        {
            return;
        }

        CurrentDecompressorSize= size;
        BuildPatchDequantizeTable (size);
        SetupPatchICosines (size);
        BuildDeCopyMatrix (size);
    }

    protected static void BuildPatchDequantizeTable (int size)
    {
        int j;
        for (j = 0; j < size; j++)
        {
            int i;
            for (i = 0; i < size; i++)
            {
                PatchDequantizeTable[j * size + i] = (1f + 2f * (i + j));
            }
        }
    }

    protected static void SetupPatchICosines(int size)
    {
        int u;
        float oosob = Mathf.PI * 0.5f / size;

        for (u = 0; u < size; u++)
        {
            int n;
            for (n = 0; n < size; n++)
            {
                PatchICosines[u * size + n] = Mathf.Cos((2f * n + 1f) * u * oosob);
            }
        }
    }

    protected static void BuildDeCopyMatrix(int size)
    {
        bool diagonal = false;
        bool right = true;

        int i = 0;
        int j = 0;
        int count = 0;

        while ((i < size) && (j < size))
        {
            DeCopyMatrix[j * size + i] = count;
            count++;


            if (!diagonal)
            {
                if (right)
                {
                    if (i < size - 1)
                    {
                        i++;
                    }
                    else
                    {
                        j++;
                    }
                    right = false;
                    diagonal = true;
                }
                else
                {
                    if (j < size - 1)
                    {
                        j++;
                    }
                    else
                    {
                        i++;
                    }
                    right = true;
                    diagonal = true;
                }
            }
            else
            {
                if (right)
                {
                    i++;
                    j--;
                    if ((i == size - 1) || (j == 0))
                    {
                        diagonal = false;
                    }
                }
                else
                {
                    i--;
                    j++;
                    if ((i == 0) || (j == size - 1))
                    {
                        diagonal = false;
                    }
                }
            }
        }
    }

    public static void SetGroupHeader (GroupHeader groupHeader)
    {
        GroupHeader = groupHeader;
    }

    /// <summary>
    /// Expands compressed patch data into the given array of int values
    ///
    /// If the value starts with bits   0: The value is zero
    /// If the value starts with bits  00: The rest of the patch is zeroes
    /// If the value starts with bits 000: The value is a positive integer of wordBits bits
    /// If the value starts with bits 001: The value is a negative integer of wordBits bits
    /// 
    /// </summary>
    /// <param name="bitPack">The bitPack to read data from</param>
    /// <param name="patchSize">The number of values in the patch is patchSize * patchSize</param>
    /// <param name="wordBits">The number of bits in each value</param>
    /// <param name="patchData">Array to place the decoded values in</param>
    public static void Decode (BitPack bitPack, int patchSize, int wordBits, int[] patchData)
    {
        Logger.LogDebug($"Patch.Decode: patchSize={patchSize}, wordBits={wordBits}");
        // TODO: Different for Big Endian?
        int i;
        int j;
        for (i = 0; i < patchSize * patchSize; i++)
        {
            if (bitPack.GetBool() == false)
            {
                patchData[i] = 0;
                continue;
            }

            if (bitPack.GetBool() == false)
            {
                for (j = i; j < patchSize * patchSize; j++)
                {
                    patchData[j] = 0;
                }

                return;
            }

            bool isNegative = bitPack.GetBool();
            patchData[i] = (int)bitPack.GetUInt32_Le(wordBits) * (isNegative ? -1 : 1);
        }
    }

}
