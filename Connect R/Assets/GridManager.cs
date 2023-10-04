using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width, height;
    [SerializeField] Tile tile;
    [SerializeField] Transform camera;
    [SerializeField] InputField input;
    bool player1Turn;

    Color yellow = new Color(255, 200, 0);
    Color red = new Color(200, 20, 0);
    Color white = new Color(255, 255, 255);

    private void Start()
    {
        generateGrid();
        player1Turn = true;
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
        Tile t = getLowest(int.Parse(input.text), height-1);

        if (player1Turn)
        {
            print("this one");
            t.currentState = Tile.states.player1;
            t.GetComponent<SpriteRenderer>().color = Color.red;
            player1Turn = !player1Turn;
        }
        else
        {
            t.currentState = Tile.states.player2;
            t.GetComponent<SpriteRenderer>().color = Color.yellow;
            player1Turn = !player1Turn;
        }

    }
}
