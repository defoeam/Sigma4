using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class GameBoard
{
    public int Size {  get; private set; }
    public int[,,] State { get { return this.state; } private set { this.state = value;  } }
    private int[,,] state;
    private float zeroMod;
    // Width Scores
    public float[,] positiveWidthScores;
    public float[,] negativeWidthScores;

    // Height Scores
    public float[,] positiveHeightScores;
    public float[,] negativeHeightScores;

    // Depth Scores
    public float[,] positiveDepthScores;
    public float[,] negativeDepthScores;

    // Diagnal Scores



    public GameBoard(int size)
    {
        this.Size = size;
        state = new int[size, size, size];
        positiveWidthScores = new float[size, size];
        negativeWidthScores = new float[size, size];
        positiveHeightScores = new float[size, size];
        negativeHeightScores = new float[size, size];
        positiveDepthScores = new float[size, size];
        negativeDepthScores = new float[size, size];
        zeroMod = 0.8f / size;
    }

    public int GetSpot((int, int, int) position)
    {
        return this.GetSpot(position.Item1, position.Item2, position.Item3);
    }

    public int GetSpot(int x, int y, int z)
    {
        return state[x, y, z];
    }

    public void SetSpot((int, int, int) position, int value)
    {
        this.SetSpot(position.Item1, position.Item2, position.Item3, value);
    }

    public void SetSpot(int x, int y, int z, int value)
    {
        state[x, y, z] = value;
    }

    public void SetSlice(int x, int[,] slice)
    {
        for(int y = 0; y < slice.GetLength(0); y++)
        {
            for (int z = 0; z < slice.GetLength(1); z++)
            {
                this.state[x, y, z] = slice[y, z];
            }
        }
    }

    public (float, float) GetAverageOpportunityScores()
    {
        float posSum = Array2DAverage(positiveWidthScores) + Array2DAverage(positiveHeightScores) + Array2DAverage(positiveDepthScores);
        float negSum = Array2DAverage(negativeWidthScores) + Array2DAverage(negativeHeightScores) + Array2DAverage(negativeDepthScores);

        return (posSum / 3, negSum / 3);
    }

    public void CalculateOpportunityScores()
    {
        // Width Scores
        for (int z = 0; z < Size; z++)
        {
            for (int y = 0; y < Size; y++)
            {
                positiveWidthScores[y, z] = CalculateLineScore((0, y, z), 0);
                negativeWidthScores[y, z] = CalculateLineScore((0, y, z), 0, true);
            }
        }

        // Height Scores
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                positiveHeightScores[x, z] = CalculateLineScore((x, 0, z), 1);
                negativeHeightScores[x, z] = CalculateLineScore((x, 0, z), 1, true);
            }
        }

        // Depth Scores
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                positiveWidthScores[x, y] = CalculateLineScore((x, y, 0), 2);
                negativeWidthScores[x, y] = CalculateLineScore((x, y, 0), 2, true);
            }
        }
    }

    // Calculate the opportunity score for a line starting at x,y,z and then increment on the index
    // index 0=x    1=y    2=z   3=xdiagnal      4=zdiagnal
    public float CalculateLineScore((int, int, int) startPos, int index, bool flipPerspective=false)
    {
        // Extract line for processing
        int[] line = new int[Size];
        (int, int, int) currentPos = startPos;
        for (int i = 0; i < Size; i++)
        {
            line[i] = this.GetSpot(currentPos);
            // Increment to next spot
            if (index == 0)
            {
                currentPos.Item1 += 1;
            }
            else if (index == 1)
            {
                currentPos.Item2 += 1;
            }
            else if (index == 2)
            {
                currentPos.Item3 += 1;
            }
        }

        // If perspective fliped, flip 1s and -1s
        if (flipPerspective)
        {
            for(int i = 0; i < line.Length; i++)
            {
                line[i] = line[i] * -1;
            }
        }

        // Get max number of 1 or 0s in a row
        (int, int, int) inARow = GetMostInARow(line);
        int maxNumInARow = inARow.Item1;
        int startIndex = inARow.Item2;
        int ones = inARow.Item3;
        int zeros = maxNumInARow - ones;

        Debug.Log(startPos.Item1 + ":" + startPos.Item2 + ":" + startPos.Item3 + " " + GameBoard.ArrayToString(line.Select(x => (float)x).ToArray()));


        // Calculate score
        // If can grow to a connect 4
        if ((ones + zeros) >= 4)
        {
            // Connect 4 = 1, more zeros make it further away from 1
            return 1f - (zeroMod * zeros);
        }else
        {
            // No way to get to connect 4, return 0
            return 0;
        }
    }

    // Given an array of 0, 1, or -1, returns a tuple (int, int, int) for the (max number of 1 or 0s in a row, startIndex, num of 1s in that return sequence)
    // ie [0, 1, 1, 0, -1, 1, 1, 0] would see the first 4 spots as the max in a row and return (4, 0, 2)
    public (int, int, int) GetMostInARow(int[] input)
    {
        int length = input.Length;
        int current = 0; // Current 1s in a row
        int currentIndex = 0;
        int max = 0; // Max 1s in a row
        int maxIndex = 0;
        for (int i = 0; i < length; i++)
        {
            // If 1 or 0, add 1 to current
            if (input[i] > -1)
            {
                // If not following 1s in a row yet, set the current start index
                if (current == 0) currentIndex = i;
                current += 1;
            }else
            {
                // Otherwise, potientally replace the maxs and restart
                if(current > max)
                {
                    max = current;
                    maxIndex = currentIndex; 
                }
                current = 0;
            }
        }
        // Last check for end of line max, potientally replace the maxs and restart
        if (current > max)
        {
            max = current;
            maxIndex = currentIndex;
        }

        // Get amount of 1s in the max in a row sequence
        int numOfOnes = 0;
        for (int i = maxIndex; i < max; i++)
        {
            if (input[i] == 1) numOfOnes++;
        }

        return (max, maxIndex, numOfOnes);
    }

    public static string Array2DToString(float[,] input)
    {
        string output = "[ ";
        for(int x = 0; x < input.GetLength(0); x++)
        {
            for(int y = 0;  y < input.GetLength(1); y++)
            {
                output += input[x, y] + " ";
            }
            output += "\n";
        }
        output += "]";
        return output;
    }

    public static string ArrayToString(float[] input) 
    {
        string output = "[ ";
        for(int i = 0; i < input.Length; i++)
        {
            output += input[i] + " ";
        }
        return output + "]";
    }

    public static float Array2DAverage(float[,] input)
    {
        float sum = 0;
        int count = 0;
        for (int x = 0; x < input.GetLength(0); x++)
        {
            for (int y = 0; y < input.GetLength(1); y++)
            {
                sum += input[x, y];
                count++;
            }
        }
        return sum / count;
    }
}
