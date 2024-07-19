using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RopeLimiter : MonoBehaviour
{
    public string mask;
    private float time;
    public float duration = 0.2f;
    public Transform zNode;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == mask && other.transform.position.y < transform.position.y)
        {
            other.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            time = 0;
            StartCoroutine(LevelJoint(other.transform));
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == mask && other.transform.position.y < transform.position.y)
        {
            other.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
        else if (other.gameObject.tag == mask)
        {
            other.transform.rotation = Quaternion.identity;
            other.transform.localPosition = new Vector3(zNode.transform.localPosition.x, other.transform.position.y, 0);
        }
    }

    IEnumerator LevelJoint(Transform t)
    {

        while (time < duration)
        {
            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.identity, time / duration);
            //t.position = Vector3.Lerp(t.position, new Vector3(zNode.position.x, t.position.y, t.position.x), time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        t.rotation = Quaternion.identity;
        //t.position = new Vector3(zNode.position.x, t.position.y, t.position.x);
    }
}
