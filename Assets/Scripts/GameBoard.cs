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
    public float[,] widthScores;

    // Height Scores
    public float[,] heightScores;

    // Depth Scores
    public float[,] depthScores;

    // Diagnal Scores



    public GameBoard(int size)
    {
        this.Size = size;
        state = new int[size, size, size];
        widthScores = new float[size, size];
        heightScores = new float[size, size];
        depthScores = new float[size, size];
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
        return (0.0f, 0.0f);
    }

    public void CalculateOpportunityScores()
    {
        // Width Scores
        for (int z = 0; z < Size; z++)
        {
            for (int y = 0; y < Size; y++)
            {
                widthScores[y, z] = CalculateLineScore((0, y, z), 0);
                Debug.Log(widthScores[y, z]);
            }
        }

        // Height Scores
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                widthScores[x, z] = CalculateLineScore((x, 0, z), 1);
            }
        }

        // Depth Scores
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                widthScores[x, y] = CalculateLineScore((x, y, 0), 2);
            }
        }
    }

    // Calculate the opportunity score for a line starting at x,y,z and then increment on the index
    // index 0=x    1=y    2=z   3=xdiagnal      4=zdiagnal
    public float CalculateLineScore((int, int, int) startPos, int index)
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

        // Get max number of 1 or 0s in a row
        (int, int, int) inARow = GetMostInARow(line);
        int maxNumInARow = inARow.Item1;
        int startIndex = inARow.Item2;
        int ones = inARow.Item3;
        int zeros = maxNumInARow - ones;

        Debug.Log(GameBoard.ArrayToString(line.Select(x => (float)x).ToArray()));


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
}
