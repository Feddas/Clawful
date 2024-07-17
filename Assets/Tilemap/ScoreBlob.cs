using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary> Scores a tile with its linked neighbors </summary>
public class ScoreBlob
{
    public event Action<Vector2Int> OnRequestDestroyCell;

    readonly Vector2Int[] neighborTiles = new Vector2Int[4] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    private Dictionary<Vector2Int, byte> tileValues = new Dictionary<Vector2Int, byte>();

    private Tilemap tilemap;
    private Tilemap overlay;

    public ScoreBlob(Tilemap tilemap)
    {
        this.tilemap = tilemap;
        this.overlay = tilemap.transform.GetChild(0).GetComponent<Tilemap>();
    }

    public byte this[Vector2Int i]
    {
        get { return tileValues[i]; }
        set { tileValues[i] = value; }
    }

    public int ScoreTilesLinkedTo(Vector2Int position)
    {
        // track which tiles have been scored and what their score is
        var blobGroupPoints = new Dictionary<Vector2Int, byte>();
        ScoreTilesLinkedTo(position, blobGroupPoints);

        // sum points
        int total = 0;
        foreach (var point in blobGroupPoints.Values)
        {
            total += point;
        }
        Debug.Log("SCORE of " + total + " from " + position);
        return total;
    }

    private void ScoreTilesLinkedTo(Vector2Int position, Dictionary<Vector2Int, byte> blobGroupPoints)
    {
        if (tilemap.HasTile((Vector3Int)position) == false || blobGroupPoints.ContainsKey(position))
        {
            return;
        }

        // add score
        if (tileValues.ContainsKey(position))
        {
            blobGroupPoints[position] = tileValues.ContainsKey(position) ? tileValues[position] : (byte)0;
            tileValues.Remove(position);
        }
        else // mark the tile as visited
        {
            blobGroupPoints[position] = 0;
        }

        // add score of neighbors
        var scoredTile = tilemap.GetTile<BlobTile>((Vector3Int)position);
        BlobTile neighborTile;
        foreach (var neighbor in neighborTiles)
        {
            neighborTile = tilemap.GetTile<BlobTile>((Vector3Int)(position + neighbor));
            if (neighborTile == scoredTile)
            {
                ScoreTilesLinkedTo(position + neighbor, blobGroupPoints);
            }
        }

        // remove tile
        if (OnRequestDestroyCell != null)
        {
            OnRequestDestroyCell.Invoke(position);
        }
    }
}
