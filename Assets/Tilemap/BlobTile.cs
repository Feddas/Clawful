using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary> Custom UnityEngine.RuleTile for blob cells to visually auto-join and to track their per cell overlay value. </summary>
public class BlobTile : RuleTile
{
#if UNITY_EDITOR
    /// <summary> Add function to the UnityEditor to create BlobTiles </summary>
    [MenuItem("Assets/Create/2D/Clawful Tile")]
    public static void CreateAssetFile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Blob Tile", "New Blob Tile", "Asset", "Save Counting Tile", "Assets");
        if (path == "")
        {
            return;
        }

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BlobTile>(), path);
    }
#endif

    [Tooltip("Color of all tiles created with this asset.")]
    public Color Color = Color.red;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.color = Color;
    }
}
