using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        GameBoard board = new GameBoard(4);

        int[,] zero = { { 1, 1, 1, 1},
                        { 1, 1, 1, 0},
                        { 1, 1, 0, 0},
                        { 1 ,0 ,0 ,0} };
        int[,] one = { { 0, 0, 0, 0},
                        { -1, 0, 0, 0},
                        { -1, -1, 0, 0},
                        { -1 ,-1 , -1 ,-1} };
        int[,] two = { { 1, 1, -1, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0 ,0 ,0 ,0} };
        int[,] three = { { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0 ,0 ,0 ,0} };

        board.SetSlice(0, zero);
        board.SetSlice(1, one);
        board.SetSlice(2, two);
        board.SetSlice(3, three);

        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                float score = board.CalculateLineScore((x, y, 0), 2);
                Debug.Log("Score: " + score);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
