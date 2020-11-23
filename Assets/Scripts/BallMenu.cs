using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMenu : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //StartBounce();
    }

    public void StartBounce()
    {
        int[] vals = { -1, 1 };
        //Vector2 dir = new Vector2(vals[Random.Range(0, 2)], vals[Random.Range(0, 2)]);
        Vector2 dir = new Vector2(1, 0);
        dir.Normalize();
        rb.AddForce(dir * speed, ForceMode2D.Impulse);
    }
}
