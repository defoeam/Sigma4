using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Player1Piece;
    public GameObject Player2Piece;
    public GameObject[] SpawnLoc;

    public Sigma4Agent Agent1;
    public Sigma4Agent Agent2;

    /// <summary>
    /// true = Player1 turn, false = Player2 turn.
    /// </summary>
    public bool Turn = true;
    public int Size = 4;
    public bool HumanPlayer = false;

    
    public GameBoard BoardState;

    public List<int> FullColumns;
    private Dictionary<int, (int,int)> _columnToIndex;
    private List<GameObject> _piecesPlaced;
    private bool waitForChoice = false;
    private bool _gameOver = false;

    //test
    private static List<string> exploredStates = new List<string>();


    void Start() {
        _columnToIndex = new Dictionary<int, (int, int)>();
        _piecesPlaced = new List<GameObject>();

        if (HumanPlayer)
        {
            Agent1.player = 1;
            Agent2.player = 2;
        }
        else
        {
            Agent1.player = 2;
            Agent2.player = 1;
        }

        InitializeNewGame();

        // setup columnToIndex dict
        SetupColumnToIndexDictionary();
    }

    /// <summary>
    /// Starts a fresh game.
    /// </summary>
    private void InitializeNewGame() {
        _gameOver = false;
        BoardState = new GameBoard(4);
        FullColumns = new List<int>();
        Turn = true;

        // Clear all pieces from scene
        foreach(GameObject piece in _piecesPlaced) 
            GameObject.Destroy(piece);

        _piecesPlaced = new List<GameObject>();

        waitForChoice = !HumanPlayer;
    }


    public void AgentAction(int choice){
        if(PlacePiece(choice))
            waitForChoice = false;
    }
    
    void Update() {
        if(_gameOver) InitializeNewGame();

        if (!HumanPlayer)
        {
            Sigma4Agent agent = Turn ? Agent1 : Agent2;
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
    private bool UpdateBoardState(int column) {
        if(FullColumns.Exists(c => c == column)) return false;

        (int,int) colTup = _columnToIndex[column];
        int colX = colTup.Item1;
        int colZ = colTup.Item2;

        // OpenY is slice height
        int openY = 0;

        // check if column is full
        for(int i = 0; i < Size; i++)
            if(BoardState.GetSpot(colX,colZ, i) == 0){
                openY = i;
                break;
            }

        // if openY is 3, add column to FullColumns
        if(openY == 3) FullColumns.Add(column);

        // Update BoardState.GetSpot(and r)turn success
        BoardState.SetSpot(colX, colZ, openY, Turn ? 1 : -1); 
        return true;
    }

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="column"></param>
    public bool PlacePiece(int column) {
        if(!UpdateBoardState(column)){
            return false;
        }

        // Get current agent
        Sigma4Agent currentAgent = Turn ? Agent1 : Agent2;

        // Check if new state was explored
        string boardStr = BoardState.StateToString();
        if(!exploredStates.Contains(boardStr)){
            exploredStates.Add(boardStr);
            currentAgent.AddReward(0.2f);
            //Debug.Log($"Explored States length: {exploredStates.Count}");
        }

        // Add opportunity reward
        (float, float) opps = BoardState.GetAverageOpportunityScores();
        float opportunity = Turn ? opps.Item1 : opps.Item2;
        //Debug.Log($"Player {currentAgent.player}'s opp: {opportunity}");
        currentAgent.AddReward(opportunity);

        // Spawn game piece in scene.
        GameObject newPiece = Instantiate(Turn ? Player1Piece : Player2Piece, SpawnLoc[column - 1].transform.position, Quaternion.identity);
        _piecesPlaced.Add(newPiece);

        // goal check
        int check = CheckGoalState();
        
        if(check != 0){
            // win/loss reward/penalty assigning
            Sigma4Agent winningAgent = check == 1 ? Agent1 : Agent2;
            Sigma4Agent losingAgent = check == 1 ? Agent2 : Agent1;
            winningAgent.AddReward(10f);
            losingAgent.AddReward(-10f);

            Debug.Log("Winner: Agent " + check);

            if(HumanPlayer)
                Agent1.EndEpisode();
            else
                Agent1.EndEpisode(); Agent2.EndEpisode();
            
            _gameOver = true;
            
        } // Tie case 
        
        if(check == 0 &&_piecesPlaced.Count == 64){
            Debug.Log("Tie!!");
            if (HumanPlayer)
                Agent1.EndEpisode();
            else
                Agent1.EndEpisode(); Agent2.EndEpisode();

            _gameOver = true;
        }

        if (!_gameOver)
        {
            Turn = !Turn; // Switch turn

            if (HumanPlayer)
                waitForChoice = false; // Human player made a move, no need to wait
            else
                waitForChoice = true; // Wait for AI decision if it's the AI's turn
        }

        return true;

    }

    /// <summary>
    /// Checks if any player has accomplished a connect 4.
    /// </summary>
    /// <returns>0 = No Connect4, 1 = Player1 wins, 2 = Player2 wins</returns>
    private int CheckGoalState() { 
        #region Single and Dual Variable Checks
        // Check x oriented 
        for(int x = 0; x < Size; x++){
            // check for (-z,-y) to (z,y) diagonals
            if(BoardState.GetSpot(x,0,0) != 0 
            && BoardState.GetSpot(x,0,0) == BoardState.GetSpot(x,1,1)
            && BoardState.GetSpot(x,1,1) == BoardState.GetSpot(x,2,2) 
            && BoardState.GetSpot(x,2,2) == BoardState.GetSpot(x,3,3))
                return BoardState.GetSpot(x,0,0);
            
            // check for (-z,y) to (z,-y) diagonals
            if(BoardState.GetSpot(x,0,3) != 0 
            && BoardState.GetSpot(x,0,3) == BoardState.GetSpot(x,1,2)
            && BoardState.GetSpot(x,1,2) == BoardState.GetSpot(x,2,1)
            && BoardState.GetSpot(x,2,1) == BoardState.GetSpot(x,3,0))
                return BoardState.GetSpot(x,0,3);

            // check for -y to +y columns
            for(int z = 0; z < Size; z++)
                if(BoardState.GetSpot(x,z,0) == 0) continue;
                else if(BoardState.GetSpot(x,z,0) == BoardState.GetSpot(x,z,1) 
                && BoardState.GetSpot(x,z,1) == BoardState.GetSpot(x,z,2) 
                && BoardState.GetSpot(x,z,2) == BoardState.GetSpot(x,z,3))
                    return BoardState.GetSpot(x,z,0);
            
            // check for -z to +z rows
            for(int y = 0; y < Size; y++)
                if(BoardState.GetSpot(x,0,y) == 0) continue;
                else if(BoardState.GetSpot(x,0,y) == BoardState.GetSpot(x,1,y) 
                && BoardState.GetSpot(x,1,y) == BoardState.GetSpot(x,2,y) 
                && BoardState.GetSpot(x,2,y) == BoardState.GetSpot(x,3,y))
                    return BoardState.GetSpot(x,0,y);
        }
            
        // Check z oriented
        for(int z = 0; z < Size; z++){
            // Check for (-x,-y) to (x,y) diagonals
            if(BoardState.GetSpot(0,z,0) != 0
            && BoardState.GetSpot(0,z,0) == BoardState.GetSpot(1,z,1)
            && BoardState.GetSpot(1,z,1) == BoardState.GetSpot(2,z,2)
            && BoardState.GetSpot(2,z,2) == BoardState.GetSpot(3,z,3))
                return BoardState.GetSpot(0,z,0);

            // Check for (-x,y) to (x,-y) diagonals
            if(BoardState.GetSpot(0,z,3) != 0
            && BoardState.GetSpot(0,z,3) == BoardState.GetSpot(1,z,2)
            && BoardState.GetSpot(1,z,2) == BoardState.GetSpot(2,z,1)
            && BoardState.GetSpot(2,z,1) == BoardState.GetSpot(3,z,0))
                return BoardState.GetSpot(0,z,3);
            
            // Check for -x to +x rows    
            for(int y = 0; y < Size; y++)
                if(BoardState.GetSpot(0,z,y) == 0) continue;
                else if(BoardState.GetSpot(0,z,y) == BoardState.GetSpot(1,z,y) 
                && BoardState.GetSpot(1,z,y) == BoardState.GetSpot(2,z,y) 
                && BoardState.GetSpot(2,z,y) == BoardState.GetSpot(3,z,y))
                    return BoardState.GetSpot(0,z,y);
        }
            
        // Check y oriented
        for(int y = 0; y < Size; y++){
            // Check for (-x,-z) to (x,z) diagonals
            if(BoardState.GetSpot(0,0,y) != 0
            && BoardState.GetSpot(0,0,y) == BoardState.GetSpot(1,1,y)
            && BoardState.GetSpot(1,1,y) == BoardState.GetSpot(2,2,y)
            && BoardState.GetSpot(2,2,y) == BoardState.GetSpot(3,3,y))
                return BoardState.GetSpot(0,0,y);

            // Check for (-x,z) to (x,-z) diagonals
            if(BoardState.GetSpot(0,3,y) != 0
            && BoardState.GetSpot(0,3,y) == BoardState.GetSpot(1,2,y)
            && BoardState.GetSpot(1,2,y) == BoardState.GetSpot(2,1,y)
            && BoardState.GetSpot(2,1,y) == BoardState.GetSpot(3,0,y))
                return BoardState.GetSpot(0,3,y);
        }
        #endregion

        #region Hard Diagonal Tests
        // Check (-x,-z,-y) to (x,z,y) diagonal
        if(BoardState.GetSpot(0,0,0) != 0
        && BoardState.GetSpot(0,0,0) == BoardState.GetSpot(1,1,1)
        && BoardState.GetSpot(1,1,1) == BoardState.GetSpot(2,2,2)
        && BoardState.GetSpot(2,2,2) == BoardState.GetSpot(3,3,3))
            return BoardState.GetSpot(0,0,0);

        // Check (-x,-z,y) to (x,z,-y) diagonal
        if(BoardState.GetSpot(0,0,3) != 0
        && BoardState.GetSpot(0,0,3) == BoardState.GetSpot(1,1,2)
        && BoardState.GetSpot(1,1,2) == BoardState.GetSpot(2,2,1)
        && BoardState.GetSpot(2,2,1) == BoardState.GetSpot(3,3,0))
            return BoardState.GetSpot(0,0,3);

        // Check (x,-z,-y) to (-x,z,y) diagonal
        if(BoardState.GetSpot(3,0,0) != 0
        && BoardState.GetSpot(3,0,0) == BoardState.GetSpot(2,1,1)
        && BoardState.GetSpot(2,1,1) == BoardState.GetSpot(1,2,2)
        && BoardState.GetSpot(1,2,2) == BoardState.GetSpot(0,3,3))
            return BoardState.GetSpot(3,0,0);

        // Check (-x,z,-y) to (x,-z,y) diagonal
        if(BoardState.GetSpot(0,3,0) != 0
        && BoardState.GetSpot(0,3,0) == BoardState.GetSpot(1,2,1)
        && BoardState.GetSpot(1,2,1) == BoardState.GetSpot(2,1,2)
        && BoardState.GetSpot(2,1,2) == BoardState.GetSpot(3,0,3))
            return BoardState.GetSpot(0,3,0);

        #endregion

        // return 0 if no connect4 is found
        return 0;
    }

    /// <summary>
    /// Populates the _columnToIndex dicionary
    /// </summary>
    private void SetupColumnToIndexDictionary() {
        int currentCol = 1;
        for(int i = 0; i < Size; i++){
            for(int j = 0; j < Size; j++){
                _columnToIndex.Add(currentCol, (i, j));
                currentCol++;
            }
        }
    }
}
