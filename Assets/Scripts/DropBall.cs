using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DropBall:MonoBehaviour
{
    private Rigidbody2D rb;
    [Header("If is a point value ball")]
    public bool isMatcher;
    public string colorTag;
    public int pointValue;
    public bool isMultiplier;
    [Header("Match checking information")]
    public bool isInGrid;
    public bool isClaimed;
    public bool matchLeft = false;
    public bool matchRight = false;
    public bool matchTop = false;
    public bool matchBottom = false;
    [Header("Renderer info for point value balls")]
    public SpriteRenderer spriteRenderer;
    public List<Sprite> spritesList;

    [Header("Other possible variabels go here")]
    public GridNode nodeLocation;

    public TextMeshProUGUI displayValue;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (displayValue != null && isMatcher && pointValue != 0)
        {
            if (!isMultiplier)
            {
                displayValue.text = pointValue.ToString();
            }
            else
            {

                displayValue.text = "X";
            }
        }
    }
    public void MatchCheck()
    {
        if (matchLeft && matchRight && matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[9];
            // All matches (left, right, top, bottom)
        }
        else if (matchLeft && matchRight && matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[12];
            // Matches left, right, and top
        }
        else if (matchLeft && matchRight && !matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[6];
            // Matches left, right, and bottom
        }
        else if (matchLeft && !matchRight && matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[10];
            // Matches left, top, and bottom
        }
        else if (!matchLeft && matchRight && matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[8];
            // Matches right, top, and bottom
        }
        else if (matchLeft && matchRight && !matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[14];
            // Matches left and right
        }
        else if (matchLeft && !matchRight && matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[13];
            // Matches left and top
        }
        else if (matchLeft && !matchRight && !matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[7];
            // Matches left and bottom
        }
        else if (!matchLeft && matchRight && matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[11];
            // Matches right and top
        }
        else if (!matchLeft && matchRight && !matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[5];
            // Matches right and bottom
        }
        else if (!matchLeft && !matchRight && matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[15];
            // Matches top and bottom
        }
        else if (matchLeft && !matchRight && !matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[2];
            // Matches left only
        }
        else if (!matchLeft && matchRight && !matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[1];
            // Matches right only
        }
        else if (!matchLeft && !matchRight && matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[4];
            // Matches top only
        }
        else if (!matchLeft && !matchRight && !matchTop && matchBottom)
        {
            spriteRenderer.sprite = spritesList[3];
            // Matches bottom only
        }
        else if (!matchLeft && !matchRight && !matchTop && !matchBottom)
        {
            spriteRenderer.sprite = spritesList[0];
            // No matches
            //print("No matches");
        }
    }

    public void LoseConnection()
    {
        isClaimed = false;
        rb.isKinematic = false;
    }

    public void ClearMatches()
    {
        matchLeft = false;
        matchRight = false;
        matchTop = false;
        matchBottom = false;
        spriteRenderer.sprite = spritesList[0];
    }

    public void ValueChange(int i)
    {
        pointValue += i;
        displayValue.text = pointValue.ToString();
    }

    public virtual void PowerActivation()
    {
    }
}
