using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System;

public class Sigma4Agent : Agent
{
    public GameManager Game;
    public int player = 0;

    public override void CollectObservations(VectorSensor sensor)
    {
        int[,,] board = new int[Game.Size, Game.Size, Game.Size];
        Array.Copy(Game.BoardState.State, board, board.Length);

        // account for player 2 case:
        //    reverse the board state map such that the 1s and 2s are swapped.
        if(player == 2)
            for(int x = 0; x < Game.Size; x++)
                for(int z = 0; z < Game.Size; z++)
                    for(int y = 0; y < Game.Size; y++)
                        if(board[x,z,y] == 1)
                            board[x,z,y] = -1;
                        else if(board[x,z,y] == -1)
                            board[x,z,y] = 1;
        
        // Add observerations
        foreach(int i in board)
            sensor.AddObservation(i);
        
    }

    /// <summary>
    /// Agent will konw not to select a column that is full as an action.
    /// </summary>
    /// <param name="actionMask"></param>
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        foreach(int col in Game.FullColumns)
            actionMask.SetActionEnabled(0, col - 1, false);
    }


    /// <summary>
    /// Recieves actions and assigns rewards
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        int col = actions.DiscreteActions[0] + 1;
        Game.AgentAction(col);
    }

    // Simple wait util that doesn't cause the main thread to pause.
    // (For some reason, this method doesn't work when two AIs are playing each other)
    private async Task wait() => await Task.Delay(500);
}
