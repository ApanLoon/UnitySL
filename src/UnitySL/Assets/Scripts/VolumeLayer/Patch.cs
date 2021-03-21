
#define _PATCH_SIZE_16_AND_32_ONLY

using UnityEngine;

public class Patch
{
    public static readonly int NORMAL_PATCH_SIZE = 16;
    public static readonly int LARGE_PATCH_SIZE = 32;

    //protected static readonly float SQRT2 = Mathf.Sqrt (2);
    protected static readonly float OO_SQRT2 = 1f / Mathf.Sqrt(2); //0.7071067811865475244008443621049f;

    protected static int CurrentDecompressorSize;
    protected static float[] PatchDequantizeTable = new float [LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];
    protected static float[] PatchICosines        = new float [LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];
    protected static int[]   DeCopyMatrix         = new int   [LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];

    /// <summary>
    /// Use ONLY for unit testing!
    /// </summary>
    public static float[] _PatchDequantizeTable => PatchDequantizeTable;
    /// <summary>
    /// Use ONLY for unit testing!
    /// </summary>
    public static float[] _PatchICosines => PatchICosines;
    /// <summary>
    /// Use ONLY for unit testing!
    /// </summary>
    public static int[] _DeCopyMatrix => DeCopyMatrix;

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
        //Logger.LogDebug($"Patch.Decode: patchSize={patchSize}, wordBits={wordBits}");

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

    public static void DeCompress (float[] dest, uint destOffset, int[] src, PatchHeader patchHeader)
    {
        {
            int i;
            int j;

            float[] block = new float[LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];
            int tblockIndex = 0;

            int tpatchIndex;

            GroupHeader groupHeader = GroupHeader;
            int size = groupHeader.PatchSize;
            float range = patchHeader.Range;
            int prequant = (patchHeader.QuantWBits >> 4) + 2;
            int quantize = 1 << prequant;
            float hmin = patchHeader.DcOffset;
            int stride = groupHeader.Stride;

            float ooq = 1f / (float) quantize;
            int dqIndex = 0;
            int decopyMatrixIndex = 0;

            float mult = ooq * range;
            float addval = mult * (float) (1 << (prequant - 1)) + hmin;

            for (i = 0; i < size * size; i++)
            {
                block[tblockIndex++] = src[DeCopyMatrix[decopyMatrixIndex++]] * PatchDequantizeTable[dqIndex++];
            }

            if (size == 16)
            {
                idct_patch(block);
            }
            else
            {
                idct_patch_large(block);
            }

            for (j = 0; j < size; j++)
            {
                tpatchIndex = j * stride;
                tblockIndex = j * size;
                for (i = 0; i < size; i++)
                {
                    dest[destOffset + tpatchIndex++] = block[tblockIndex++] * mult + addval;
                }
            }
        }
    }

    protected static void idct_patch (float[] block)
    {
        float[] temp = new float[LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];

#if _PATCH_SIZE_16_AND_32_ONLY
        idct_column (block, temp, 0);
        idct_column (block, temp, 1);
        idct_column (block, temp, 2);
        idct_column (block, temp, 3);

        idct_column (block, temp, 4);
        idct_column (block, temp, 5);
        idct_column (block, temp, 6);
        idct_column (block, temp, 7);

        idct_column(block, temp, 8);
        idct_column(block, temp, 9);
        idct_column(block, temp, 10);
        idct_column(block, temp, 11);

        idct_column(block, temp, 12);
        idct_column(block, temp, 13);
        idct_column(block, temp, 14);
        idct_column(block, temp, 15);

        idct_line(temp, block, 0);
        idct_line(temp, block, 1);
        idct_line(temp, block, 2);
        idct_line(temp, block, 3);

        idct_line(temp, block, 4);
        idct_line(temp, block, 5);
        idct_line(temp, block, 6);
        idct_line(temp, block, 7);

        idct_line(temp, block, 8);
        idct_line(temp, block, 9);
        idct_line(temp, block, 10);
        idct_line(temp, block, 11);

        idct_line(temp, block, 12);
        idct_line(temp, block, 13);
        idct_line(temp, block, 14);
        idct_line(temp, block, 15);
#else
        // TODO: Test this to make sure that it is correct
        int i;
        int size = GroupHeader.PatchSize;
        for (i = 0; i < size; i++)
        {
            idct_column (block, temp, i);
        }
        for (i = 0; i < size; i++)
        {
            idct_line (temp, block, i);
        }
#endif
    }

