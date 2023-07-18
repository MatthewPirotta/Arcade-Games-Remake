using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BFS : MonoBehaviour
{
    public Tile[,] grid;
    Queue<Tile> searchQueue = new Queue<Tile>();
    Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };


    void Start()
    {
        grid = new Tile[5, 5];
        

        initialiseGrid();
    }

    public void initialiseGrid()
    {
        for (int rows = 0; rows < grid.GetLength(0); rows++)
        {
            for (int cols = 0; cols < grid.GetLength(1); cols++)
            {
                grid[rows, cols] = new Tile(new Vector2Int(rows, cols));
            }
        }
    }

    public void breathFirstSearch(Tile root)
    {
        Tile currentSearchNode;

        searchQueue.Clear();

        searchQueue.Enqueue(root);
        root.isrevealed = true;
        while (searchQueue.Count > 0)
        {
            currentSearchNode = searchQueue.Dequeue();
            setTileMineNum(currentSearchNode);
            exploreNeighbors(currentSearchNode);
        }
    }

    public void exploreNeighbors(Tile currentSearchNode)
    {
        List<Tile> neighbours = new List<Tile>();

        foreach(Vector2Int direction in directions)
        {
            Vector2Int neighboorCoords = currentSearchNode.coordinates + direction;

            if (neighboorCoords.x >= 0 && neighboorCoords.x < grid.GetLength(0) &&
                neighboorCoords.y>= 0  && neighboorCoords.y < grid.GetLength(1))
            {
                neighbours.Add(grid[neighboorCoords.x,neighboorCoords.y]);
            }
        }

        foreach (Tile neighbour in neighbours)
        {
            if (!neighbour.isrevealed)
            {
                neighbour.isrevealed = true;
                searchQueue.Enqueue(neighbour);
            }
        }

    }

    public void exploreNeighbors2(Tile currentSearchNode)
    {
        Debug.Log($"exploring {currentSearchNode.coordinates}");
        ///displayPlayerGrid();
        int x = currentSearchNode.coordinates.x;
        int y = currentSearchNode.coordinates.y;

        if (grid[x, y].numNeighbouringMines != 0) return; // leave if the node has surrounding bombs
        for (int disX = x - 1; disX <= x + 1; disX++)
        {
            for (int disY = x - 1; disY <= y + 1; disY++)
            {
                Debug.LogWarning($"Tile {disX}, {disY} is a possible neighbor");
                if ((disX == x) && (disY == y)) { Debug.Log($"Tile {disX}, {disY} are same as disp"); continue; } // ignoring the search origin
                if (disX < 0 || disX >= grid.GetLength(0) ||
                    disY < 0 || disY >= grid.GetLength(1))
                {
                    Debug.Log($"Tile {disX}, {disY} is being skipped");
                    continue; //checking if new co-ords are within the grid
                }

                if (grid[disX, disY].isbomb) { Debug.Log($"Tile {disX}, {disY} is a bomb"); continue; } // skip if tile is a mine //TODO NOT SURE LOL
                if (grid[disX, disY].isrevealed) { Debug.LogError($"Tile {disX}, {disY} is revelaed"); }
                if (!grid[disX, disY].isrevealed)
                {
                    Debug.Log($"Tile {disX}, {disY} is being added to queue");
                    grid[disX, disY].isrevealed = true;
                    searchQueue.Enqueue(grid[disX, disY]);
                }
            }
        }
    }

    public void setTileMineNum(Tile tile)
    {
        int x = tile.coordinates.x;
        int y = tile.coordinates.y;
        //  grid[x, y].isrevealed = true; TODO MAYBE

        int sumMines = 0;
        for (int disX = x - 1; disX <= x + 1; disX++)
        {
            for (int disY = x - 1; disY <= y + 1; disY++)
            {
                if (disX == x && disY == y) continue; // ignoring the search origin
                if (disX < 0 || disX >= grid.GetLength(0) ||
                    disY < 0 || disY >= grid.GetLength(1))
                {
                    //Debug.Log($"Tile {disX}, {disY} is being skipped");
                    continue; //checking if new co-ords are within the grid
                }

                if (grid[disX, disY].isbomb) sumMines++;

                // Debug.Log($"{disX} , {disY}");
            }
        }
        grid[x, y].numNeighbouringMines = sumMines;
    }

}
