using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPole : MonoBehaviour
{
    public int columm;

    public GameManager instance;
<<<<<<< Updated upstream
    // Start is called before the first frame update
    void OnMouseDown()
    {
        instance.SelectColumn(columm);
        
    }
=======



    void OnMouseDown()
    {
        if (instance.HumanPlayer && instance.Turn == true)
            instance.PlacePiece(columm);

    }

>>>>>>> Stashed changes
}
