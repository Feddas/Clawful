using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary> Scores a tile with its linked neighbors </summary>
public class ScoreBlob
{
    readonly Vector2Int[] neighborTiles = new Vector2Int[4] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    public event Action<Vector2Int> OnRequestDestroyCell;
    public event Action<int> OnScored;

    private Dictionary<Vector2Int, byte> tileValues = new Dictionary<Vector2Int, byte>();

    private Tilemap tilemap;
    private Tilemap overlay;

    public byte this[Vector2Int i]
    {
        get { return tileValues[i]; }
        set { tileValues[i] = value; }
    }

    public ScoreBlob(Tilemap tilemap)
    {
        this.tilemap = tilemap;
        this.overlay = tilemap.transform.GetChild(0).GetComponent<Tilemap>();
    }

    /// <summary> Determines score from tiles removed from connection <paramref name="position"/>. </summary>
    /// <returns> computed score </returns>
    public int ScoreTilesLinkedTo(Vector2Int position)
    {
        // track which tiles have been scored and what their score is
        var blobGroupPoints = new Dictionary<Vector2Int, byte>();
        ScoreTilesLinkedTo(position, blobGroupPoints);

        // sum points and multipliers
        int total = 0;
        int totalX2 = 0;
        foreach (BlobBall.PointsEnum pointValue in blobGroupPoints.Values)
        {
            switch (pointValue)
            {
                case BlobBall.PointsEnum.MultiplierX2:
                    totalX2++;
                    break;
                default:
                    total += (int)pointValue;
                    break;
            }
        }

        // apply multipliers
        total *= (int)Math.Pow(2, totalX2);

        // report score
        if (total > 0 && OnScored != null)
        {
            Debug.Log(tilemap.name + " SCORED " + total + " from " + position);
            OnScored.Invoke(total);
        }
        return total;
    }

    /// <summary> Finds all tiles linked to a position. Queueing their score before deleting their gameobjects </summary>
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
