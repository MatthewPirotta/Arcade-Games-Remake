using System.Collections.Generic;
using UnityEngine;

public class TTTGrid : MonoBehaviour {
    const int gridSize = 3;
    public Dictionary<Vector2Int, TTTTile> grid = new Dictionary<Vector2Int, TTTTile>();
    public int movesplayed = 0;
    GameState gameState;

    TTTManager manager;
   

    private void Awake() {
        manager = GetComponent<TTTManager>();
        populateGrid();
        gameState = GameState.Running;
    }

    void populateGrid() {
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                grid.Add(new Vector2Int(x, y), new TTTTile(x, y));
            }
        }
    }

    public void checkGameState() {
        int xScore = 0;
        int oScore = 0;
        //Verticle check
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                // Debug.Log($"{x}, {y} xScore{xScore}, Oscore{oScore}");
                updateLocalScore(x, y);
            }
            checkVictory();
        }

        //Horizontal check
        for (int y = 0; y < gridSize; y++) {
            for (int x = 0; x < gridSize; x++) {
                // Debug.Log($"{x}, {y} xScore{xScore}, Oscore{oScore}");
                updateLocalScore(x, y);
            }
            checkVictory();
        }

        //diagonal check
        Vector2Int centreNode = new Vector2Int(1, 1);
        var incline = new List<Vector2Int>() { Vector2Int.zero, new Vector2Int(1, 1), new Vector2Int(-1, -1) };// y = x
        var decline = new List<Vector2Int>() { Vector2Int.zero, new Vector2Int(-1, 1), new Vector2Int(1, -1) };// y = -x

        foreach (Vector2Int displacement in incline) {
            Vector2Int newPos = centreNode + displacement;
            updateLocalScore(newPos.x, newPos.y);
        }
        checkVictory();

        foreach (Vector2Int displacement in decline) {
            Vector2Int newPos = centreNode + displacement;
            updateLocalScore(newPos.x, newPos.y);

        }
        checkVictory();


        void updateLocalScore(int x, int y) {
            if (grid[new Vector2Int(x, y)].getState() == State.X) xScore++;
            else if (grid[new Vector2Int(x, y)].getState() == State.O) oScore++;
        }

        void checkVictory() {
            if (xScore >= 3) { Debug.Log("X won"); gameState = GameState.Win; manager.gameOver(); }
            if (oScore >= 3) { Debug.Log("O won"); gameState = GameState.Loss; manager.gameOver(); }
            xScore = 0; oScore = 0;
        }

        //draw check
        if (movesplayed == Mathf.Pow(gridSize, 2)) {
            Debug.Log("Draw");
            gameState = GameState.Draw;
            manager.gameOver();
            return;
        }
    }

    public void printGrid() {
        string gridprint = ""; 
        for (int y = gridSize - 1; y >= 0; y--) {
            for(int x = 0; x < gridSize; x++) {
                gridprint += "\t";
                if (grid[new Vector2Int(x, y)].getState() == State.Undefined) gridprint += " |";
                else if (grid[new Vector2Int(x, y)].getState() == State.X) gridprint += " X ";
                else if (grid[new Vector2Int(x, y)].getState() == State.O) gridprint += " O ";
            }
            gridprint += "\n";
        }
        Debug.Log(gridprint);
    }
}
