using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Grid : MonoBehaviour
{
    int height;
    int width;
    int totMines;
    int remainingMines;
    bool firstpick = true;
    public bool isGameOver = false;
    bool isWon = false;
    public Tile[,] grid;

    MinesweeperManager minesweeperManager;
    [SerializeField] TextMeshProUGUI minesText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI wonText;

    Queue<Tile> searchQueue = new Queue<Tile>();
    Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down,
                                new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1) };


    private void Awake()
    {
        minesweeperManager = GetComponent<MinesweeperManager>();
    }

    public void Start()
    {
        height = 9;
        width = 9;
        grid = new Tile[height, width];

        totMines = 15;
        remainingMines = totMines;

        initialiseGrid();

        displayPlayerGrid();
        minesweeperManager.generateGrid();
    }

    public void initialiseGrid()
    {
        Vector2Int coords;
        for (int rows = 0; rows < grid.GetLength(0); rows++)
        {
            for (int cols = 0; cols < grid.GetLength(1); cols++)
            {
                coords = new Vector2Int(rows, cols);
                grid[rows, cols] = new Tile(coords);
            }
        }
    }

    public void generateMines(int firstX, int firstY)
    {
        int randRow;
        int randCol;
        bool notFirstTile;
        int mines = totMines;
        while (mines > 0)
        {
            randRow = UnityEngine.Random.Range(0, grid.GetLength(0)); //min Inlcusive, maxExclusive
            randCol = UnityEngine.Random.Range(0, grid.GetLength(1));
            notFirstTile = !(randRow == firstX && randCol == firstY);

            if (!grid[randRow, randCol].isbomb && notFirstTile)
            {
                grid[randRow, randCol].isbomb = true;
                mines--;
            }

        }
    }

    public void displayBombsGrid()
    {
        String gridData = "";
        for (int rows = 0; rows < grid.GetLength(0); rows++)
        {
            for (int cols = 0; cols < grid.GetLength(1); cols++)
            {
                gridData += "\t";
                gridData += (grid[rows, cols].isbomb) ? 1 : 0;
            }
            gridData += "\n";
        }
        Debug.Log(gridData);
    }

    public void displayPlayerGrid()
    {
        Tile displayTile;
        String gridData = "";
        for (int rows = 0; rows < grid.GetLength(0); rows++)
        {
            for (int cols = 0; cols < grid.GetLength(1); cols++)
            {
                displayTile = grid[rows, cols];
                gridData += "\t";
                if (displayTile.isflaged) gridData += "&";
                else if (grid[rows, cols].isrevealed) gridData += grid[rows, cols].numNeighbouringMines;
                else gridData += "-";
            }
            gridData += "\n";
        }
       // Debug.Log(gridData);
       // Debug.Log($"Remaining mines {remainingMines}");
    }

    public void pickTile(int x, int y)
    {
        if (firstpick)
        {
            generateMines(x, y);
            firstpick = false;
        }

        if (grid[x, y].isbomb) { GameOver(); return; } //TODO DEATH 
        breathFirstSearch(grid[x, y]);

        updateGameState();
    }

    //TODO should this be in Tile?
    public void setTileMineNum(Tile currentSearchNode)
    {
        int sumMines = 0;

        List<Tile> neighbours = findNeighbours(currentSearchNode);
        foreach (Tile neighbour in neighbours)
            if (grid[neighbour.coordinates.x, neighbour.coordinates.y].isbomb) sumMines++;

        grid[currentSearchNode.coordinates.x, currentSearchNode.coordinates.y].numNeighbouringMines = sumMines;
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

            if (currentSearchNode.numNeighbouringMines == 0)
            {
                exploreNeighbors(currentSearchNode);
            }
        }
    }

    public void breathFirstSearchReavealTile(Tile root)
    {
        Tile currentSearchNode;

        searchQueue.Clear();

        searchQueue.Enqueue(root);
        root.isrevealed = true;
        while (searchQueue.Count > 0)
        {
            currentSearchNode = searchQueue.Dequeue();
            setTileMineNum(currentSearchNode);
            if(currentSearchNode == root)
            {
                exploreNeighbors(currentSearchNode);
            }
            if (currentSearchNode.numNeighbouringMines == 0)
            {
                exploreNeighbors(currentSearchNode);
            }
        }
    }

    public List<Tile> findNeighbours(Tile currentSearchNode)
    {
        List<Tile> neighbours = new List<Tile>();

        //exploring all adjacent tiles
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighboorCoords = currentSearchNode.coordinates + direction;

            if (neighboorCoords.x >= 0 && neighboorCoords.x < grid.GetLength(0) &&
                neighboorCoords.y >= 0 && neighboorCoords.y < grid.GetLength(1))
            {
                neighbours.Add(grid[neighboorCoords.x, neighboorCoords.y]);
            }
        }

        return neighbours;
    }

    public void exploreNeighbors(Tile currentSearchNode)
    {
        List<Tile> neighbours = findNeighbours(currentSearchNode);

        //checking if said tiles are bombs and if not are added to search queue
        foreach (Tile neighbour in neighbours)
        {
            if (neighbour.isrevealed) continue;
            if (neighbour.isflaged) continue;
            if (neighbour.isbomb) continue;

            neighbour.isrevealed = true;
            searchQueue.Enqueue(neighbour);
        }

    }

    public void flagTile(int x, int y)
    {
        Tile tile = grid[x, y];
        if (tile.isrevealed) return;
        if (tile.isflaged)
            remainingMines++;
        else
            remainingMines--;

        grid[x, y].flagTile();

        updateGameState();
    }

    public void revealTiles(int x, int y)
    {
        Tile selectedTile = grid[x, y];
        if (!selectedTile.isrevealed) return;
        int surroundingFlags = 0;
        List<Tile> neighbours = findNeighbours(selectedTile);
        foreach (Tile neighbour in neighbours) if (neighbour.isflaged) surroundingFlags++;

        if (selectedTile.numNeighbouringMines == surroundingFlags)
        {
            foreach (Tile neighbour in neighbours)
            {
                if (neighbour.isflaged && !neighbour.isbomb)
                {
                    GameOver(); 
                    return;
                }
            }
            breathFirstSearchReavealTile(selectedTile);
        }

        updateGameState();
    }

    public void GameOver()
    {
        Debug.Log("DEATh");
        isGameOver = true;
        revealmines();
    }

    public void revealmines()
    {
        foreach (Tile tile in grid)
        {
            if (tile.isbomb) tile.isrevealed = true;
        }
        updateGameState();
    }

    public void checkWin()
    {
        int found = 0;
        int totalTiles = width * height;
        foreach(Tile tile in grid)
        {
            if (tile.isrevealed || (tile.isbomb && tile.isflaged)) found ++;
        }
        
        if(remainingMines == 0 && totalTiles == found)
        {
            isWon = true;
        }
    }

    public void updateGameState()
    {
        displayPlayerGrid();
        minesweeperManager.updateLabels();
        checkWin();
        minesText.text = $"Mines {remainingMines}";
        statusText.text = $"is dead: {isGameOver}";
        wonText.text = $"Won : {isWon}"; 
    }
}