    protected static void idct_patch_large(float[] block)
    {
        float[] temp = new float[LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];

        idct_column_large_slow(block, temp, 0);
        idct_column_large_slow(block, temp, 1);
        idct_column_large_slow(block, temp, 2);
        idct_column_large_slow(block, temp, 3);

        idct_column_large_slow(block, temp, 4);
        idct_column_large_slow(block, temp, 5);
        idct_column_large_slow(block, temp, 6);
        idct_column_large_slow(block, temp, 7);

        idct_column_large_slow(block, temp, 8);
        idct_column_large_slow(block, temp, 9);
        idct_column_large_slow(block, temp, 10);
        idct_column_large_slow(block, temp, 11);

        idct_column_large_slow(block, temp, 12);
        idct_column_large_slow(block, temp, 13);
        idct_column_large_slow(block, temp, 14);
        idct_column_large_slow(block, temp, 15);

        idct_column_large_slow(block, temp, 16);
        idct_column_large_slow(block, temp, 17);
        idct_column_large_slow(block, temp, 18);
        idct_column_large_slow(block, temp, 19);

        idct_column_large_slow(block, temp, 20);
        idct_column_large_slow(block, temp, 21);
        idct_column_large_slow(block, temp, 22);
        idct_column_large_slow(block, temp, 23);

        idct_column_large_slow(block, temp, 24);
        idct_column_large_slow(block, temp, 25);
        idct_column_large_slow(block, temp, 26);
        idct_column_large_slow(block, temp, 27);

        idct_column_large_slow(block, temp, 28);
        idct_column_large_slow(block, temp, 29);
        idct_column_large_slow(block, temp, 30);
        idct_column_large_slow(block, temp, 31);

        idct_line_large_slow(temp, block, 0);
        idct_line_large_slow(temp, block, 1);
        idct_line_large_slow(temp, block, 2);
        idct_line_large_slow(temp, block, 3);

        idct_line_large_slow(temp, block, 4);
        idct_line_large_slow(temp, block, 5);
        idct_line_large_slow(temp, block, 6);
        idct_line_large_slow(temp, block, 7);

        idct_line_large_slow(temp, block, 8);
        idct_line_large_slow(temp, block, 9);
        idct_line_large_slow(temp, block, 10);
        idct_line_large_slow(temp, block, 11);

        idct_line_large_slow(temp, block, 12);
        idct_line_large_slow(temp, block, 13);
        idct_line_large_slow(temp, block, 14);
        idct_line_large_slow(temp, block, 15);

        idct_line_large_slow(temp, block, 16);
        idct_line_large_slow(temp, block, 17);
        idct_line_large_slow(temp, block, 18);
        idct_line_large_slow(temp, block, 19);

        idct_line_large_slow(temp, block, 20);
        idct_line_large_slow(temp, block, 21);
        idct_line_large_slow(temp, block, 22);
        idct_line_large_slow(temp, block, 23);

        idct_line_large_slow(temp, block, 24);
        idct_line_large_slow(temp, block, 25);
        idct_line_large_slow(temp, block, 26);
        idct_line_large_slow(temp, block, 27);

        idct_line_large_slow(temp, block, 28);
        idct_line_large_slow(temp, block, 29);
        idct_line_large_slow(temp, block, 30);
        idct_line_large_slow(temp, block, 31);
    }

    protected static void idct_line (float[] linein, float[] lineout, int line)
    {
        int n;
        float total;
        int pcpIndex = 0; //gPatchICosines;

#if _PATCH_SIZE_16_AND_32_ONLY
        float oosob = 2f / 16f;
        int line_size = line * NORMAL_PATCH_SIZE;
        int tlineinIndex;
        int tpcpIndex;

        for (n = 0; n < NORMAL_PATCH_SIZE; n++)
        {
            tpcpIndex = pcpIndex + n;
            tlineinIndex = line_size;

            total = OO_SQRT2 * linein[tlineinIndex++];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            lineout[line_size + n] = total * oosob;
        }
#else
        // TODO: Test this to make sure that it is correct
        int size = GroupHeader.PatchSize;
        float oosob = 2f / size;
        int line_size = line * size;
        int u;
        for (n = 0; n < size; n++)
        {
            total = OO_SQRT2 * linein[line_size];
            for (u = 1; u < size; u++)
            {
                total += linein[line_size + u] * PatchICosines[u * size + n];
            }
            lineout[line_size + n] = total * oosob;
        }
#endif
    }

