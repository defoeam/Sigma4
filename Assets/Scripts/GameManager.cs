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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectColumn(int column)
    {
        Debug.Log("Gamemanager Colum " + column);
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
            Instantiate(player2, spawnLoc[column-1].transform.position,Quaternion.identity);
            turn = true;
        }
        
    }
}
