using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TTTManager : MonoBehaviour {

    const int scale = 10;
    bool isGameOver = false;
    bool turn = true;

    [SerializeField] Camera camera;
    [SerializeField] GameObject plane;
    TTTGrid tttGrid;

    MinMax minMax;
    

    void Awake() {
        camera = Camera.main;
        tttGrid = GetComponent<TTTGrid>();
        minMax = new MinMax();
    }

    void Start() {
        generateGridInWorldSpace();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        if (isGameOver) return;
        userInput();
    }


    public void generateGridInWorldSpace() {
        foreach (Vector2Int coordinates in tttGrid.grid.Keys) {
            Vector3Int location = new Vector3Int(coordinates.x * scale, 0, coordinates.y * scale);
            GameObject objectTile = Instantiate(plane, location, Quaternion.identity, gameObject.transform);
            objectTile.name = $"{coordinates.x},{coordinates.y}";
        }
    }

    public void updateGameState() {
        updateLabels();
        tttGrid.checkGameState();
    }

    private void minMaxPlay(Vector2Int playerMove) {
        //todo set value as true or false for new node?
        Node startingNode = new Node(new Dictionary<Vector2Int, TTTTile>(tttGrid.grid), true, 1, playerMove);
        minMax.generateNextLayerOfMoves(startingNode);
       // minMax.printChildrenNodes(startingNode);
        minMax.minMaxAlgorithm(startingNode, 9, int.MinValue, int.MaxValue, false);
        Vector2Int moveToPlay = minMax.selectMove(startingNode);
       // Debug.Log($"{moveToPlay}");
        playMove(moveToPlay);
        //Debug.Log("----------");
    }

    public void userInput() {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                Vector2Int coords = translateWorldTileCoordsToInt(hit);
                playMove(coords);
                minMaxPlay(coords);
            }
        }
    }

    private void playMove(Vector2Int coords) {
        //Todo check if good
        if (tttGrid.grid[coords].setState(turn ? State.X : State.O)) {
            turn = !turn;
            tttGrid.movesplayed++;
            updateGameState();
        }
    }

    public void updateLabels() {
        string tileData = "";

        foreach (TTTTile tile in tttGrid.grid.Values) {
            Transform foundTile = gameObject.transform.Find($"{tile.coordinates.x},{tile.coordinates.y}");
            TextMeshPro label = foundTile.GetComponentInChildren<TextMeshPro>();

            tileData = "";
            if (tile.getState() == State.Undefined) tileData += ""; // making undefined empty
            else tileData += tile.getState();

            label.text = tileData;
            //label.color = updateTextColour(tile);
        }
    }


    public void gameOver() {
        isGameOver = true;
    }

    

    public Vector2Int translateWorldTileCoordsToInt(RaycastHit hit) {
        int x = (int)hit.transform.position.x / scale;
        int y = (int)hit.transform.position.z / scale;

        return new Vector2Int(x, y);
    }

    void setTileValue(Vector2Int coordinates, State state) {
        tttGrid.grid[coordinates].setState(state);
    }
}