    protected static void idct_line_large_slow (float[] linein, float[] lineout, int line)
    {
        int n;
        float total;
        int pcpIndex = 0; //gPatchICosines;

        float oosob = 2f / 32f;
        int line_size = line * LARGE_PATCH_SIZE;
        int tlineinIndex;
        int tpcpIndex;
        
        for (n = 0; n < LARGE_PATCH_SIZE; n++)
        {
            tpcpIndex = pcpIndex + n;
            tlineinIndex = line_size;

            total = OO_SQRT2 * linein[tlineinIndex++];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            lineout[line_size + n] = total * oosob;
        }
    }

    protected static void idct_line_large(float[] linein, float[] lineout, int line)
    {
        int n;
        float total;
        int pcpIndex = 0; //gPatchICosines;

        float oosob = 2f / 32f;
        int line_size = line * LARGE_PATCH_SIZE;
        int tlineinIndex;
        int tpcpIndex;

        int baselineinIndex = line_size;
        int baselineoutIndex = line_size;

        for (n = 0; n < LARGE_PATCH_SIZE; n++)
        {
            tpcpIndex = pcpIndex++;
            tlineinIndex = baselineinIndex;

            total = OO_SQRT2 * linein[tlineinIndex++];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex++] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            lineout[baselineoutIndex++] = total * oosob;
        }
    }

    protected static void idct_column(float[] linein, float[] lineout, int column)
    {
        int n;
        float total;
        int pcpIndex = 0; //gPatchICosines;

#if _PATCH_SIZE_16_AND_32_ONLY
        int tlineinIndex;
        int tpcpIndex;

        for (n = 0; n < NORMAL_PATCH_SIZE; n++)
        {
            tpcpIndex = pcpIndex + n;
            tlineinIndex = column;

            total = OO_SQRT2 * linein[tlineinIndex];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];
            total += linein[tlineinIndex += NORMAL_PATCH_SIZE] * PatchICosines[tpcpIndex += NORMAL_PATCH_SIZE];

            lineout[(n << 4) + column] = total;
        }

#else
        int size = GroupHeader.PatchSize;
        int u;
        int u_size;

        for (n = 0; n < size; n++)
        {
            total = OO_SQRT2 * linein[column];
            for (u = 1; u < size; u++)
            {
                u_size = u * size;
                total += linein[u_size + column] * PatchICosines[u_size + n];
            }
            lineout[size * n + column] = total;
        }
#endif
    }

    protected static void idct_column_large_slow(float[] linein, float[] lineout, int column)
    {
        int n;
        float total;
        int pcpIndex = 0; //gPatchICosines;

        int tlineinIndex;
        int tpcpIndex;

        for (n = 0; n < LARGE_PATCH_SIZE; n++)
        {
            tpcpIndex = pcpIndex + n;
            tlineinIndex = column;

            total = OO_SQRT2 * linein[tlineinIndex];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];

            lineout[(n << 5) + column] = total;
        }
    }

    // Nota Bene: assumes that coefficients beyond 128 are 0!

    void idct_column_large(float[] linein, float[] lineout, int column)
    {
        int n;
        int m;
        float total;
        int pcpIndex = 0; //gPatchICosines;

        int tlineinIndex;
        int tpcpIndex;
        int baselineinIndex = column;
        int baselineoutIndex = column;

        for (n = 0; n < LARGE_PATCH_SIZE; n++)
        {
            tpcpIndex = pcpIndex++;
            tlineinIndex = baselineinIndex;

            total = OO_SQRT2 * linein[tlineinIndex];
            for (m = 1; m < NORMAL_PATCH_SIZE; m++)
            {
                total += linein[tlineinIndex += LARGE_PATCH_SIZE] * PatchICosines[tpcpIndex += LARGE_PATCH_SIZE];
            }

            lineout[baselineoutIndex + (n << 5)] = total;
        }
    }

}
