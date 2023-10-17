using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width, height, amountToConnect, maxDepth;
    [SerializeField] Tile tile;
    [SerializeField] Transform camera;
    [SerializeField] InputField input;
    [SerializeField] Text winText;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject inGameMenu;
    [SerializeField] InputField widthInput;
    [SerializeField] InputField heightInput;
    [SerializeField] InputField connectInput;
    [SerializeField] Toggle player1AI;
    [SerializeField] Toggle player2AI;
    bool player1Turn;
    bool gameStarted = false;

    public void StartGame()
    {
        mainMenu.SetActive(false);
        inGameMenu.SetActive(true);

        width = int.Parse(widthInput.text);
        height = int.Parse(heightInput.text);
        amountToConnect = int.Parse(connectInput.text);

        generateGrid();
        player1Turn = true;
        winText.gameObject.SetActive(false);

        gameStarted = true;
    }

    private void Update()
    {
        if (gameStarted)
        {
            Tile[] board = FindObjectsOfType<Tile>();
            List<Tile> currentBoard = new List<Tile>();

            foreach (Tile t in board)
            {
                currentBoard.Add(t);
            }


            if (player1Turn && player1AI.isOn)
            {
                print("a");
                int useless = miniMax(currentBoard, 0, true);
                player1Turn = false;
            }
            else if (!player1Turn && player2AI.isOn)
            {
                print("b");
                int useless = miniMax(currentBoard, 0, false);
                player1Turn = true;
            }
        }
    }

    void generateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tile, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                spawnedTile.width = width;
                spawnedTile.height = height;
                spawnedTile.gridManager = this;
            }
        }
        float avg = (width + height) / 1.5f;
        camera.position = new Vector3(width/2f - .5f, height/2f -.5f, - avg);
    }

    public void placeToken()
    {
        
        Tile t = getTileAtPosition(int.Parse(input.text), height - 1);

        if (int.Parse(input.text) < height)
        {
            t = getLowest(int.Parse(input.text), height - 1);
        }

        if (t != null)
        {
            if (player1Turn)
            {
                t.currentState = Tile.states.player1;
                t.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                t.currentState = Tile.states.player2;
                t.GetComponent<SpriteRenderer>().color = Color.yellow;
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

            player1Turn = !player1Turn;
        }

    }

    public void placeToken(int position)
    {

        Tile t = getTileAtPosition(position, height - 1);

       

        if (position < height)
        {
            t = getLowest(position, height);
        }


        if (t != null)
        {
            if (player1Turn)
            {
                t.currentState = Tile.states.player1;
                t.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                t.currentState = Tile.states.player2;
                t.GetComponent<SpriteRenderer>().color = Color.yellow;
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

            player1Turn = !player1Turn;
        }

    }

    private int miniMax(List<Tile> board, int depth, bool isMaximizing)
    {
        
        int score = 0;
        int bestScore = 0;
        int bestMove = -1;

        List<Tile> unoccupiedTiles = new List<Tile>();
        List<Tile> player1Tiles = new List<Tile>();
        List<Tile> player2Tiles = new List<Tile>();

        foreach(Tile t in board)
        {
            switch (t.currentState)
            {
                case Tile.states.none:
                    unoccupiedTiles.Add(t);
                    break;
                case Tile.states.player1:
                    player1Tiles.Add(t);
                    break;
                case Tile.states.player2:
                    player2Tiles.Add(t);
                    break;
            }
        }


        //if the game is over then do not do the rest of the algorithm
        if (winText.gameObject.activeSelf == true)
            return -1;

        score = heuristic(board, isMaximizing);

        if(depth > maxDepth)
        {
            return score;
        }

        //if next move is a terminal state, return it
        //foreach (Tile t in board)
        //{
        //    if(getHighestMatch(t) >= amountToConnect)
        //    {
        //        print("hi");
        //        print(getHighestMatch(t));
        //        placeToken((int)t.transform.position.x);
        //        return score;
        //    }
        //}

        if (isMaximizing)
        {
            bestScore = int.MinValue;

            for(int i = 0; i < width; i++)
            {
                Tile toPlace = getLowest(i, height);
                getTileAtPosition((int)toPlace.gameObject.transform.position.x, (int)toPlace.gameObject.transform.position.y).currentState = Tile.states.player1;

                score = miniMax(board, depth + 1, false);
                getTileAtPosition((int)toPlace.gameObject.transform.position.x, (int)toPlace.gameObject.transform.position.y).currentState = Tile.states.none;

                //bestScore = Mathf.Max(bestScore, score);
                if(score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }

            foreach(Tile t in unoccupiedTiles)
            {
                t.currentState = Tile.states.none;
            }
            foreach (Tile t in player1Tiles)
            {
                t.currentState = Tile.states.player1;
            }
            foreach (Tile t in player2Tiles)
            {
                t.currentState = Tile.states.player2;
            }
            placeToken(bestMove);
            return bestScore;
        }
        else
        {
            
            bestScore = int.MaxValue;

            for (int i = 0; i < width; i++)
            {
                Tile toPlace = getLowest(i, height);
                //getTileAtPosition((int)toPlace.gameObject.transform.position.x, (int)toPlace.gameObject.transform.position.y).currentState = Tile.states.player2;
                toPlace.currentState = Tile.states.player2;
                score = miniMax(board, depth + 1, true);
                toPlace.currentState = Tile.states.none;
                //getTileAtPosition((int)toPlace.gameObject.transform.position.x, (int)toPlace.gameObject.transform.position.y).currentState = Tile.states.none;

                //bestScore = Mathf.Min(bestScore, score);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }

            foreach (Tile t in unoccupiedTiles)
            {
                t.currentState = Tile.states.none;
            }
            foreach (Tile t in player1Tiles)
            {
                t.currentState = Tile.states.player1;
            }
            foreach (Tile t in player2Tiles)
            {
                t.currentState = Tile.states.player2;
            }
            placeToken(bestMove);

            Tile bestTile = getLowest(bestScore, height - 1);
            print(bestTile.transform.position.y);

            return bestScore;
        }


        
    }

    public Tile getTileAtPosition(int x, int y)
    {
        Tile[] tiles = FindObjectsOfType<Tile>();

        foreach(Tile t in tiles)
        {
            if(t.transform.position.x == x && t.transform.position.y == y)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// gets the lowest unoccupied tile in the column
    /// </summary>
    public Tile getLowest(int x, int y)
    {
        

        if (getTileAtPosition(x, y) != null && getTileAtPosition(x, y).currentState != Tile.states.none)
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


    int getHighestMatch(Tile t)
    {
        //print(t.name);
        int currentMax = 0;

        //check up and down
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y-1).currentState == t.currentState && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1).currentState != Tile.states.none)
        {
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1);
        }

        int numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1) != null && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1).currentState == t.currentState)
        {
            if (t.transform.position.x >= width - 1 || t.transform.position.y >= height - 1 || t.transform.position.x <= 0 || t.transform.position.y <= 0)
                break;
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        //check left and right
        while (getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y) != null && getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y) != null && getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        //check diagonal left
        while (getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y-1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y + 1) != null && getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y+1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y+1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        //check diagonal right
        while (getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1) != null && getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);

        return currentMax;
    }

    int heuristic(List<Tile> inputBoard, bool isPlayer1)
    {
        int score = 0;

        foreach(Tile t in inputBoard)
        {
            if (isPlayer1)
            {
                if (t.currentState == Tile.states.player1)
                {
                    score += evalTile(t);

                    
                }
                else if (t.currentState == Tile.states.player2)
                {
                    score -= evalTile(t);
                }
            }
            else
            {
                if (t.currentState == Tile.states.player1)
                {
                    score -= evalTile(t);
                }
                else if (t.currentState == Tile.states.player2)
                {
                    score += evalTile(t);
                }
            }
        }
        return score;
    }

    int evalTile(Tile t)
    {
        int score = 0;

        //how close to the center is it
        score -= (int)Mathf.Abs(t.transform.position.x - (float)width) * 30;

        //how many total are in a row for each direction with better score for the higher the amount in a row is 
        score += getAmountInEachDirection(t)*2;


        //how many can be in a row in the future
        score += getHighestWithBlanks(t);


        return score;
    }

    int getAmountInEachDirection(Tile t)
    {
        int score = 0;

        int currentMax = 0;

        //check up and down
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1);
        }

        int numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1) != null && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1);
            numInRow++;
        }

        score += (int )Mathf.Pow(currentMax, 2);
        currentMax = 0;

        //check left and right
        while (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y) != null && getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y) != null && getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y);
            numInRow++;
        }

        score += (int)Mathf.Pow(currentMax, 2);
        currentMax = 0;

        //check diagonal left
        while (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y - 1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y + 1) != null && getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y + 1);
            numInRow++;
        }

        score += (int)Mathf.Pow(currentMax, 2);
        currentMax = 0;

        //check diagonal right
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1);
        }

        numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1) != null && getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1);
            numInRow++;
        }

        score += (int)Mathf.Pow(currentMax, 2);

        return score;
    }

    int getHighestWithBlanks(Tile t)
    {
        if(t.currentState == Tile.states.none)
        {
            return 0;
        }
         int currentMax = 0;

       
        int numInRow = 1;
        int numNull = 0;


        currentMax = Mathf.Max(numInRow, currentMax);
        if(currentMax ==amountToConnect && numNull == 1)
        {
            return 999999;
        }

        //check left and right
        while (getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y) != null && getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y);
        }

        numInRow = 1;
        numNull = 0;
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y) != null && (getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y).currentState == Tile.states.none))
        {
            if (t.currentState == Tile.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);
        if (currentMax == amountToConnect && numNull == 1)
        {
            return 999999;
        }

        //check diagonal left
        while (getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y-1);
        }

        numInRow = 1;
        numNull = 0;
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y + 1) != null && (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y+1).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y +1).currentState == Tile.states.none))
        {
            if (t.currentState == Tile.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y+1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);
        if (currentMax == amountToConnect && numNull == 1)
        {
            return 999999;
        }

        //check diagonal right
        while (getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y - 1);
        }

        numInRow = 1;
        numNull = 0;
        while (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1) != null && (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y+1).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x-1, (int)t.transform.position.y +1).currentState == Tile.states.none))
        {
            if (t.currentState == Tile.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1);
            numInRow++;
        }

        currentMax = Mathf.Max(numInRow, currentMax);
        if (currentMax == amountToConnect && numNull == 1)
        {
            return 999999;
        }

        return currentMax * 10;
    }

}
