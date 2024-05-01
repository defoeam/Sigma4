using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPole : MonoBehaviour
{
    private int index;
    public GameManager instance;

    private void Awake()
    {
        index = int.Parse(gameObject.name);
    }

    void OnMouseDown()
    {
        if (instance.HumanPlayer && instance.Turn == true)
            instance.PlacePiece(index);

    }

}
