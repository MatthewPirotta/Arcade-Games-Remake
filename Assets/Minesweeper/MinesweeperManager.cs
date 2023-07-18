using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class MinesweeperManager : MonoBehaviour
{
    [SerializeField] GameObject plane;
    Camera camera;
    Grid playerGrid;

    int scale = 10;

    private void Awake()
    {
        playerGrid = GetComponent<Grid>();
        camera = Camera.main;
    }

    public void generateGrid()
    {
        for (int row = 0; row < playerGrid.grid.GetLength(0); row++)
        {
            for (int col = 0; col < playerGrid.grid.GetLength(1); col++)
            {
                Vector3Int location = new Vector3Int(row * scale, 0, col * scale);
                GameObject tile = Instantiate(plane, location, Quaternion.identity, gameObject.transform);
                tile.name = $"{row},{col}";
            }
        }
    }

    public void updateLabels()
    {
        string tileData = "";

        foreach (Tile tile in playerGrid.grid)
        {
            Transform foundTile = gameObject.transform.Find($"{tile.coordinates.x},{tile.coordinates.y}");
            TextMeshPro label = foundTile.GetComponentInChildren<TextMeshPro>();

            tileData = "";
            if (tile.isflaged) tileData += "&";
            else if (tile.isbomb && playerGrid.isGameOver) tileData += "B";
            else if (tile.isrevealed && tile.numNeighbouringMines == 0) tileData += ""; // mkaing the 0 empty display
            else if (tile.isrevealed) tileData += tile.numNeighbouringMines;
            else tileData += "-";

            label.text = tileData;
            label.color = updateTextColour(tile);
        }
    }

    public Color updateTextColour(Tile tile)
    {
        if (tile.isflaged) return Color.black;
        switch (tile.numNeighbouringMines)
        {
            case 0: return Color.gray;
            case 1: return Color.blue;
            case 2: return Color.green;
            case 3: return Color.red;
            case 4: return new Color(0, 0, .39f, 1);
            case 5: return Color.yellow;
            case 6: return Color.cyan;
            case 7: return Color.magenta;
            case 8: return Color.white;
        }
        
        if(tile.isrevealed) return Color.gray;
        return Color.black;
    }

    private void Update()
    {
        userInput();
    }

    public void userInput()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector2Int coords;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                coords = translateCharNumToInt(hit);
                playerGrid.pickTile(coords.x, coords.y);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                coords = translateCharNumToInt(hit);
                playerGrid.flagTile(coords.x, coords.y);
            }
        }
        else if (Input.GetMouseButtonDown(2))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                coords = translateCharNumToInt(hit);
                playerGrid.revealTiles(coords.x, coords.y);
            }
        }
    }

    public Vector2Int translateCharNumToInt(RaycastHit hit)
    {
        int x = (int)hit.transform.position.x / scale;
        int y = (int)hit.transform.position.z / scale;

        return new Vector2Int(x, y);
    }
}
