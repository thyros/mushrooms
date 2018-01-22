using UnityEngine;

public struct Morph {    
    public Vector2Int position;
 
    public int size;

    public Morph(Vector2Int position, int size) {
        this.position = position;
        this.size = size;
    }
}