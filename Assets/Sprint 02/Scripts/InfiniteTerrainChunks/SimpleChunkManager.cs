using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimpleChunkManager : MonoBehaviour
{
    public GameObject player;
    public GameObject planePrefab;
    public float chunkSize = 10f;
    public int viewDistance = 3;
    public Color outerChunkColor = Color.red; 
    public Color middleChunkColor = Color.yellow;
    public Color innerChunkColor = Color.green;

    private Vector3 lastPlayerPosition;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> chunkPool = new Queue<GameObject>();

    void Start()
    {
        lastPlayerPosition = player.transform.position;
        UpdateTerrain();
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, lastPlayerPosition) > chunkSize * 0.5f)
        {
            lastPlayerPosition = player.transform.position;
            UpdateTerrain();
        }
    }

    void UpdateTerrain()
    {
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x / chunkSize),
            Mathf.FloorToInt(player.transform.position.z / chunkSize)
        );

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + z);

                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    Vector3 chunkPosition = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
                    GameObject chunk = GetChunkFromPool(chunkPosition);
                    chunk.name = $"Chunk ({chunkCoord.x}, {chunkCoord.y})";
                    chunk.transform.SetParent(transform);

                    bool isOuterChunk = Mathf.Abs(x) == viewDistance || Mathf.Abs(z) == viewDistance;
                    bool isMiddleChunk = Mathf.Abs(x) == viewDistance - 1 || Mathf.Abs(z) == viewDistance - 1;
                    SetChunkColor(chunk, isOuterChunk, isMiddleChunk);

                    activeChunks.Add(chunkCoord, chunk);
                }
                else
                {
                    bool isOuterChunk = Mathf.Abs(x) == viewDistance || Mathf.Abs(z) == viewDistance;
                    bool isMiddleChunk = Mathf.Abs(x) == viewDistance - 1 || Mathf.Abs(z) == viewDistance - 1;
                    SetChunkColor(activeChunks[chunkCoord], isOuterChunk, isMiddleChunk);
                }
            }
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunkCoord in activeChunks.Keys)
        {
            if (Mathf.Abs(chunkCoord.x - playerChunkCoord.x) > viewDistance ||
                Mathf.Abs(chunkCoord.y - playerChunkCoord.y) > viewDistance)
            {
                chunksToRemove.Add(chunkCoord);
            }
        }

        foreach (var chunkCoord in chunksToRemove)
        {
            ReturnChunkToPool(activeChunks[chunkCoord]);
            activeChunks.Remove(chunkCoord);
        }
    }

    GameObject GetChunkFromPool(Vector3 position)
    {
        GameObject chunk;
        if (chunkPool.Count > 0)
        {
            chunk = chunkPool.Dequeue();
            chunk.transform.position = position;
            chunk.SetActive(true);
        }
        else
        {
            chunk = Instantiate(planePrefab, position, Quaternion.identity);
        }

        return chunk;
    }

    void ReturnChunkToPool(GameObject chunk)
    {
        chunk.SetActive(false);
        chunkPool.Enqueue(chunk);
    }

    void SetChunkColor(GameObject chunk, bool isOuterChunk, bool isMiddleChunk)
    {
        Renderer renderer = chunk.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (isOuterChunk)
            {
                renderer.material.color = outerChunkColor;
            }
            else if (isMiddleChunk)
            {
                renderer.material.color = middleChunkColor;
            }
            else
            {
                renderer.material.color = innerChunkColor;
            }
        }
    }
}
