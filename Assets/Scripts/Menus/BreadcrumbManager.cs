using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Tracks a hierarchy of gameobjects to allow traversing back to the root after opening a tree of children.  </summary>
public class BreadcrumbManager : MonoBehaviour
{
    Stack<GameObject> stack = new Stack<GameObject>();

    /// <summary> open new gameobject, closing last gameobject. For UI, gameobject is typically a modal window.
    /// Called from UnityEvents in PauseMenu </summary>
    public void OpenNewCrumb(GameObject newLeaf)
    {
        if (stack.Count > 0)
        {
            var closing = stack.Peek();
            closing.SetActive(false);
        }

        stack.Push(newLeaf);
        newLeaf.SetActive(true);
    }

    /// <summary> open previous gameobject, close current gameobject AKA press the back button.
    /// Called from UnityEvents in PauseMenu </summary>
    public void CloseCurrent()
    {
        var closing = stack.Pop();
        closing.SetActive(false);

        if (stack.Count > 0)
        {
            var opening = stack.Peek();
            opening.SetActive(true);
        }
    }

    public void CloseAll(int but = 0)
    {
        do
        {
            stack.Pop().SetActive(false);
        } while (stack.Count > but);

        if (stack.Count > 0)
        {
            var opening = stack.Peek();
            opening.SetActive(true);
        }
    }
}
