using System;
using System.Collections;
using UnityEngine;

/// <summary> Individual ball that can be claw grabbed and thrown into the grid </summary>
public class BlobBall : MonoBehaviour
{
    /// <summary> Scores to be evaluated by ScoreBlob.cs's ScoreTilesLinkedTo switch statement </summary>
    public enum PointsEnum
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        MultiplierX2 = 10,
    }

    /// <summary> What should happen to this ball after it attaches to the grid. </summary>
    public enum AfterUsed
    {
        Destroy,
        AddToGrid,
    }

    /// <summary> References to child gameobjects specific to BlobBall </summary>
    [Serializable]
    public class HasParts
    {
        public SpriteRenderer Circle;
        public SpriteRenderer Text;
        public Rigidbody2D rb;
        public CircleCollider2D circleCollider;
        public Sprite[] TextSprites;
        public BallFloorEvent FloorSensor;
    }

    public BallFloorEvent FloorSensor { get { return this.subPart.FloorSensor; } }

    [Tooltip("Reference to child gameobjects for easy access in the script")]
    [SerializeField]
    private HasParts subPart;

    [Tooltip("Balls will group when their color exactly matches one another")]
    public Color Color = Color.white;

    [Tooltip("How this ball will contribute to the players score")]
    public PointsEnum PointValue;

    [Tooltip("Size of ball when it can be claw grabbed or when its floating in the water. 100% size is used while in a blob grid.")]
    [SerializeField]
    [Range(.1f, 1f)]
    private float GrabSize = 0.6f;

    public void OnValidate()
    {
        subPart.Circle.color = this.Color;

        Color inverted = new Color(1 - this.Color.r, 1 - this.Color.g, 1 - this.Color.b);
        subPart.Text.color = inverted;

        if (subPart.TextSprites != null && subPart.TextSprites.Length > (int)PointValue)
        {
            subPart.Text.sprite = subPart.TextSprites[(int)PointValue];
        }
        else
        {
            subPart.Text.sprite = null;
        }

        this.transform.localScale = Vector3.one * GrabSize;
    }

    /// <summary> What should happen to this ball after it attaches to the grid. </summary>
    public virtual AfterUsed PowerActivation(Vector2Int at, ScoreBlob scoring)
    {
        return AfterUsed.AddToGrid;
    }

    /// <summary> Toggle components used while the ball is moving that conflict with the ball when it is a tile. </summary>
    public void SetState(bool isATile)
    {
        this.subPart.rb.bodyType = isATile ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        if (isATile)
        {
            this.subPart.rb.velocity = Vector2.zero;
            this.subPart.rb.angularVelocity = 0;
        }
        this.subPart.Circle.transform.parent.gameObject.SetActive(false == isATile); // SetActive of "Visual"
        this.subPart.circleCollider.enabled = (false == isATile);
    }

    /// <summary> Prepares if a ball moved at <paramref name="position"/> would cause this ball to fall. </summary>
    public void OtherBallRemoved(Vector2 position)
    {
        // note: don't seem to need to handle +/-0.1f in x-value error. AKA, don't yet need IsInSameColumn(position)
        if (position.x != this.transform.position.x || position.y > this.transform.position.y)
        {
            return; // ball is in a different column or above this ball
        }

        this.FloorSensor.FallsUntil -= 1;

        // Debug.Log($"{this.name} at {this.transform.position} is falling to {this.FloorSensor.FallsUntil.ToString("F1")} due to ball removed at {position}");
        this.FloorSensor.SetNoFloor(this);
    }
}
