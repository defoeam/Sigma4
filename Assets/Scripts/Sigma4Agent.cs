using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class Sigma4Agent : Agent
{

    private GameManager Game;
    
    void Start()
    {
        Game = GetComponent<GameManager>();
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach(int i in Game.BoardState){
            sensor.AddObservation(i);
        }


            
    }


    /// <summary>
    /// Recieves actions and assigns rewards
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        int col = actions.DiscreteActions[0] + 1;
        Debug.Log("I want to do this col: " + col);
    
    }

}
