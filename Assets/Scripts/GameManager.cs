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

    public int[,,] BoardState;
    public List<int> FullColumns;
    private Dictionary<int, (int,int)> _columnToIndex;
    private List<GameObject> _piecesPlaced;
    private bool waitForChoice = false;
    private bool _gameOver = false;


    void Start() {
        _columnToIndex = new Dictionary<int, (int, int)>();
        _piecesPlaced = new List<GameObject>();

        //Agent1 = GetComponent<Sigma4Agent>();
        //Agent2 = GetComponent<Sigma4Agent>();
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
        BoardState = new int[Size, Size, Size]; 
        FullColumns = new List<int>();
        Turn = true;

        // Clear all pieces from scene
        foreach(GameObject piece in _piecesPlaced) 
            GameObject.Destroy(piece);

        _piecesPlaced = new List<GameObject>();

        waitForChoice = !HumanPlayer;
        
        //Dont know if we need this
        // training case
        /*if(Agent1 != null && Agent2 != null)
            waitForChoice = false;*/
            
        
    }


    public void AgentAction(int choice){
        PlacePiece(choice);
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
            {
                waitForChoice = true;
            }
            else
            {
                Agent2.RequestDecision();
                waitForChoice = true;
            }
        }

    }

    /// <summary>
    /// Updates BoardState 
    /// </summary>
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
            if(BoardState[colX, colZ, i] == 0){
                openY = i;
                break;
            }

        // if openY is 3, add column to FullColumns
        if(openY == 3) FullColumns.Add(column);

        // Update BoardState and return success
        BoardState[colX, colZ, openY] = Turn ? 1 : 2;
        return true;
    }

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="column"></param>
    public void PlacePiece(int column) {
        if(!UpdateBoardState(column)){
            Debug.Log("Something unexpected happended...");
            return;
        }

        // Spawn game piece in scene.
        GameObject newPiece = Instantiate(Turn ? Player1Piece : Player2Piece, SpawnLoc[column - 1].transform.position, Quaternion.identity);
        _piecesPlaced.Add(newPiece);

        // goal check
        int check = CheckGoalState();
        
        if(check != 0){
            Sigma4Agent winningAgent = check == 1 ? Agent1 : Agent2;
            winningAgent.AddReward(10f);
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
            {
                waitForChoice = false; // Human player made a move, no need to wait
            }
            else
            {
                waitForChoice = true; // Wait for AI decision if it's the AI's turn
            }
        }

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
            if(BoardState[x,0,0] != 0 
            && BoardState[x,0,0] == BoardState[x,1,1]
            && BoardState[x,1,1] == BoardState[x,2,2] 
            && BoardState[x,2,2] == BoardState[x,3,3])
                return BoardState[x,0,0];
            
            // check for (-z,y) to (z,-y) diagonals
            if(BoardState[x,0,3] != 0 
            && BoardState[x,0,3] == BoardState[x,1,2]
            && BoardState[x,1,2] == BoardState[x,2,1]
            && BoardState[x,2,1] == BoardState[x,3,0])
                return BoardState[x,0,3];

            // check for -y to +y columns
            for(int z = 0; z < Size; z++)
                if(BoardState[x,z,0] == 0) continue;
                else if(BoardState[x,z,0] == BoardState[x,z,1] 
                && BoardState[x,z,1] == BoardState[x,z,2] 
                && BoardState[x,z,2] == BoardState[x,z,3])
                    return BoardState[x,z,0];
            
            // check for -z to +z rows
            for(int y = 0; y < Size; y++)
                if(BoardState[x,0,y] == 0) continue;
                else if(BoardState[x,0,y] == BoardState[x,1,y] 
                && BoardState[x,1,y] == BoardState[x,2,y] 
                && BoardState[x,2,y] == BoardState[x,3,y])
                    return BoardState[x,0,y];
        }
            
        // Check z oriented
        for(int z = 0; z < Size; z++){
            // Check for (-x,-y) to (x,y) diagonals
            if(BoardState[0,z,0] != 0
            && BoardState[0,z,0] == BoardState[1,z,1]
            && BoardState[1,z,1] == BoardState[2,z,2]
            && BoardState[2,z,2] == BoardState[3,z,3])
                return BoardState[0,z,0];

            // Check for (-x,y) to (x,-y) diagonals
            if(BoardState[0,z,3] != 0
            && BoardState[0,z,3] == BoardState[1,z,2]
            && BoardState[1,z,2] == BoardState[2,z,1]
            && BoardState[2,z,1] == BoardState[3,z,0])
                return BoardState[0,z,3];
            
            // Check for -x to +x rows    
            for(int y = 0; y < Size; y++)
                if(BoardState[0,z,y] == 0) continue;
                else if(BoardState[0,z,y] == BoardState[1,z,y] 
                && BoardState[1,z,y] == BoardState[2,z,y] 
                && BoardState[2,z,y] == BoardState[3,z,y])
                    return BoardState[0,z,y];
        }
            
        // Check y oriented
        for(int y = 0; y < Size; y++){
            // Check for (-x,-z) to (x,z) diagonals
            if(BoardState[0,0,y] != 0
            && BoardState[0,0,y] == BoardState[1,1,y]
            && BoardState[1,1,y] == BoardState[2,2,y]
            && BoardState[2,2,y] == BoardState[3,3,y])
                return BoardState[0,0,y];

            // Check for (-x,z) to (x,-z) diagonals
            if(BoardState[0,3,y] != 0
            && BoardState[0,3,y] == BoardState[1,2,y]
            && BoardState[1,2,y] == BoardState[2,1,y]
            && BoardState[2,1,y] == BoardState[3,0,y])
                return BoardState[0,3,y];
        }
        #endregion

        #region Hard Diagonal Tests
        // Check (-x,-z,-y) to (x,z,y) diagonal
        if(BoardState[0,0,0] != 0
        && BoardState[0,0,0] == BoardState[1,1,1]
        && BoardState[1,1,1] == BoardState[2,2,2]
        && BoardState[2,2,2] == BoardState[3,3,3])
            return BoardState[0,0,0];

        // Check (-x,-z,y) to (x,z,-y) diagonal
        if(BoardState[0,0,3] != 0
        && BoardState[0,0,3] == BoardState[1,1,2]
        && BoardState[1,1,2] == BoardState[2,2,1]
        && BoardState[2,2,1] == BoardState[3,3,0])
            return BoardState[0,0,3];

        // Check (x,-z,-y) to (-x,z,y) diagonal
        if(BoardState[3,0,0] != 0
        && BoardState[3,0,0] == BoardState[2,1,1]
        && BoardState[2,1,1] == BoardState[1,2,2]
        && BoardState[1,2,2] == BoardState[0,3,3])
            return BoardState[3,0,0];

        // Check (-x,z,-y) to (x,-z,y) diagonal
        if(BoardState[0,3,0] != 0
        && BoardState[0,3,0] == BoardState[1,2,1]
        && BoardState[1,2,1] == BoardState[2,1,2]
        && BoardState[2,1,2] == BoardState[3,0,3])
            return BoardState[0,3,0];

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
