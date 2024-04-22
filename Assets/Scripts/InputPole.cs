using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPole : MonoBehaviour
{
    public int columm;
    public GameManager instance;

    void OnMouseDown() => instance.PlacePiece(columm);

}
