using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private GM gm;
    private Rigidbody2D rb;
    public float maxAngle = 60;
    private float speed;
    public float startSpeed = 5;
    public float speedChange = 0.5f;

    // colors
    private SpriteRenderer sprite;
    private SpriteRenderer shadow;
    private TrailRenderer line;
    private GradientColorKey[] keys;

    private void Awake()
    {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GM>();
        rb = GetComponent<Rigidbody2D>();

        sprite = GetComponent<SpriteRenderer>();
        shadow = transform.GetChild(0).GetComponent<SpriteRenderer>();
        line = GetComponent<TrailRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;

        // check if goal
        if (tag == "BlueWall")
        {
            gm.Score(0);
        }
        else if (tag == "RedWall")
        {
            gm.Score(1);
        }

        // check if player
        if (tag == "RedPlayer" || tag == "BluePlayer")
        {
            speed += speedChange;

            float pos = collision.transform.position.x;
            float width = collision.transform.localScale.x;
            float hit = collision.GetContact(0).point.x;

            float v = (hit - pos) / (width / 2);
            float angle = (Mathf.PI / 2) - v * (Mathf.PI / 180) * maxAngle;

            Vector2 bounce = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            if (tag == "BluePlayer")
            {
                bounce.y *= -1;
            }

            rb.velocity = bounce * speed;

            // adapt color
            ChangeColor(collision.gameObject.GetComponent<SpriteRenderer>().color);
        }
    }

    public void ResetPosition()
    {
        transform.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        line.Clear();
    }

    public void StartBounce(bool down)
    {
        speed = startSpeed;
        int[] vals = { -1, 1 };
        Vector2 dir = new Vector2(Random.Range(-1.5f, 1.5f), vals[Random.Range(0, 2)]);
        if (down) dir.y = -1;
        dir.Normalize();
        rb.AddForce(dir * speed, ForceMode2D.Impulse);
    }

    private void ChangeColor(Color c)
    {
        sprite.color = c;
        shadow.color = new Color(c.r, c.g, c.b, 0.25f);
        line.startColor = c;
    }

    public float getSpeed()
    {
        return speed;
    }
}
