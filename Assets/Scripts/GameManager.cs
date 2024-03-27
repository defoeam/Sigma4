using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player1;
    public GameObject player2;
    public GameObject[] spawnLoc;

    public bool turn = true;

    public int length = 4;
    public int width = 4;
    public int height = 4;

    int[,,] boardState;

    // 2d side view
    // 0 0 0 0
    // 0 0 0 0
    // 0 0 0 0
    // 0 0 0 0

    void Start()
    {
        boardState = new int[length, width, height]; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool UpdateBoardState(int column)
    {
        for (int row = 0; row < 4; row++)
        {
            for (int height = 0; height < 4; height++)
            {
                if (boardState[column, row, height] == 0)
                {
                    if (turn)
                    {
                        boardState[column, row, height] = 1;
                    }
                    else
                    {
                        boardState[column, row, height] = 2;
                    }
                    Debug.Log("Piece being spawned at (" + column + ", " + row + ", " + height + ")");
                    return true;
                }
            }
        }
        Debug.LogWarning("");
        return false; 
    }

    public void SelectColumn(int column)
    {
        Debug.Log("Gamemanager Column " + column);
        TakeTurn(column);
    }

    void TakeTurn(int column)
    {
        if (turn)
        {
            Instantiate(player1, spawnLoc[column - 1].transform.position, Quaternion.identity);
            turn = false;
        }
        else
        {
            Instantiate(player2, spawnLoc[column - 1].transform.position, Quaternion.identity);
            turn = true;
        }
    }
}
