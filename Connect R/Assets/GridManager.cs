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
    int maximumDepth = 2;

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

    int childrenCount = 0;

    int minIndex, maxIndex;

    private void Start()
    {
        //generateGrid();
        player1Turn = true;
        winText.gameObject.SetActive(false);
        
        
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

        tileObjects = FindObjectsOfType<Tile>();


        List<TileClass> tempTiles = new List<TileClass>();


        buildTree(true);

        

        TreeNode<List<TileClass>> node = moveTree;

       

        if (player1AI.isOn)
        {
            doAIMove(true);
        }

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
    /// returns a TileClass that is a copy of the input, but not a reference
    /// </summary>
    TileClass duplicateTile(TileClass tileIn)
    {
        TileClass newTile = new TileClass();
        newTile.x = tileIn.x;
        newTile.y = tileIn.y;
        newTile.currentState = tileIn.currentState;

        return newTile;
    }

    List<TileClass> duplicateBoard(List<TileClass> inputBoard)
    {
        List<TileClass> newList = new List<TileClass>();

        foreach (TileClass t in inputBoard)
        {
            newList.Add(duplicateTile(t));
        }

        return newList;
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

            if (player1Turn)
            {
                getTileObject(t).GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                getTileObject(t).GetComponent<SpriteRenderer>().color = Color.yellow;
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
                return;
            }

            

            player1Turn = !player1Turn;

            if (player1Turn && player1AI.isOn)
            {
                doAIMove(true);
            }
            if (!player1Turn && player2AI.isOn)
            {
                doAIMove(false);
            }
        }

    }

    public void placeTokenOnTurn(int pos, bool isPlayer1)
    {
        TileClass t = getLowest(pos, height - 1);

        if (pos < height)
        {
            t = getLowest(pos, height - 1);
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

        }

    }

    void placeTokenOnTurnColor(int pos, bool isPlayer1)
    {
        TileClass t = getLowest(pos, height - 1);

        if (pos < height)
        {
            t = getLowest(pos, height - 1);
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

        }

        if (player1Turn)
        {
            getTileObject(t).GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            getTileObject(t).GetComponent<SpriteRenderer>().color = Color.yellow;
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
            return;
        }

        

        player1Turn = !player1Turn;

        if(player1Turn && player1AI.isOn)
        {
            doAIMove(true);
        }
        if(!player1Turn && player2AI.isOn)
        {
            doAIMove(false);
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
        while (getTileAtPosition((int)t.x, (int)t.y + 1) != null && getTileAtPosition((int)t.x, (int)t.y + 1).currentState == t.currentState)
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
            if (t.currentState == TileClass.states.player1)
            {
                //how close to the center is it
                score +=(width+1- (int)Mathf.Abs(t.x + 1 - (float)width / 2) )* 10;
                print((int)Mathf.Abs(t.x + 1 - (float)width / 2));
                //how many total are in a row for each direction with better score for the higher the amount in a row is 
                //score += getAmountInEachDirection(t) * 20;

                score += getHighestMatch(t) * 20;
                print(getHighestMatch(t));

                //how many can be in a row in the future
                score += getHighestWithBlanks(t)*3;


            }
            else if (t.currentState == TileClass.states.player2)
            {
                //how close to the center is it
                score -= (width+1 - (int)Mathf.Abs(t.x + 1 - (float)width / 2)) * 10;

                //how many total are in a row for each direction with better score for the higher the amount in a row is 
                //score -= getAmountInEachDirection(t) * 20;

                score -= getHighestMatch(t) * 20;

                //how many can be in a row in the future
                score -= getHighestWithBlanks(t) *3;

            }



            //if (isPlayer1)
            //{
            //    if (t.currentState == TileClass.states.player1)
            //    {
            //        //how close to the center is it
            //        score -= (int)Mathf.Abs(t.x+1 - (float)width/2) * 30;

            //        //how many total are in a row for each direction with better score for the higher the amount in a row is 
            //        score += getAmountInEachDirection(t) * 2;


            //        //how many can be in a row in the future
            //        score += getHighestWithBlanks(t);


            //    }
            //    else if (t.currentState == TileClass.states.player2)
            //    {
            //        //how close to the center is it
            //        score += (int)Mathf.Abs(t.x - (float)width) * 30;

            //        //how many total are in a row for each direction with better score for the higher the amount in a row is 
            //        score -= getAmountInEachDirection(t) * 2;

            //    }
            //}
            //else
            //{
            //    if (t.currentState == TileClass.states.player2)
            //    {
            //        //how close to the center is it
            //        score -= (int)Mathf.Abs(t.x + 1 - (float)width/2) * 30;

            //        //how many total are in a row for each direction with better score for the higher the amount in a row is 
            //        score += getAmountInEachDirection(t) * 2;


            //        //how many can be in a row in the future
            //        score += getHighestWithBlanks(t);
            //    }
            //    else if (t.currentState == TileClass.states.player2)
            //    {
            //        //how close to the center is it
            //        score -= (int)Mathf.Abs(t.x - (float)width) * 30;

            //        //how many total are in a row for each direction with better score for the higher the amount in a row is 
            //        score += getAmountInEachDirection(t) * 2;
            //    }
            //}
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
        while (getTileAtPosition((int)t.x, (int)t.y + 1) != null && getTileAtPosition((int)t.x, (int)t.y + 1).currentState == t.currentState)
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
            return 1000;
        }

        return currentMax * 10;
    }

    /// <summary>
    /// populates the tree up to 5 deep
    /// </summary>
    void populateChildren(int currentDepth, bool maxingPlayer)
    {
        //I truly am sorry for the way I ended up doing this, but I worked for well over 5 hours just on this iteration of the tree. I already had restarted before this
        print("in depth " + currentDepth);
        switch (currentDepth)
        {
            case 0:
                childrenCount++;
                for (int i = 0; i < width; i++)
                {
                    List<TileClass> originalBoard = duplicateBoard(allTiles);

                    allTiles = duplicateBoard(moveTree.Value);

                    placeTokenOnTurn(i, maxingPlayer);
                    moveTree.AddChild(duplicateBoard(allTiles));


                    allTiles = duplicateBoard(originalBoard);

                }
                break;
            case 1:
                foreach(TreeNode<List<TileClass>> node in moveTree.Children)
                {
                    childrenCount++;
                    for (int i = 0; i < width; i++)
                    {
                        List<TileClass> originalBoard = duplicateBoard(allTiles);

                        allTiles = duplicateBoard(node.Value);

                        placeTokenOnTurn(i, maxingPlayer);
                        node.AddChild(duplicateBoard(allTiles));

                        allTiles = duplicateBoard(originalBoard);

                    }
                }
                break;
            case 2:
                foreach (TreeNode<List<TileClass>> node in moveTree.Children)
                {
                    foreach (TreeNode<List<TileClass>> node2 in node.Children)
                    {
                        childrenCount++;
                        for (int i = 0; i < width; i++)
                        {
                            List<TileClass> originalBoard = duplicateBoard(allTiles);

                            allTiles = duplicateBoard(node2.Value);

                            placeTokenOnTurn(i, maxingPlayer);
                            node2.AddChild(duplicateBoard(allTiles));

                            allTiles = duplicateBoard(originalBoard);

                        }
                    }
                }
                break;
            case 3:
                foreach (TreeNode<List<TileClass>> node in moveTree.Children)
                {
                    foreach (TreeNode<List<TileClass>> node2 in node.Children)
                    {
                        foreach (TreeNode<List<TileClass>> node3 in node2.Children)
                        {
                            childrenCount++;
                            for (int i = 0; i < width; i++)
                            {
                                List<TileClass> originalBoard = duplicateBoard(allTiles);

                                allTiles = duplicateBoard(node3.Value);

                                placeTokenOnTurn(i, maxingPlayer);
                                node3.AddChild(duplicateBoard(allTiles));

                                allTiles = duplicateBoard(originalBoard);

                            }
                        }
                    }
                }
                break;
            case 4:
                foreach (TreeNode<List<TileClass>> node in moveTree.Children)
                {
                    foreach (TreeNode<List<TileClass>> node2 in node.Children)
                    {
                        foreach (TreeNode<List<TileClass>> node3 in node2.Children)
                        {
                            foreach (TreeNode<List<TileClass>> node4 in node3.Children)
                            {
                                childrenCount++;
                                for (int i = 0; i < width; i++)
                                {
                                    List<TileClass> originalBoard = duplicateBoard(allTiles);

                                    allTiles = duplicateBoard(node4.Value);

                                    placeTokenOnTurn(i, maxingPlayer);
                                    node4.AddChild(duplicateBoard(allTiles));

                                    allTiles = duplicateBoard(originalBoard);

                                }
                            }
                        }
                    }
                }
                break;
            case 5:
                foreach (TreeNode<List<TileClass>> node in moveTree.Children)
                {
                    foreach (TreeNode<List<TileClass>> node2 in node.Children)
                    {
                        foreach (TreeNode<List<TileClass>> node3 in node2.Children)
                        {
                            foreach (TreeNode<List<TileClass>> node4 in node3.Children)
                            {
                                foreach (TreeNode<List<TileClass>> node5 in node4.Children)
                                {
                                    childrenCount++;
                                    for (int i = 0; i < width; i++)
                                    {
                                        List<TileClass> originalBoard = duplicateBoard(allTiles);

                                        allTiles = duplicateBoard(node5.Value);

                                        placeTokenOnTurn(i, maxingPlayer);
                                        node5.AddChild(duplicateBoard(allTiles));

                                        allTiles = duplicateBoard(originalBoard);

                                    }
                                }
                            }
                        }
                    }
                }
                break;

        }

    }

    /// <summary>
    /// builds the tree at the current game state
    /// </summary>
    void buildTree(bool player1Starting)
    {
        moveTree = new TreeNode<List<TileClass>>(duplicateBoard(allTiles));

        populateChildren(0, player1Starting);
        populateChildren(1, !player1Starting);
        populateChildren(2, player1Starting);
        populateChildren(3, !player1Starting);
        populateChildren(4, player1Starting);
    }


    int miniMax(TreeNode<List<TileClass>> node, int depth, bool maxingPlayer)
    {
        int score = heuristic(node.Value, maxingPlayer);
        int bestScore = 0;

        //check terminal
        if(node.Children.Count == 0 || depth >= maximumDepth)
        {
            return score;
        }

       

  
        //check if that move is better than any of the other moves and overwrite it
        if (maxingPlayer)
        {
            bestScore = int.MinValue;
            //evaluate all possible moves at this depth
            foreach (TreeNode<List<TileClass>> board in node.Children)
            {
                //call minimaax on that new board
                score = miniMax(board, depth + 1, !maxingPlayer);

                bestScore = Mathf.Max(score, bestScore);

            }
            
        }
        else
        {
            bestScore = int.MaxValue;
            //evaluate all possible moves at this depth
            foreach (TreeNode<List<TileClass>> board in node.Children)
            {
                //call minimaax on that new board
                score = miniMax(board, depth + 1, !maxingPlayer);

                bestScore = Mathf.Min(score, bestScore);

            }
            

        }




        //print("current best: " + bestScore);

        //return the best score
        return bestScore;
    }

    void doAIMove(bool maximizing) 
    {
        buildTree(maximizing);

        List <TileClass> bestBoard = new List<TileClass>();

        if (maximizing)
        {
            int bestScore = int.MinValue;

            foreach (TreeNode<List<TileClass>> node in moveTree.Children)
            {
                int score = miniMax(node, 0, true);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestBoard = node.Value;
                }
            }
        }
        else
        {
            int bestScore = int.MaxValue;

            foreach (TreeNode<List<TileClass>> node in moveTree.Children)
            {
                int score = miniMax(node, 0, false);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestBoard = node.Value;
                }
            }
        }

        int bestmove = -1;
        for(int i = 0; i < allTiles.Count; i ++)
        {
            if (bestBoard[i].currentState != allTiles[i].currentState)
            {
                bestmove = bestBoard[i].x;
                
            }
        }
        placeTokenOnTurnColor(bestmove, maximizing);
        
    }
}
