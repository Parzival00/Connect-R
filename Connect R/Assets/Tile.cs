using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public int width, height;
    [HideInInspector] public GridManager gridManager;
    [HideInInspector] public bool occupied = false;

    SpriteRenderer renderer;

    public enum states
    {
        none, player1, player2
    }
    public states currentState;

    private void Start()
    {
        currentState = states.none;
        renderer = this.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        width = (int)gameObject.transform.position.x;
        height = (int)gameObject.transform.position.y;
        if(currentState != states.none)
        {
            occupied = true;
        }
        else
        {
            occupied = false;
        }

    }


}
