using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Game Visualization Properties
    public GameObject Player1Piece;
    public GameObject Player2Piece;
    public GameObject[] SpawnLoc;
    public bool VisualizeGame;    // Set to true if you want to see the pieces being placed

    // Agents
    public Sigma4Agent Agent1;
    public Sigma4Agent Agent2;
    public bool UseRandomForAgent2;

    
    // Game Logic
    public bool Turn = true;      // true = Player1 turn, false = Player2 turn.
    private int TurnNumber;
    public int Size = 4;
    public bool HumanPlayer = false;
    public List<int> FullColumns;
    private Dictionary<int, (int, int)> _columnToIndex;
    private List<GameObject> _piecesPlaced;
    private bool waitForChoice = false;
    private bool _gameOver = false;


    // Current State of the Board
    public GameBoard BoardState;


    // Reward Structure Settings ~ (win/loss) is always enabled
    public bool UseOpportunityScore;
    public bool UseHaste;
    private float hasteMultiplier = 1f;


    void Start()
    {
        _columnToIndex = new Dictionary<int, (int, int)>();
        _piecesPlaced = new List<GameObject>();

        Agent1.player = 1;
        Agent2.player = 2;

        //if(UseRandomForAgent2) Agent2 = null;

        InitializeNewGame();

        // setup columnToIndex dict
        SetupColumnToIndexDictionary();
    }

    /// <summary>
    /// Starts a fresh game.
    /// </summary>
    private void InitializeNewGame()
    {
        _gameOver = false;
        BoardState = new GameBoard(4);
        FullColumns = new List<int>();
        Turn = true;
        TurnNumber = 0;

        // Clear all pieces from scene
        foreach (GameObject piece in _piecesPlaced)
            GameObject.Destroy(piece);

        _piecesPlaced = new List<GameObject>();

        waitForChoice = !HumanPlayer;
    }

    // Because the agent sometimes tries to place pieces in columns that are full,
    // this method is needed to "re-ask" the agent for another choice that is hopefully valid this time.
    public void AgentAction(int choice)
    {
        if (PlacePiece(choice))
            waitForChoice = false;
    }

    // Called every frame, handles the game loop.
    void Update()
    {
        if (_gameOver) InitializeNewGame();

        if (!HumanPlayer)
        {
            Sigma4Agent agent = Turn ? Agent1 : Agent2;
            if(UseRandomForAgent2 && !Turn){
                System.Random rand = new System.Random();
                int col = rand.Next(1, 16);
                AgentAction(col);
            }
            else
                agent.RequestDecision();
            waitForChoice = true;
        }

        if (!waitForChoice)
        {
            if (Turn)
                waitForChoice = true;
            else
            {
                Agent2.RequestDecision();
                waitForChoice = true;
            }
        }

    }

    /// <summary>
    /// Updates BoardState
    // </summary>
    /// <param name="column"></param>
    /// <returns>True if piece placement is valid</returns>
    private bool UpdateBoardState(int column)
    {
        if (FullColumns.Exists(c => c == column)) return false;

        (int, int) colTup = _columnToIndex[column];
        int colX = colTup.Item1;
        int colZ = colTup.Item2;

        // OpenY is slice height
        int openY = 0;

        // check if column is full
        for (int i = 0; i < Size; i++)
            if (BoardState.GetSpot(colX, colZ, i) == 0)
            {
                openY = i;
                break;
            }

        // if openY is 3, add column to FullColumns
        if (openY == 3) FullColumns.Add(column);

        // Update BoardState.GetSpot(and r)turn success
        BoardState.SetSpot(colX, colZ, openY, Turn ? 1 : -1);
        return true;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="column"></param>
    public bool PlacePiece(int index)
    {
        if (!UpdateBoardState(index))
            return false;
        
        // Spawn game piece in scene if enabled.
        if(VisualizeGame){
            Vector3 spawnLocation = SpawnLoc[index].transform.position + new Vector3(0f, 3.2f, 0f);
            GameObject newPiece = Instantiate(Turn ? Player1Piece : Player2Piece, spawnLocation, Quaternion.Euler(-90, 0, 0));
            _piecesPlaced.Add(newPiece);
        }
        
        // goal check
        int check = CheckGoalState();

        // Somebody won!!!!
        if (check != 0)
        {
            // win/loss reward/penalty assigning
            bool winningAgent = check == 1;

            // Grab Haste Values if enabled
            if(UseHaste){
                float haste = CollectHasteValue();
                if(haste > 0.8f) Debug.Log("SOME AGENT JUST WON IN LESS THAN 6 MOVES!!!");
                haste*=hasteMultiplier;
                AddAgentReward(winningAgent, haste);
                AddAgentReward(!winningAgent, -1 * haste);
            }

            // default win/lose rewards
            AddAgentReward(winningAgent, 1f);
            AddAgentReward(!winningAgent, -1f);

            Debug.Log("Winner: Agent " + check);

            if (HumanPlayer)
                Agent1.EndEpisode();
            else
                Agent1.EndEpisode(); Agent2.EndEpisode();

            _gameOver = true;
        } 
        
        // Tie case 
        if (_piecesPlaced.Count == 64 || FullColumns.Count == 16)
        {
            _gameOver = true;
            Debug.Log("Tie!!");

            //Agent1.AddReward(0f);
            AddAgentReward(true, 0f);
            Agent1.EndEpisode();

            if (HumanPlayer) return true;

            //Agent2.AddReward(0f);
            AddAgentReward(false, 0f);
            Agent2.EndEpisode();            
        }

        // If no goal was reached, proceed with next turn
        if (!_gameOver)
        {
            // Calc opScores if enabled
            if(UseOpportunityScore){
                // Get current agent
                Sigma4Agent currentAgent = Turn ? Agent1 : Agent2;

                BoardState.CalculateOpportunityScores();

                // Add opportunity reward
                (float, float) opps = BoardState.GetMaxOpportunityScores();
                float opportunity = Turn ? opps.Item1 : opps.Item2;
                AddAgentReward(Turn, opportunity / 2);
            }
            
            Turn = !Turn; // Switch turn
            TurnNumber++; 
            // Human player made a move, no need to wait. or...
            // Wait for AI decision if it's the AI's turn
            waitForChoice = !HumanPlayer;
        }

        return true;
    }

    /// <summary>
    /// Checks if any player has accomplished a connect 4.
    /// </summary>
    /// <returns>0 = No Connect4, 1 = Player1 wins, 2 = Player2 wins</returns>
    private int CheckGoalState()
    {
        #region Single and Dual Variable Checks
        // Check x oriented 
        for (int x = 0; x < Size; x++)
        {
            // check for (-z,-y) to (z,y) diagonals
            if (BoardState.GetSpot(x, 0, 0) != 0
            && BoardState.GetSpot(x, 0, 0) == BoardState.GetSpot(x, 1, 1)
            && BoardState.GetSpot(x, 1, 1) == BoardState.GetSpot(x, 2, 2)
            && BoardState.GetSpot(x, 2, 2) == BoardState.GetSpot(x, 3, 3))
                return BoardState.GetSpot(x, 0, 0);

            // check for (-z,y) to (z,-y) diagonals
            if (BoardState.GetSpot(x, 0, 3) != 0
            && BoardState.GetSpot(x, 0, 3) == BoardState.GetSpot(x, 1, 2)
            && BoardState.GetSpot(x, 1, 2) == BoardState.GetSpot(x, 2, 1)
            && BoardState.GetSpot(x, 2, 1) == BoardState.GetSpot(x, 3, 0))
                return BoardState.GetSpot(x, 0, 3);

            // check for -y to +y columns
            for (int z = 0; z < Size; z++)
                if (BoardState.GetSpot(x, z, 0) == 0) continue;
                else if (BoardState.GetSpot(x, z, 0) == BoardState.GetSpot(x, z, 1)
                && BoardState.GetSpot(x, z, 1) == BoardState.GetSpot(x, z, 2)
                && BoardState.GetSpot(x, z, 2) == BoardState.GetSpot(x, z, 3))
                    return BoardState.GetSpot(x, z, 0);

            // check for -z to +z rows
            for (int y = 0; y < Size; y++)
                if (BoardState.GetSpot(x, 0, y) == 0) continue;
                else if (BoardState.GetSpot(x, 0, y) == BoardState.GetSpot(x, 1, y)
                && BoardState.GetSpot(x, 1, y) == BoardState.GetSpot(x, 2, y)
                && BoardState.GetSpot(x, 2, y) == BoardState.GetSpot(x, 3, y))
                    return BoardState.GetSpot(x, 0, y);
        }

        // Check z oriented
        for (int z = 0; z < Size; z++)
        {
            // Check for (-x,-y) to (x,y) diagonals
            if (BoardState.GetSpot(0, z, 0) != 0
            && BoardState.GetSpot(0, z, 0) == BoardState.GetSpot(1, z, 1)
            && BoardState.GetSpot(1, z, 1) == BoardState.GetSpot(2, z, 2)
            && BoardState.GetSpot(2, z, 2) == BoardState.GetSpot(3, z, 3))
                return BoardState.GetSpot(0, z, 0);

            // Check for (-x,y) to (x,-y) diagonals
            if (BoardState.GetSpot(0, z, 3) != 0
            && BoardState.GetSpot(0, z, 3) == BoardState.GetSpot(1, z, 2)
            && BoardState.GetSpot(1, z, 2) == BoardState.GetSpot(2, z, 1)
            && BoardState.GetSpot(2, z, 1) == BoardState.GetSpot(3, z, 0))
                return BoardState.GetSpot(0, z, 3);

            // Check for -x to +x rows    
            for (int y = 0; y < Size; y++)
                if (BoardState.GetSpot(0, z, y) == 0) continue;
                else if (BoardState.GetSpot(0, z, y) == BoardState.GetSpot(1, z, y)
                && BoardState.GetSpot(1, z, y) == BoardState.GetSpot(2, z, y)
                && BoardState.GetSpot(2, z, y) == BoardState.GetSpot(3, z, y))
                    return BoardState.GetSpot(0, z, y);
        }

        // Check y oriented
        for (int y = 0; y < Size; y++)
        {
            // Check for (-x,-z) to (x,z) diagonals
            if (BoardState.GetSpot(0, 0, y) != 0
            && BoardState.GetSpot(0, 0, y) == BoardState.GetSpot(1, 1, y)
            && BoardState.GetSpot(1, 1, y) == BoardState.GetSpot(2, 2, y)
            && BoardState.GetSpot(2, 2, y) == BoardState.GetSpot(3, 3, y))
                return BoardState.GetSpot(0, 0, y);

            // Check for (-x,z) to (x,-z) diagonals
            if (BoardState.GetSpot(0, 3, y) != 0
            && BoardState.GetSpot(0, 3, y) == BoardState.GetSpot(1, 2, y)
            && BoardState.GetSpot(1, 2, y) == BoardState.GetSpot(2, 1, y)
            && BoardState.GetSpot(2, 1, y) == BoardState.GetSpot(3, 0, y))
                return BoardState.GetSpot(0, 3, y);
        }
        #endregion

        #region Hard Diagonal Tests
        // Check (-x,-z,-y) to (x,z,y) diagonal
        if (BoardState.GetSpot(0, 0, 0) != 0
        && BoardState.GetSpot(0, 0, 0) == BoardState.GetSpot(1, 1, 1)
        && BoardState.GetSpot(1, 1, 1) == BoardState.GetSpot(2, 2, 2)
        && BoardState.GetSpot(2, 2, 2) == BoardState.GetSpot(3, 3, 3))
            return BoardState.GetSpot(0, 0, 0);

        // Check (-x,-z,y) to (x,z,-y) diagonal
        if (BoardState.GetSpot(0, 0, 3) != 0
        && BoardState.GetSpot(0, 0, 3) == BoardState.GetSpot(1, 1, 2)
        && BoardState.GetSpot(1, 1, 2) == BoardState.GetSpot(2, 2, 1)
        && BoardState.GetSpot(2, 2, 1) == BoardState.GetSpot(3, 3, 0))
            return BoardState.GetSpot(0, 0, 3);

        // Check (x,-z,-y) to (-x,z,y) diagonal
        if (BoardState.GetSpot(3, 0, 0) != 0
        && BoardState.GetSpot(3, 0, 0) == BoardState.GetSpot(2, 1, 1)
        && BoardState.GetSpot(2, 1, 1) == BoardState.GetSpot(1, 2, 2)
        && BoardState.GetSpot(1, 2, 2) == BoardState.GetSpot(0, 3, 3))
            return BoardState.GetSpot(3, 0, 0);

        // Check (-x,z,-y) to (x,-z,y) diagonal
        if (BoardState.GetSpot(0, 3, 0) != 0
        && BoardState.GetSpot(0, 3, 0) == BoardState.GetSpot(1, 2, 1)
        && BoardState.GetSpot(1, 2, 1) == BoardState.GetSpot(2, 1, 2)
        && BoardState.GetSpot(2, 1, 2) == BoardState.GetSpot(3, 0, 3))
            return BoardState.GetSpot(0, 3, 0);

        #endregion

        // return 0 if no connect4 is found
        return 0;
    }

    /// <summary>
    /// Populates the _columnToIndex dicionary
    /// </summary>
    private void SetupColumnToIndexDictionary()
    {
        int currentCol = 0;
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                _columnToIndex.Add(currentCol, (i, j));
                currentCol++;
            }
        }
    }


    // Winning agent should recieve H + 1 reward, where H is the float returned by this method.
    // Losing agent should recieve -H + -1 reward.
    private float CollectHasteValue(){
        float div2 = TurnNumber / 2;
        float ugh = 32 - div2;
        float fin = ugh / 32;

        return fin;
    }

    private void AddAgentReward(bool player, float reward){
        if(UseRandomForAgent2 && !player) return;

        Sigma4Agent agent = player ? Agent1 : Agent2;
        agent.AddReward(reward);
    }


}
