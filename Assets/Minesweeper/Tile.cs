using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile
{
    public bool isbomb = false;
    public bool isrevealed = false;
    public bool isflaged = false;
    public int numNeighbouringMines { get; set; }

    public Vector2Int coordinates;


    public Tile(Vector2Int coordinate)
    {
        this.coordinates = coordinate;
    }

    public void flagTile()
    {
        if (isrevealed) return; // cant flag revealed tiles
        isflaged = !isflaged;
    }
}
