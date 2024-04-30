using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        GameBoard board = new GameBoard(4);

        int[,] zero = { { 1, 0, 0, 0},
                        { 0, 1, 0, 0},
                        { 0, 0, 1, 0},
                        { 0, 0, 0, -1} };
        int[,] one = { { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0},
                        { 0, 0, 0, 0 } };
        int[,] two = { { 0, 0, 0, 0},
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

        float score = board.CalculateLineScore((0, 0, 0), ScoreDirection.YZDia);
        Debug.Log("Score: " + score);

        //board.CalculateOpportunityScores();
        //Debug.Log("Averages: " + board.GetAverageOpportunityScores());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
