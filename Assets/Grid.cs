using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2Int size;
    public GameObject tilePrefab;
    public GameObject bigMushroomPrefab;
    public GameObject smallMushroomPrefab;
    public Transform mainCamera;

    public Mushroom[] grid;
    public List<Vector2Int> toBeMorphed;
    public float morphSpeed = 0.1f;

	void Awake() {
		StartCoroutine(InfiniteMorph());
	}
    void Start()
    {
        grid = new Mushroom[size.x * size.y];
        toBeMorphed = new List<Vector2Int>();

        for (int y = 0; y < size.y; ++y)
            for (int x = 0; x < size.x; ++x)
            {
                var go = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                go.name = string.Format("Tile {0}, {1}", x, y);
                go.transform.SetParent(transform);
            }

        mainCamera.transform.position = new Vector3(size.x / 2, size.y / 2, mainCamera.transform.position.z);
    }

    void TrySpawningMushroom(Vector2Int position)
    {
        if (!IsOnBoard(position))
        {
            return;
        }
        var current = grid[ToIndex(position)];
        if (current == null)
        {
            SpawnMushroom(position);
        }
    }

    void SpawnMushroom(Vector2Int position)
    {
        var go = Instantiate(bigMushroomPrefab, new Vector3(position.x, position.y), Quaternion.identity);
        go.name = string.Format("Mushroom {0},{1}", position.x, position.y);
        go.transform.SetParent(transform);
        grid[ToIndex(position)] = go.GetComponent<Mushroom>();

        TrySpawningSmallMushroom(new Vector2Int(position.x - 1, position.y));
        TrySpawningSmallMushroom(new Vector2Int(position.x + 1, position.y));
        TrySpawningSmallMushroom(new Vector2Int(position.x, position.y - 1));
        TrySpawningSmallMushroom(new Vector2Int(position.x, position.y + 1));
    }

    void TrySpawningSmallMushroom(Vector2Int position)
    {
        if (!IsOnBoard(position))
        {
            return;
        }
        var index = ToIndex(position);

        var current = grid[index];
        if (current == null)
        {
            var go = Instantiate(smallMushroomPrefab, new Vector3(position.x, position.y), Quaternion.identity);
            go.name = string.Format("Small Mushroom {0},{1}", position.x, position.y);
            go.transform.SetParent(transform);
            grid[index] = go.GetComponent<Mushroom>();
        }
        else if (current.size == Mushroom.Size.Small)
        {
            toBeMorphed.Add(position);
        }
    }

    private IEnumerator InfiniteMorph()
    {
        while (true)
        {
            if (toBeMorphed.Count > 0)
            {
				var position = toBeMorphed[0];
				toBeMorphed.RemoveAt(0);

				var current = grid[ToIndex(position)];
				if (current.size == Mushroom.Size.Small) {
                	Destroy(current.gameObject);
                	grid[ToIndex(position)] = null;

                	TrySpawningMushroom(position);
				}
            }
			yield return new WaitForSeconds(morphSpeed);
        }
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

                TrySpawningMushroom(gridPosition);
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
