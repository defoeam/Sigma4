using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sigma4GameInstance
{
    public int[,,] _gameMap;
    private int _mapSize;


    public Sigma4GameInstance(int mapSize, Agent agent1, Agent agent2){
        _mapSize = mapSize >= 4 ? mapSize : 4;
    }
    

    // DUDE WHAT ARE WE GONNA DO WE ARE SO COOKED
}