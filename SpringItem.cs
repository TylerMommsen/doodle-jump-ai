using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringItem : MonoBehaviour
{
    // boost player high when collided with spring
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb.velocity.y <= 0.5f) {
                Vector2 vel = rb.velocity;
                vel.y = 20f;
                rb.velocity = vel;
            }
        }
    }
}
