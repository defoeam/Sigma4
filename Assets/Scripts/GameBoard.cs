using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public int Size {  get; private set; }
    private int[,,] state;
    // Width Scores
    private int[,] widthScores;

    // Height Scores
    private int[,] heightScores;

    // Depth Scores
    private int[,] depthScores;

    // Diagnal Scores



    public GameBoard(int size)
    {
        this.Size = size;
        state = new int[size, size, size];
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

    public void CalculateOpportunityScores()
    {
        // Width Scores
        for (int z = 0; z < Size; z++)
        {
            for (int y = 0; y < Size; y++)
            {
                widthScores[y, z] = CalculateLineScore((0, y, z), 0);
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
    private float CalculateLineScore((int, int, int) startPos, int index)
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
            else if (index == 3)
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

        // Calculate score
        // If no zeros, no room to grow, zero score
        if (zeros == 0)
        {
            return 0.0f;
        }
        // If no ones, return zero score
        else if (ones == 0)
        {
            return 0.0f;
        }
        // If ones + zeros = 4, best case, scale with number of ones
        else if ((ones + zeros) >= 4)
        {
            return (ones * 0.25f) + (zeros * 0.1f);
        }
        // Otherwise, same score calculation with small reduction
        else
        {
            return ((ones * 0.25f) + (zeros * 0.1f)) * 0.8f;
        }
    }

    // Given an array of 0, 1, or -1, returns a tuple (int, int, int) for the (max number of 1 or 0s in a row, startIndex, num of 1s in that return sequence)
    // ie [0, 1, 1, 0, -1, 1, 1, 0] would see the first 4 spots as the max in a row and return (4, 0, 2)
    private (int, int, int) GetMostInARow(int[] input)
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

        // Get amount of 1s in the max in a row sequence
        int numOfOnes = 0;
        for (int i = maxIndex; i < max; i++)
        {
            if (i == 1) numOfOnes++;
        }

        return (max, maxIndex, numOfOnes);
    }
}
