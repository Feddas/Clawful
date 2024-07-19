using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary> Manages the PuyoPuyo aspect of the game for a single player. </summary>
[RequireComponent(typeof(Tilemap))]
public class BlobGrid : MonoBehaviour
{
    /// <summary> Balls can't be activated when above this height </summary>
    private const int gridHeight = 8;

    [SerializeField]
    private TextMeshProUGUI ScoreText;

    [Tooltip("tilemap aligned to the same transform as the blobtiles tilemap. overlays are used to add text over tiles in thie tilemap.")]
    [SerializeField]
    private Tilemap overlay;

    [Tooltip("all sprites (assigned to tiles) available to be overlays.")]
    [SerializeField]
    private Tile[] overlayTiles;

    // TODO: setup BlobTile[] array
    [SerializeField]
    private BlobTile whiteTile;

    [SerializeField]
    private BlobTile redTile;

    private int CurrentScore;
    private Vector2 cellCornerToCenter;
    private Dictionary<Vector2Int, BlobBall> blobRigidBodies = new Dictionary<Vector2Int, BlobBall>();
    private ScoreBlob scoreBlob;

    /// <summary> allows tilemap to be initialized before Start() </summary>
    private Tilemap tilemap
    {
        get
        {
            if (_tilemap == null)
            {
                initTilemap();
            }
            return _tilemap;
        }
    }
    private Tilemap _tilemap;

    /// <summary> Called by EnterGrid.cs's OnBlobEntered UnityEvent </summary>
    public void AddToGrid(BlobBall blobBall)
    {
        var newPosition = SnapToClosestCell(blobBall.transform.position);
        newPosition += cellCornerToCenter; // account for cell pivot in lower left and ball pivot in center
        blobBall.LerpTo(newPosition);

        var floor = blobBall.GetComponentInChildren<BallFloorEvent>();
        if (floor != null)
        {
            floor.GridName = this.name;
            floor.MaxHeight = this.transform.position.y + gridHeight + 0.6f;
            floor.OnHitFloor += Floor_OnHitFloor;  // TODO: -= when ball is cleared from scoring
            floor.OnFloorMissing += Floor_OnFloorMissing;
        }
    }

    private void initTilemap()
    {
        _tilemap = GetComponent<Tilemap>();
        scoreBlob = new ScoreBlob(_tilemap);
        scoreBlob.OnRequestDestroyCell += DestroyCell;
        scoreBlob.OnScored += AddScore;
        cellCornerToCenter = (Vector2)(tilemap.layoutGrid.cellSize / 2);
    }

    private void OnDestroy()
    {
        scoreBlob.OnRequestDestroyCell -= DestroyCell;
        scoreBlob.OnScored -= AddScore;
    }

    private void AddScore(int scoreDelta)
    {
        CurrentScore += scoreDelta;
        ScoreText.text = CurrentScore.ToString();
    }

    private void DestroyCell(Vector2Int position)
    {
        RemoveTile(position);

        if (blobRigidBodies.ContainsKey(position))
        {
            Destroy(blobRigidBodies[position].gameObject);
            blobRigidBodies.Remove(position);
        }
    }

    private void RemoveTile(Vector2Int position)
    {
        tilemap.SetTile((Vector3Int)position, null);
        overlay.SetTile((Vector3Int)position, null);
    }

    private void Floor_OnFloorMissing(BlobBall blobBall)
    {
        var gridPosition = tilemap.WorldToCell(blobBall.transform.position);
        RemoveTile((Vector2Int)gridPosition);
        blobBall.SetState(isATile: false);
    }

    private void Floor_OnHitFloor(BlobBall dropBall)
    {
        var targetTile = (Vector2Int)tilemap.WorldToCell(dropBall.transform.position);
        var afterlife = dropBall.PowerActivation(targetTile, scoreBlob);

        switch (afterlife)
        {
            default:
            case BlobBall.AfterUsed.Destroy:
                Destroy(dropBall.gameObject);
                break;
            case BlobBall.AfterUsed.AddToGrid:
                // deactivate rigidbody until tile needs to fall
                dropBall.SetState(isATile: true);
                blobRigidBodies[targetTile] = dropBall;

                // attach to tilemap
                if (dropBall.Color == whiteTile.Color)
                {
                    Set(whiteTile, targetTile, (int)dropBall.PointValue);
                }
                else if (dropBall.Color == redTile.Color)
                {
                    Set(redTile, targetTile, (int)dropBall.PointValue);
                }
                break;
        }
    }

    private void Set(TileBase coloredTile, Vector2Int position, int pointValue = -1)
    {
        // show and store point value
        if (pointValue != -1) // add overlay tile
        {
            overlay.SetTile((Vector3Int)position, overlayTiles[pointValue]);
            scoreBlob[position] = (byte)pointValue;
        }

        // link cell with neighboring cells
        this.tilemap.SetTile((Vector3Int)position, coloredTile);
    }

    /// <returns> the world position center of the tile closest to the given<paramref name="worldPosition"/></returns>
    private Vector2 SnapToClosestCell(Vector2 worldPosition)
    {
        var gridPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.CellToWorld(gridPosition);
    }

    private void Start()
    {
        initTilemap();

        // Test rule tiles render
        //Set(whiteTile, new Vector2Int(0, 0), 0);
        //Set(whiteTile, new Vector2Int(1, 1));
        //Set(whiteTile, new Vector2Int(1, 0), 10);
        //Set(redTile, new Vector2Int(0, 1), 1);
        //Set(redTile, new Vector2Int(0, 2), 9);
        //Set(redTile, new Vector2Int(0, 3), 4);
        //Set(whiteTile, new Vector2Int(0, 4), 1);
        //Set(whiteTile, new Vector2Int(0, 5), 9);
        //Set(whiteTile, new Vector2Int(0, 6), 10);
        //Set(whiteTile, new Vector2Int(0, 7), 4);

        //Set(redTile, new Vector2Int(2, 1), 8);
        //Set(redTile, new Vector2Int(2, 0), 9);
        //Set(redTile, new Vector2Int(3, 0), 6);
        //Set(redTile, new Vector2Int(4, 0), 10);
        //Set(whiteTile, new Vector2Int(5, 0));
        //Set(whiteTile, new Vector2Int(6, 0));
        //Set(whiteTile, new Vector2Int(7, 0));
        //Set(whiteTile, new Vector2Int(7, 1));
    }
}
