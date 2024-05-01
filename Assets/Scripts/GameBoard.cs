using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

// Used for picking direction in score calculation in the 3D array
//       0 = x    1 = y    2 = z
//       3=yzdiagonal  4=yzdiagonal_backwards   (width wise)
//       4=xzdiagonal  5=xzdiagonal_backwards   (height wise)
//       6=xydiagonal  7=xydiagonal_backwards   (depth wise)
//       7=corners (000, 111, 222, 333)
//       8=corners (300, 211, 122, 033)
//       9=corners (003, 112, 221, 330)
//       10=corners (303, 212, 121, 030)
public enum ScoreDirection
{
    X,
    Y,
    Z,
    YZDia,
    YZBack,
    XZDia,
    XZBack,
    XYDia,
    XYBack,
    Corner0,
    Corner1,
    Corner2,
    Corner3,
}

// Contains Connect 4 game state
// Can calculate an oppertunity score for each state
// Opportunity score is a heuristic of how good a position is for the player
// Player is either considered positive or negitive 
//      ie they are represented on the board by a 1 or -1 value in the game spot
//      value of 0 means no piece is placed yet
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

    // diagonal Scores
    // - Width (both forwards and backwards so size*2)
    public float[] positiveDiagonalWidthScores;
    public float[] negativeDiagonalWidthScores;
    // - Height (both forwards and backwards so size*2)
    public float[] positiveDiagonalHeightScores;
    public float[] negativeDiagonalHeightScores;
    // - Depth (both forwards and backwards so size*2)
    public float[] positiveDiagonalDepthScores;
    public float[] negativeDiagonalDepthScores;
    // - Corners (4 corners)
    public float[] positiveDiagonalCornerScores;
    public float[] negativeDiagonalCornerScores;


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
        positiveDiagonalWidthScores = new float[size * 2];
        negativeDiagonalWidthScores = new float[size * 2];
        positiveDiagonalHeightScores = new float[size * 2];
        negativeDiagonalHeightScores = new float[size * 2];
        positiveDiagonalDepthScores = new float[size * 2];
        negativeDiagonalDepthScores = new float[size * 2];
        positiveDiagonalCornerScores = new float[4];
        negativeDiagonalCornerScores = new float[4];
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

    public (float, float) GetMaxOpportunityScores()
    {
        float[] positives = { Array2DMax(positiveWidthScores), 
            Array2DMax(positiveHeightScores), 
            Array2DMax(positiveDepthScores),
            ArrayMax(positiveDiagonalWidthScores),
            ArrayMax(positiveDiagonalHeightScores),
            ArrayMax(positiveDiagonalDepthScores),
            ArrayMax(positiveDiagonalCornerScores),
        };

        float[] negatives = { Array2DMax(negativeWidthScores),
            Array2DMax(negativeHeightScores),
            Array2DMax(negativeDepthScores),
            ArrayMax(negativeDiagonalWidthScores),
            ArrayMax(negativeDiagonalHeightScores),
            ArrayMax(negativeDiagonalDepthScores),
            ArrayMax(negativeDiagonalCornerScores),
        };

        return (ArrayMax(positives), ArrayMax(negatives));
    }

    public (float, float) GetAverageOpportunityScores()
    {
        float posSum = 0;
        float posCount = 0;
        float negSum = 0;
        float negCount = 0;

        // Get pos sum and pos count
        (float, int) temp = Array2DSum(positiveWidthScores);
        posSum += temp.Item1;
        posCount += temp.Item2;
        temp = Array2DSum(positiveHeightScores);
        posSum += temp.Item1;
        posCount += temp.Item2;
        temp = Array2DSum(positiveDepthScores);
        posSum += temp.Item1;
        posCount += temp.Item2;
        temp = ArraySum(positiveDiagonalWidthScores);
        posSum += temp.Item1;
        posCount += temp.Item2;
        temp = ArraySum(positiveDiagonalHeightScores);
        posSum += temp.Item1;
        posCount += temp.Item2;
        temp = ArraySum(positiveDiagonalDepthScores);
        posSum += temp.Item1;
        posCount += temp.Item2;
        temp = ArraySum(positiveDiagonalCornerScores);
        posSum += temp.Item1;
        posCount += temp.Item2;

        // Get neg sum and neg count
        temp = Array2DSum(negativeWidthScores);
        negSum += temp.Item1;
        negCount += temp.Item2;
        temp = Array2DSum(negativeHeightScores);
        negSum += temp.Item1;
        negCount += temp.Item2;
        temp = Array2DSum(negativeDepthScores);
        negSum += temp.Item1;
        negCount += temp.Item2;
        temp = ArraySum(negativeDiagonalWidthScores);
        negSum += temp.Item1;
        negCount += temp.Item2;
        temp = ArraySum(negativeDiagonalHeightScores);
        negSum += temp.Item1;
        negCount += temp.Item2;
        temp = ArraySum(negativeDiagonalDepthScores);
        negSum += temp.Item1;
        negCount += temp.Item2;
        temp = ArraySum(negativeDiagonalCornerScores);
        negSum += temp.Item1;
        negCount += temp.Item2;

        // Return averages
        return (posSum / posCount, negSum / negCount);
    }

    public void CalculateOpportunityScores()
    {
        // Width Scores
        for (int z = 0; z < Size; z++)
        {
            for (int y = 0; y < Size; y++)
            {
                positiveWidthScores[y, z] = CalculateLineScore((0, y, z), ScoreDirection.X);
                negativeWidthScores[y, z] = CalculateLineScore((0, y, z), ScoreDirection.X, true);
            }
        }

        // Height Scores
        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                positiveHeightScores[x, z] = CalculateLineScore((x, 0, z), ScoreDirection.Y);
                negativeHeightScores[x, z] = CalculateLineScore((x, 0, z), ScoreDirection.Y, true);
            }
        }

        // Depth Scores
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                positiveWidthScores[x, y] = CalculateLineScore((x, y, 0), ScoreDirection.Z);
                negativeWidthScores[x, y] = CalculateLineScore((x, y, 0), ScoreDirection.Z, true);
            }
        }

        // Diagonal Width Scores (YZ)
        for (int i = 0; i < Size; i++)
        {
            positiveDiagonalWidthScores[i] = CalculateLineScore((i, 0, 0), ScoreDirection.YZDia);
            negativeDiagonalWidthScores[i] = CalculateLineScore((i, 0, 0), ScoreDirection.YZDia, true);
        }
        for (int i = 0; i < Size; i++)
        {
            positiveDiagonalWidthScores[i] = CalculateLineScore((i, 0, 3), ScoreDirection.YZBack);
            negativeDiagonalWidthScores[i] = CalculateLineScore((i, 0, 3), ScoreDirection.YZBack, true);
        }
        // Diagonal Height Scores (XZ)
        for (int i = 0; i < Size; i++)
        {
            positiveDiagonalWidthScores[i] = CalculateLineScore((0, i, 0), ScoreDirection.XZDia);
            negativeDiagonalWidthScores[i] = CalculateLineScore((0, i, 0), ScoreDirection.XZDia, true);
        }
        for (int i = 0; i < Size; i++)
        {
            positiveDiagonalWidthScores[i] = CalculateLineScore((3, i, 0), ScoreDirection.XZBack);
            negativeDiagonalWidthScores[i] = CalculateLineScore((3, i, 0), ScoreDirection.XZBack, true);
        }
        // Diagonal Depth Scores (XY)
        for (int i = 0; i < Size; i++)
        {
            positiveDiagonalWidthScores[i] = CalculateLineScore((0, 0, i), ScoreDirection.XYDia);
            negativeDiagonalWidthScores[i] = CalculateLineScore((0, 0, i), ScoreDirection.XYDia, true);
        }
        for (int i = 0; i < Size; i++)
        {
            positiveDiagonalWidthScores[i] = CalculateLineScore((3, 0, i), ScoreDirection.XYBack);
            negativeDiagonalWidthScores[i] = CalculateLineScore((3, 0, i), ScoreDirection.XYBack, true);
        }
        // Corners
        positiveDiagonalCornerScores[0] = CalculateLineScore((0, 0, 0), ScoreDirection.Corner0);
        negativeDiagonalCornerScores[0] = CalculateLineScore((0, 0, 0), ScoreDirection.Corner0, true);
        positiveDiagonalCornerScores[1] = CalculateLineScore((3, 0, 0), ScoreDirection.Corner1);
        negativeDiagonalCornerScores[1] = CalculateLineScore((3, 0, 0), ScoreDirection.Corner1, true);
        positiveDiagonalCornerScores[2] = CalculateLineScore((0, 0, 3), ScoreDirection.Corner2);
        negativeDiagonalCornerScores[2] = CalculateLineScore((0, 0, 3), ScoreDirection.Corner2, true);
        positiveDiagonalCornerScores[3] = CalculateLineScore((3, 0, 3), ScoreDirection.Corner3);
        negativeDiagonalCornerScores[3] = CalculateLineScore((3, 0, 3), ScoreDirection.Corner3, true);
    }

    // Calculate the opportunity score for a line starting at x,y,z and then increment on the index
    public float CalculateLineScore((int, int, int) startPos, ScoreDirection dir, bool flipPerspective=false)
    {
        // Extract line for processing
        int[] line = new int[Size];
        (int, int, int) currentPos = startPos;
        for (int i = 0; i < Size; i++)
        {
            line[i] = this.GetSpot(currentPos);
            // Increment to next spot
            if (dir == ScoreDirection.X)
            {
                currentPos.Item1 += 1;
            }
            else if (dir == ScoreDirection.Y)
            {
                currentPos.Item2 += 1;
            }
            else if (dir == ScoreDirection.Z)
            {
                currentPos.Item3 += 1;
            }
            else if (dir == ScoreDirection.YZDia)
            {
                currentPos.Item2 += 1;
                currentPos.Item3 += 1;
            }
            else if (dir == ScoreDirection.YZBack)
            {
                currentPos.Item2 += 1;
                currentPos.Item3 -= 1;
            }
            else if (dir == ScoreDirection.XZDia)
            {
                currentPos.Item1 += 1;
                currentPos.Item3 += 1;
            }
            else if (dir == ScoreDirection.XZBack)
            {
                currentPos.Item1 -= 1;
                currentPos.Item3 += 1;
            }
            else if (dir == ScoreDirection.XYDia)
            {
                currentPos.Item1 += 1;
                currentPos.Item2 += 1;
            }
            else if (dir == ScoreDirection.XYBack)
            {
                currentPos.Item1 -= 1;
                currentPos.Item2 += 1;
            }
            else if (dir == ScoreDirection.Corner0)
            {
                currentPos.Item1 += 1;
                currentPos.Item2 += 1;
                currentPos.Item3 += 1;
            }
            else if (dir == ScoreDirection.Corner1)
            {
                currentPos.Item1 -= 1;
                currentPos.Item2 += 1;
                currentPos.Item3 += 1;
            }
            else if (dir == ScoreDirection.Corner2)
            {
                currentPos.Item1 += 1;
                currentPos.Item2 += 1;
                currentPos.Item3 -= 1;
            }
            else if (dir == ScoreDirection.Corner3)
            {
                currentPos.Item1 -= 1;
                currentPos.Item2 += 1;
                currentPos.Item3 -= 1;
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

        // Will print start pos and line extracted
        //Debug.Log(startPos.Item1 + ":" + startPos.Item2 + ":" + startPos.Item3 + " " + GameBoard.ArrayToString(line.Select(x => (float)x).ToArray()));


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

    // player 1: 2, player 2: 0, empty: 1
    public string StateToString(){
        string output = "";
        foreach(int i in State)
            output += i + 1;
        return output;
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

    public static (float,int) Array2DSum(float[,] input)
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
        return (sum, count);
    }

    public static float ArrayAverage(float[] input)
    {
        float sum = 0;
        for (int i = 0; i < input.Length; i++)
        {
            sum += input[i];
        }
        return sum / input.Length;
    }
    public static (float, int) ArraySum(float[] input)
    {
        float sum = 0;
        for (int i = 0; i < input.Length; i++)
        {
            sum += input[i];
        }
        return (sum, input.Length);
    }

    public static float Array2DMax(float[,] input)
    {
        float max = 0;
        for (int x = 0; x < input.GetLength(0); x++)
        {
            for (int y = 0; y < input.GetLength(1); y++)
            {
                if (input[x,y] > max)
                {
                    max = input[x,y];
                }
            }
        }
        return max;
    }

    public static float ArrayMax(float[] input)
    {
        float max = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] > max)
            {
                max = input[i];
            }
        }
        return max;
    }
}
