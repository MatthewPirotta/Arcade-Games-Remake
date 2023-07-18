using System.Collections.Generic;
using UnityEngine;

public class Node {
    public Dictionary<Vector2Int, TTTTile> grid = new Dictionary<Vector2Int, TTTTile>();
    public List<Node> children = new List<Node>();
    public bool isMaximizingPlayer;
    public int movesPlayed;
    public GameState gameState = GameState.Running;
    public int eval;
    public Vector2Int coordsPlayed;


    public Node() {

    }
    public Node(Dictionary<Vector2Int, TTTTile> grid, bool isMaximizingPlayer, Vector2Int coordsPlayed) :
        this(grid, new List<Node>(), isMaximizingPlayer, -1, GameState.Running, coordsPlayed) {
    }

    public Node(Dictionary<Vector2Int, TTTTile> grid, bool isMaximizingPlayer, int movesPlayed) :
        this(grid, new List<Node>(), isMaximizingPlayer, movesPlayed, GameState.Running, new Vector2Int(-1, -1)) {
    }

    public Node(Dictionary<Vector2Int, TTTTile> grid, bool isMaximizingPlayer, int movesPlayed, Vector2Int coordsPlayed) :
       this(grid, new List<Node>(), isMaximizingPlayer, movesPlayed, GameState.Running, coordsPlayed) {
    }

    public Node(Dictionary<Vector2Int, TTTTile> grid, List<Node> children, bool isMaximizingPlayer, int movesPlayed, GameState gameState, Vector2Int coordsPlayed) {
        this.grid = new Dictionary<Vector2Int, TTTTile>(grid);
        this.children = children;
        this.isMaximizingPlayer = isMaximizingPlayer;
        this.movesPlayed = movesPlayed;
        this.gameState = gameState;
        this.coordsPlayed = coordsPlayed;
    }
}

public class MinMax {

    const int gridSize = 3;

    public int minMaxAlgorithm(Node node, int depth, int alpha, int beta, bool isMaximizingPlayer) {
        int eval;

        //static evulation
        if (depth == 0 || node.gameState != GameState.Running) {
            switch (node.gameState) {
                case GameState.Win: node.eval = 100; return 100;
                case GameState.Loss: node.eval = -100; return -100;
                case GameState.Draw: node.eval = 0; return 0;
            }
        }

        generateNextLayerOfMoves(node);

        //x
        if (isMaximizingPlayer) {
            int maxEval = int.MinValue;
            foreach (Node child in node.children) {
                eval = minMaxAlgorithm(child, depth - 1, alpha,beta, false);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            //Debug.Log($"max value {maxEval}");
            node.eval = maxEval;
            return maxEval;
        }

        //O
        else if (!isMaximizingPlayer) {
            int minEval = int.MaxValue;
            foreach (Node child in node.children) {
                eval = minMaxAlgorithm(child, depth - 1, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Max(beta, eval);
                if (beta <= alpha) break;
            }
            //Debug.Log($"min value {minEval}");
            node.eval = minEval;
            return minEval;
        }

        //shouldnt reach here
        Debug.LogWarning("PIss");
        return 0;
    }

    public Vector2Int selectMove(Node node) {


        //todo remove !
        node.isMaximizingPlayer = !node.isMaximizingPlayer;

        int bestEval = node.isMaximizingPlayer ? int.MinValue : int.MaxValue;
        Node bestChild = null;
        
        foreach (Node child in node.children) {
            if (node.isMaximizingPlayer && child.eval > bestEval) {
                bestEval = child.eval;
                bestChild = child;
            }
            else if (!node.isMaximizingPlayer && child.eval < bestEval) {
                bestEval = child.eval;
                bestChild = child;
            }
        }

        if (bestChild != null) {
            //Debug.Log($"Child played {bestChild.coordsPlayed}");
            return bestChild.coordsPlayed;
        }

        Debug.LogWarning("//shouldn't reach here");
        Debug.Log($"Parent eval {node.eval}");
        foreach (Node child in node.children) {
            Debug.Log($"Child eval {child.eval}");
        }

        return new Vector2Int(-1, -1);
    }


    /*
    public Vector2Int selectMove(Node node) {
        foreach (Node child in node.children) {
            if (node.eval == child.eval) {
                Debug.Log($"Child played {child.coordsPlayed}");
                return child.coordsPlayed;
            }
        }
        Debug.LogWarning("//shouldnt reach here");
        Debug.Log($" parent eval {node.eval}");
        foreach (Node child in node.children) {
            Debug.Log($" child eval {child.eval}");
        }
        return new Vector2Int(-1, -1);
    }
    */

    public void generateNextLayerOfMoves(Node parentNode) {
        foreach (TTTTile tile in parentNode.grid.Values) {
            if (tile.getState() == State.Undefined) { //if there is a free tile to make a move in
                generateMove(parentNode, tile);
            }
        }
    }

    public void generateMove(Node parentNode, TTTTile tile) {
        Dictionary<Vector2Int, TTTTile> newGrid = generateNewGrid(parentNode);

        if (parentNode.isMaximizingPlayer) newGrid[tile.coordinates].setState(State.O);
        else newGrid[tile.coordinates].setState(State.X);

        Node childNode = new Node(newGrid, !parentNode.isMaximizingPlayer, parentNode.movesPlayed + 1, tile.coordinates);
        checkGameState(childNode);

        parentNode.children.Add(childNode);
        if (childNode.gameState != GameState.Running) return;
    }

    public Dictionary<Vector2Int, TTTTile> generateNewGrid(Node parentNode) {
        Dictionary<Vector2Int, TTTTile> newGrid = new Dictionary<Vector2Int, TTTTile>(parentNode.grid);
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                Vector2Int coordinate = new Vector2Int(x, y);
                newGrid[coordinate] = new TTTTile(coordinate, newGrid[coordinate].getState());
            }
        }
        return newGrid;
    }

    public void printChildrenNodes(Node node) {
        foreach (Node childNode in node.children) {
            printGrid(childNode);
            printChildrenNodes(childNode);
        }
    }

    public void checkGameState(Node node) {
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
            if (node.grid[new Vector2Int(x, y)].getState() == State.X) xScore++;
            else if (node.grid[new Vector2Int(x, y)].getState() == State.O) oScore++;
        }

        void checkVictory() {
            if (xScore >= gridSize) {node.gameState = GameState.Win; }
            else if (oScore >= gridSize) {node.gameState = GameState.Loss; }
            xScore = 0; oScore = 0;
        }

        //draw check
        if (node.gameState != GameState.Running) return; //will set game as draw if at end of game but player just won on last move
        int movesPlayed = 0;
        foreach (TTTTile tile in node.grid.Values) {
            if (tile.getState() != State.Undefined) movesPlayed++;
        }
        if (movesPlayed == Mathf.Pow(gridSize, 2)) {
            //Debug.Log("Draw");
            node.gameState = GameState.Draw;
            return;
        }

    }

    public void printGrid(Node node) {
        string gridprint = "";
        for (int y = gridSize - 1; y >= 0; y--) {
            for (int x = 0; x < gridSize; x++) {
                gridprint += "\t";
                if (node.grid[new Vector2Int(x, y)].getState() == State.Undefined) gridprint += " |";
                else if (node.grid[new Vector2Int(x, y)].getState() == State.X) gridprint += " X ";
                else if (node.grid[new Vector2Int(x, y)].getState() == State.O) gridprint += " O ";
            }
            gridprint += "\n";
        }
        gridprint += $"moves played {node.movesPlayed}";
        gridprint += "\n";
        gridprint += $"game state {node.gameState}";
        //Debug.Log(gridprint);
    }


}
