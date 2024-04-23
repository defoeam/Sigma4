using System.Collections;
using System.Collections.Generic;
<<<<<<< Updated upstream
=======
using System.Security.Cryptography;
using Unity.MLAgents;
using Unity.VisualScripting;
>>>>>>> Stashed changes
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // test
    public Sigma4Agent Agent1;
    public Sigma4Agent Agent2;


    // Start is called before the first frame update
<<<<<<< Updated upstream
    void Start()
    {
=======
    public GameObject Player1Piece;
    public GameObject Player2Piece;
    public GameObject[] SpawnLoc;

    /// <summary>
    /// true = Player1 turn, false = Player2 turn.
    /// </summary>
    public bool Turn = true;
    public int Size = 4;

    // [0, 0, 0] int at height 0 in column (x=0, y=0)
    int[,,] BoardState;
    private Dictionary<int, (int,int)> _columnToIndex;
    private List<GameObject> _piecesPlaced;


    void Start() {
        _columnToIndex = new Dictionary<int, (int, int)>();
        _piecesPlaced = new List<GameObject>();

        
        InitializeNewGame();

        // setup columnToIndex dict
        SetupColumnToIndexDictionary();
    }

    /// <summary>
    /// Starts a fresh game.
    /// </summary>
    private void InitializeNewGame() {
        BoardState = new int[Size, Size, Size]; 
        Turn = true;

        // Clear all pieces from scene
        foreach(GameObject piece in _piecesPlaced) 
            GameObject.Destroy(piece);

        _piecesPlaced = new List<GameObject>();
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

    // We could potentially use the update method to handle turn cooldowns
    // i.e., to stop rapid-clicking to reach impossible board states.
    void Update() {
>>>>>>> Stashed changes
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

<<<<<<< Updated upstream
    public void SelectColumn(int column)
    {
        Debug.Log("Gamemanager Colum " + column);
        
=======
    /// <summary>
    /// 
    /// </summary>
    /// <param name="column"></param>
    public void PlacePiece(int column) {
        if(!UpdateBoardState(column)) return;

        // Spawn game piece in scene.
        GameObject newPiece = Instantiate(Turn ? Player1Piece : Player2Piece, SpawnLoc[column - 1].transform.position, Quaternion.identity);
        _piecesPlaced.Add(newPiece);
        Turn = !Turn; // change turns

        // Check if game is over:
        int goal = CheckGoalState();

        if(goal != 0){
            Debug.Log("Winner: Player" + goal);
            InitializeNewGame();
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

        #region Hard Diagonal Checks
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

    private void Print3DIntArray(int[,,] arr){
        for(int h = 0; h < Size; h++){
            Debug.Log("[");
            for(int x = 0; x < Size; x++){
                Debug.Log("  [");
                for(int y = 0; y < Size; y++){
                    Debug.Log(arr[h,x,y]);
                }
                Debug.Log("  ]");
            }
            Debug.Log("]");
        }
        Debug.Log("-----------------------");
>>>>>>> Stashed changes
    }
}
