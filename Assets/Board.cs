using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Vector2Int size;
    public GameObject tilePrefab;
    public Mushroom mushroomPrefab;
    public Transform mainCamera;

    public Mushroom[] board;
    public List<Morph> toBeMorphed;
    public float morphSpeed = 0.1f;

    public int ownMushroomSize = 3;
    public int ownSeedSize = 2;
    public int autoSeedSize = 1;

    void Start()
    {
        StopAllCoroutines();

        board = new Mushroom[size.x * size.y];
        toBeMorphed = new List<Morph>();

        for (int y = 0; y < size.y; ++y)
            for (int x = 0; x < size.x; ++x)
            {
                var go = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                go.name = string.Format("Tile {0}, {1}", x, y);
                go.transform.SetParent(transform);
            }

        mainCamera.transform.position = new Vector3(size.x / 2, size.y / 2, mainCamera.transform.position.z);

        StartCoroutine(InfiniteMorph());
    }

    void TrySpawningMushroom(Vector2Int position, int mushroomSize, int seedsSize)
    {
        if (!IsOnBoard(position))
        {
            return;
        }
        var current = board[ToIndex(position)];
        if (current == null)
        {
            SpawnMushroom(position, mushroomSize, seedsSize);
        }
    }

    void TryGrowMushroom(Vector2Int position, int size)
    {
        if (!IsOnBoard(position))
        {
            return;
        }
        var current = board[ToIndex(position)];
        if (current == null)
        {
            SpawnMushroom(position, size, 0);
        }
        else {
            current.Grow(size);
        }
    }

    void SpawnMushroom(Vector2Int position, int mushroomSize, int seedsSize)
    {
        var go = Instantiate(mushroomPrefab, new Vector3(position.x, position.y), Quaternion.identity);
        go.name = string.Format("Mushroom {0},{1}", position.x, position.y);
        go.transform.SetParent(transform);
      
        var mushroom = go.GetComponent<Mushroom>();
        mushroom.SetSize(mushroomSize);

        board[ToIndex(position)] = mushroom;

        if (seedsSize > 0)
        {
            toBeMorphed.Add(new Morph(new Vector2Int(position.x - 1, position.y), seedsSize));
            toBeMorphed.Add(new Morph(new Vector2Int(position.x + 1, position.y), seedsSize));
            toBeMorphed.Add(new Morph(new Vector2Int(position.x, position.y - 1), seedsSize));
            toBeMorphed.Add(new Morph(new Vector2Int(position.x, position.y + 1), seedsSize));
        }
    }

    void Explode(Mushroom mushroom) {
        var position = new Vector2Int(Mathf.RoundToInt(mushroom.transform.position.x), Mathf.RoundToInt(mushroom.transform.position.y));
        toBeMorphed.Add(new Morph(new Vector2Int(position.x - 1, position.y), autoSeedSize));
        toBeMorphed.Add(new Morph(new Vector2Int(position.x + 1, position.y), autoSeedSize));
        toBeMorphed.Add(new Morph(new Vector2Int(position.x, position.y - 1), autoSeedSize));
        toBeMorphed.Add(new Morph(new Vector2Int(position.x, position.y + 1), autoSeedSize));

        GameObject.Destroy(mushroom.gameObject);
    }

    private IEnumerator InfiniteMorph()
    {
        while (true)
        {
            if (toBeMorphed.Count > 0)
            {
                var morph = toBeMorphed[0];
                toBeMorphed.RemoveAt(0);

                var current = board[ToIndex(morph.position)];
                if (current)
                {
                    current.Grow(morph.size);

                    if (current.size >=Mushroom.maxSize)
                    {
                        Explode(current);
                    }
                } else if (current == null)
                {
                    SpawnMushroom(morph.position, morph.size, 0);
                }
            }
            yield return new WaitForSeconds(morphSpeed);
        }
    }

    List<Mushroom> FindSquare()
    {
        for (int y = 0; y < size.y - 1; ++y)
        {
            for (int x = 0; x < size.x - 1; ++x)
            {
                Mushroom tl = TryGettingMushroom(new Vector2Int(x, y));
                Mushroom tr = TryGettingMushroom(new Vector2Int(x + 1, y));
                Mushroom bl = TryGettingMushroom(new Vector2Int(x, y + 1));
                Mushroom br = TryGettingMushroom(new Vector2Int(x + 1, y + 1));

                if (tl && tl.size >= Mushroom.maxSize &&
                    tr && tr.size >= Mushroom.maxSize &&
                    bl && bl.size >= Mushroom.maxSize &&
                    br && br.size >= Mushroom.maxSize)
                {
                    var square = new List<Mushroom>();
                    square.Add(tl);
                    square.Add(tr);
                    square.Add(bl);
                    square.Add(br);

                    return square;
                }
            }
        }

        return null;
    }

    Mushroom TryGettingMushroom(Vector2Int position)
    {
        if (IsOnBoard(position))
        {
            var index = ToIndex(position);
            return board[index];
        }
        return null;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && toBeMorphed.Count == 0)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                var position = hit.transform.position;
                var gridPosition = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

                TrySpawningMushroom(gridPosition, ownMushroomSize, ownSeedSize);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
        }
    }

    void Restart()
    {

        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        Start();
    }
    bool IsOnBoard(Vector2Int position)
    {
        return position.x >= 0 && position.x < size.x && position.y >= 0 && position.y < size.y;
    }
    int ToIndex(Vector2Int position)
    {
        return position.y * size.x + position.x;
    }
}
