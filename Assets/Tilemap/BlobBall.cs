using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class BlobBall : MonoBehaviour
{
    [Serializable]
    public class HasParts
    {
        public SpriteRenderer Circle;
        public SpriteRenderer Text;
        public Rigidbody2D rb;
        public CircleCollider2D circleCollider;
        public Sprite[] TextSprites;
    }

    public enum PointsEnum
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        MultiplierX2 = 10,
    }

    [Tooltip("Reference to child gameobjects for easy access in the script")]
    public HasParts subPart;

    [Tooltip("Balls will group when their color exactly matches one another")]
    public Color Color = Color.white;

    [Tooltip("How this ball will contribute to the players score")]
    public PointsEnum PointValue;

    [Tooltip("Size of ball when it can be claw grabbed or when its floating in the water. 100% size is used while in a blob grid.")]
    [Range(.1f, 1f)]
    public float GrabSize = 0.6f;

    void OnValidate()
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

    public void LerpTo(Vector3 position)
    {
        StartCoroutine(LerpToCoroutine(position));
    }

    private IEnumerator LerpToCoroutine(Vector3 end)
    {
        Vector2 start = this.transform.position;
        float time = 0;
        float columnDuration = 0.2f;
        while (time < columnDuration)
        {
            this.transform.position = Vector2.Lerp(start, end, time / columnDuration);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.identity, time / columnDuration);
            time += Time.deltaTime;
            yield return null;
        }
        this.transform.position = end;
        this.transform.rotation = Quaternion.identity;

        this.subPart.rb.bodyType = RigidbodyType2D.Dynamic;
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

    public enum AfterUsed
    {
        Destroy,
        AddToGrid,
    }

    public virtual AfterUsed PowerActivation(Vector2Int at, ScoreBlob scoring)
    {
        return AfterUsed.AddToGrid;
    }
}
