using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Debug Parameters")]
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int amountToConnect;
    [SerializeField] Tile tile;
    [SerializeField] bool useDebugSettings;
    int maximumDepth = 5;

    [Header("Object References")]
    [SerializeField] InputField input;
    [SerializeField] Text winText;
    [SerializeField] InputField widthInput;
    [SerializeField] InputField heightInput;
    [SerializeField] InputField RInput;
    [SerializeField] Toggle player1AI;
    [SerializeField] Toggle player2AI;
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject GameMenu;

    bool player1Turn;

    Tile[] tileObjects;
    List<TileClass> allTiles = new List<TileClass>();

    TreeNode<List<TileClass>> moveTree;

    private void Start()
    {
        //generateGrid();
        player1Turn = true;
        winText.gameObject.SetActive(false);
        tileObjects = FindObjectsOfType<Tile>();
        moveTree = new TreeNode<List<TileClass>>(allTiles);
    }

    public void generateGrid()
    {
        MainMenu.SetActive(false);
        GameMenu.SetActive(true);

        if (!useDebugSettings)
        {
            width = int.Parse(widthInput.text);
            height = int.Parse(heightInput.text);
            amountToConnect = int.Parse(RInput.text);
        }


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tile, new Vector3(x, y), Quaternion.identity);
                TileClass tempStruct = new TileClass();
                spawnedTile.name = $"Tile {x} {y}";
                spawnedTile.width = width;
                spawnedTile.height = height;
                spawnedTile.gridManager = this;

                tempStruct.x = x;
                tempStruct.y = y;
                tempStruct.currentState = TileClass.states.none;
                allTiles.Add(tempStruct);
            }
        }
        float avg = (width + height) / 1.5f;
        GameObject.FindGameObjectWithTag("MainCamera").transform.position = new Vector3(width/2f - .5f, height/2f -.5f, - avg);
    }

    Tile getTileObject(TileClass t)
    {
        foreach(Tile tile in tileObjects)
        {
            if (tile.transform.position.x == t.x && tile.transform.position.y == t.y)
            {
                return tile;
            }
        }

        return null; 
    }

    public TileClass getTileAtPosition(int x, int y)
    {
        foreach(TileClass t in allTiles)
        {
            if (t.x == x && t.y == y)
            {
                
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// gets the lowest unoccupied tile in the column
    /// </summary>
    public TileClass getLowest(int x, int y)
    {
        if (getTileAtPosition(x, y).currentState != TileClass.states.none)
        {
            return getTileAtPosition(x, y + 1);
        }
        else if (y <= 0)
        {
            return getTileAtPosition(x, y);
        }
        else
        {
            return getLowest(x, y - 1);
        }
    }

    public void placeToken()
    {
        TileClass t = getLowest(int.Parse(input.text), height - 1);

        if (int.Parse(input.text) < height)
        {
            t = getLowest(int.Parse(input.text), height - 1);
        }

        if (t != null)
        {
            if (player1Turn)
            {
                t.currentState = TileClass.states.player1;
            }
            else
            {
                t.currentState = TileClass.states.player2;
            }

            int highest = getHighestMatch(t);


            if (highest >= amountToConnect)
            {
                if (player1Turn)
                {
                    //player 1 wins
                    winText.text = "PLAYER 1 WINS!";
                    winText.gameObject.SetActive(true);
                }
                else
                {
                    //player 2 wins
                    winText.text = "PLAYER 2 WINS!";
                    winText.gameObject.SetActive(true);
                }
            }


            print(heuristic(allTiles, player1Turn));

            if (player1Turn)
            {
                getTileObject(t).GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                getTileObject(t).GetComponent<SpriteRenderer>().color = Color.yellow;
            }

            player1Turn = !player1Turn;
        }

    }

    int getHighestMatch(TileClass t)
    {
        int currentMax = 0;

        //check up and down
        while (getTileAtPosition((int)t.x, (int)t.y - 1) != null && getTileAtPosition((int)t.x, (int)t.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x, (int)t.y - 1);
        }

        int numInRow = 1;
        while (getTileAtPosition((int)t.x, (int)t.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x, (int)t.y + 1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        //check left and right
        while (getTileAtPosition((int)t.x-1, (int)t.y) != null && getTileAtPosition((int)t.x-1, (int)t.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x-1, (int)t.y);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.x + 1, (int)t.y) != null && getTileAtPosition((int)t.x+1, (int)t.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x+1, (int)t.y);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        //check diagonal left
        while (getTileAtPosition((int)t.x-1, (int)t.y - 1) != null && getTileAtPosition((int)t.x - 1, (int)t.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x - 1, (int)t.y-1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.x + 1, (int)t.y + 1) != null && getTileAtPosition((int)t.x + 1, (int)t.y+1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x + 1, (int)t.y+1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        //check diagonal right
        while (getTileAtPosition((int)t.x+1, (int)t.y - 1) != null && getTileAtPosition((int)t.x + 1, (int)t.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x + 1, (int)t.y - 1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.x - 1, (int)t.y + 1) != null && getTileAtPosition((int)t.x - 1, (int)t.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x - 1, (int)t.y + 1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        return currentMax;
    }

    int heuristic(List<TileClass> inputBoard, bool isPlayer1)
    {
        int score = 0;

        foreach(TileClass t in inputBoard)
        {
            if (isPlayer1)
            {
                if (t.currentState == TileClass.states.player1)
                {
                    //how close to the center is it
                    score -= (int)Mathf.Abs(t.x - (float)width) * 30;

                    //how many total are in a row for each direction with better score for the higher the amount in a row is 
                    score += getAmountInEachDirection(t) * 2;


                    //how many can be in a row in the future
                    score += getHighestWithBlanks(t);


                }
                else if (t.currentState == TileClass.states.player2)
                {
                    //how close to the center is it
                    score += (int)Mathf.Abs(t.x - (float)width) * 30;

                    //how many total are in a row for each direction with better score for the higher the amount in a row is 
                    score -= getAmountInEachDirection(t) * 2;

                }
            }
            else
            {
                if (t.currentState == TileClass.states.player1)
                {
                    //how close to the center is it
                    score += (int)Mathf.Abs(t.x - (float)width) * 30;

                    //how many total are in a row for each direction with better score for the higher the amount in a row is 
                    score -= getAmountInEachDirection(t) * 2;


                    //how many can be in a row in the future
                    score -= getHighestWithBlanks(t);
                }
                else if (t.currentState == TileClass.states.player2)
                {
                    //how close to the center is it
                    score -= (int)Mathf.Abs(t.x - (float)width) * 30;

                    //how many total are in a row for each direction with better score for the higher the amount in a row is 
                    score += getAmountInEachDirection(t) * 2;
                }
            }
        }
        return score;
    }


    int getAmountInEachDirection(TileClass t)
    {
        int score = 0;

        int currentMax = 0;

        //check up and down
        while (getTileAtPosition((int)t.x, (int)t.y - 1) != null && getTileAtPosition((int)t.x, (int)t.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x, (int)t.y - 1);
        }

        int numInRow = 1;
        while (getTileAtPosition((int)t.x, (int)t.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x, (int)t.y + 1);
            numInRow++;
        }

        score += (int )Mathf.Pow(currentMax, 2);
        currentMax = 0;

        //check left and right
        while (getTileAtPosition((int)t.x - 1, (int)t.y) != null && getTileAtPosition((int)t.x - 1, (int)t.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x - 1, (int)t.y);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.x + 1, (int)t.y) != null && getTileAtPosition((int)t.x + 1, (int)t.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x + 1, (int)t.y);
            numInRow++;
        }

        score += (int)Mathf.Pow(currentMax, 2);
        currentMax = 0;

        //check diagonal left
        while (getTileAtPosition((int)t.x - 1, (int)t.y - 1) != null && getTileAtPosition((int)t.x - 1, (int)t.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x - 1, (int)t.y - 1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.x + 1, (int)t.y + 1) != null && getTileAtPosition((int)t.x + 1, (int)t.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x + 1, (int)t.y + 1);
            numInRow++;
        }

        score += (int)Mathf.Pow(currentMax, 2);
        currentMax = 0;

        //check diagonal right
        while (getTileAtPosition((int)t.x + 1, (int)t.y - 1) != null && getTileAtPosition((int)t.x + 1, (int)t.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x + 1, (int)t.y - 1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.x - 1, (int)t.y + 1) != null && getTileAtPosition((int)t.x - 1, (int)t.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x - 1, (int)t.y + 1);
            numInRow++;
        }

        score += (int)Mathf.Pow(currentMax, 2);

        return score;
    }

    int getHighestWithBlanks(TileClass t)
    {
         int currentMax = 0;

        
        int numInRow = 1;
        int numNull = 0;
       

        //check left and right
        while (getTileAtPosition((int)t.x-1, (int)t.y) != null && getTileAtPosition((int)t.x-1, (int)t.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x-1, (int)t.y);
        }

        numInRow = 1;
        numNull = 0;
        while (getTileAtPosition((int)t.x + 1, (int)t.y) != null && (getTileAtPosition((int)t.x+1, (int)t.y).currentState == t.currentState || getTileAtPosition((int)t.x+1, (int)t.y).currentState == TileClass.states.none))
        {
            if (t.currentState == TileClass.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.x+1, (int)t.y);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);
        if (numInRow == amountToConnect - 1 && numNull == 1)
        {
            print("LR");
            return 1000;
        }

        //check diagonal left
        while (getTileAtPosition((int)t.x-1, (int)t.y - 1) != null && getTileAtPosition((int)t.x - 1, (int)t.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x - 1, (int)t.y-1);
        }

        numInRow = 1;
        numNull = 0;
        while (getTileAtPosition((int)t.x + 1, (int)t.y + 1) != null && (getTileAtPosition((int)t.x + 1, (int)t.y+1).currentState == t.currentState || getTileAtPosition((int)t.x+1, (int)t.y+1).currentState == TileClass.states.none))
        {
            if (t.currentState == TileClass.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.x + 1, (int)t.y+1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);
        if (numInRow == amountToConnect - 1 && numNull == 1)
        {
            print("DL");
            return 1000;
        }

        //check diagonal right
        while (getTileAtPosition((int)t.x+1, (int)t.y - 1) != null && getTileAtPosition((int)t.x + 1, (int)t.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.x + 1, (int)t.y - 1);
        }

        numInRow = 1;
        numNull = 0;
        while (getTileAtPosition((int)t.x - 1, (int)t.y + 1) != null && (getTileAtPosition((int)t.x - 1, (int)t.y+1).currentState == t.currentState || getTileAtPosition((int)t.x-1, (int)t.y + 1).currentState == TileClass.states.none))
        {
            if (t.currentState == TileClass.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.x - 1, (int)t.y + 1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);
        if (numInRow == amountToConnect - 1 && numNull == 1)
        {
            print("DR");
            return 1000;
        }

        return currentMax * 10;
    }

    void makeTree()
    {
        //try all of the possible moves that can be made
        //the number of potential moves a player can do is equal to the width of the board
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < maximumDepth; j++)
            {
                //add the tile to the a new identical board at position i
                List<TileClass> newBoard = new List<TileClass>();


                moveTree[j].AddChild(newBoard);
            }
        }

    }

    int miniMax(int depth, bool maxingPlayer)
    {
        int score = 0;
        int bestScore = 0;
        int bestMove = -1;

        //check if terminal and return heuristic value



        //evaluate all possible moves at this depth
        foreach(TreeNode<List<TileClass>> board in moveTree[depth].Children)
        {
            //call minimaax on that new board

            //check if that move is better than any of the other moves and overwrite it
            if (maxingPlayer)
            {

            }
            else
            {

            }
        }



        //do and return the best move

        return score;
    }
}
