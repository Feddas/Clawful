using System.Collections;
using UnityEngine;

/// <summary> Spawns Balls at random locations in a region matching the size of BallSpawners' transform. </summary>
public class BallSpawner : MonoBehaviour
{
    [Tooltip("How many seconds until the next spawn.")]
    [SerializeField]
    private float SecondsBetweenSpawn = 0.1f;

    [Tooltip("What gameobject the spawned objects will be childed to.")]
    [SerializeField]
    private Transform SpawnParent;

    [Tooltip("Spawner stops spawning until there are fewer than this many.")]
    [SerializeField]
    private int MaxObjects = 30;

    [Header("Balls")]
    [SerializeField]
    private BlobBall BlobBallPrefab;

    [SerializeField]
    private Color[] Colors = new Color[] { Color.white, Color.red };

    [Header("Bombs")]
    [SerializeField]
    private BombBall BombBallPrefab;

    [SerializeField]
    [Range(0f, 1f)]
    private float PercentBombs = 0.1f;

    IEnumerator Start()
    {
        Random.InitState(42); // ensures levels are always spawned the same way. This makes testing easier.
        var pointValues = (BlobBall.PointsEnum[])System.Enum.GetValues(typeof(BlobBall.PointsEnum));

        float nextPrefab;
        Vector3 spawnPoint;

        while (true)
        {
            yield return new WaitUntil(() => SpawnParent.childCount < MaxObjects);
            yield return new WaitForSeconds(SecondsBetweenSpawn);

            spawnPoint = transform.TransformPoint(new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), 0));
            nextPrefab = Random.value;

            if (nextPrefab < PercentBombs)
            {
                Instantiate(BombBallPrefab, spawnPoint, Quaternion.identity, SpawnParent);
            }
            else
            {
                var nextBall = Instantiate(BlobBallPrefab, spawnPoint, Quaternion.identity, SpawnParent);
                nextBall.Color = Colors[Random.Range(0, Colors.Length)];
                nextBall.PointValue = pointValues[Random.Range(0, pointValues.Length)];
                nextBall.name = $"ball{nextBall.Color.b}-{spawnPoint.x:F1}-{nextBall.PointValue}"; // ball color - ball column - ball value
                nextBall.OnValidate();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
