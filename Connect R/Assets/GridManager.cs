using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width, height, amountToConnect;
    [SerializeField] Tile tile;
    [SerializeField] Transform camera;
    [SerializeField] InputField input;
    [SerializeField] Text winText;
    bool player1Turn;

    private void Start()
    {
        generateGrid();
        player1Turn = true;
        winText.gameObject.SetActive(false);
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
        if (getTileAtPosition(x, y).currentState != Tile.states.none)
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
        print(getTileAtPosition(int.Parse(input.text), height-1).name);

        Tile t = new Tile();

        if (int.Parse(input.text) < height)
        {
            t = getLowest(int.Parse(input.text), height - 1);
        }

        if (t != null)
        {
            if (player1Turn)
            {
                print("this one");
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

    int getHighestMatch(Tile t)
    {
        int currentMax = 0;

        //check up and down
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1);
        }

        int numInRow = 1;
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1).currentState == t.currentState)
        {
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
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1).currentState == t.currentState)
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
         int currentMax = 0;

        //check up and down
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1) != null && getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y-1).currentState == t.currentState)
        {
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1);
        }

        int numInRow = 1;
        int numNull = 0;
        while (getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1).currentState == Tile.states.none)
        {
            if(t.currentState == Tile.states.none)
            {
                numNull++;
            }
            t = getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y + 1);
            numInRow++;
        }

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
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y) != null && (getTileAtPosition((int)t.transform.position.x+1, (int)t.transform.position.y).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1).currentState == Tile.states.none))
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
        while (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y + 1) != null && (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1).currentState == Tile.states.none))
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
        while (getTileAtPosition((int)t.transform.position.x - 1, (int)t.transform.position.y + 1) != null && (getTileAtPosition((int)t.transform.position.x + 1, (int)t.transform.position.y).currentState == t.currentState || getTileAtPosition((int)t.transform.position.x, (int)t.transform.position.y - 1).currentState == Tile.states.none))
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
