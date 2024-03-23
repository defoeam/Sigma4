using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPole : MonoBehaviour
{
    public int columm;

    public GameManager instance;
    // Start is called before the first frame update
    void OnMouseDown()
    {
        instance.SelectColumn(columm);
        
    }
}
