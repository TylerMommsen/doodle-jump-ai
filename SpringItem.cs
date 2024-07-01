using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringItem : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision) {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb.velocity.y <= 0.5f) {
            Vector2 vel = rb.velocity;
            vel.y = 20f;
            rb.velocity = vel;
        }
    }
}
