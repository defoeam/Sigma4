using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPole : MonoBehaviour
{
    public int columm;
    public GameManager instance;



    void OnMouseDown()
    {
        if (instance.HumanPlayer && instance.Turn == true)
            instance.PlacePiece(columm);

    }

}
