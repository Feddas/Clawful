using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float nodeSize = 1f;
    public GameObject gridNodePrefab;

    public GridNode[,] grid;

    public List<List<GridNode>> groups = new List<List<GridNode>>();

    public int score;
    public TextMeshProUGUI scoreDisplay;

    void Start()
    {
        CreateGrid();
        groups.Clear();
        Debug.Log(groups.Count);
    }

    void CreateGrid()
    {
        grid = new GridNode[gridWidth, gridHeight];
        int i = 1;
        Vector3 startPosition = transform.position;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 nodePosition = new Vector3(
                    startPosition.x + x * nodeSize,
                    startPosition.y - y * nodeSize,
                    startPosition.z
                );

                GameObject nodeObject = Instantiate(gridNodePrefab, nodePosition, Quaternion.identity);
                nodeObject.transform.parent = this.transform;
                nodeObject.name = nodeObject.name + i;

                GridNode gridNode = nodeObject.GetComponent<GridNode>();
                gridNode.gridX = x;
                gridNode.gridY = y;
                gridNode.nodeSize = nodeSize;

                grid[x, y] = gridNode;
                i++;
            }
        }
    }

    public GridNode GetNodeAtPosition(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }
        return null;
    }

    public void UpdateScore()
    {
        scoreDisplay.text = score.ToString();
    }
}
