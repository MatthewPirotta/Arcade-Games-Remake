using UnityEngine;

public class TTTTile {
    public Vector2Int coordinates;
    State state;

    public TTTTile(int x, int y) {
        this.coordinates.Set(x, y);
    }

    public TTTTile(Vector2Int coordinates, State state) {
        this.coordinates = coordinates;
        this.state = state;
    }

    public bool setState(State state) {
        if (this.state != State.Undefined) {
            Debug.Log("State already set");
            return false;
        }
        this.state = state;
        return true;
    }

    public State getState() {
        return state;
    }

}
