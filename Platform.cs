using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    private float jumpPower = 10f;

    protected virtual void PlayerJumped(Rigidbody2D playerRb) {
        Vector2 vel = playerRb.velocity;
        vel.y = jumpPower;
        playerRb.velocity = vel;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.relativeVelocity.y <= 0.3f) {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null) {
                PlayerJumped(playerRb);
            }
        }
    }
}
