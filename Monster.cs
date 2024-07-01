using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Platform
{
    private float movementAmplitude = 0.05f; // The maximum distance the monster moves side to side
    private float movementFrequency = 30.0f; // The speed of the side to side movement

    private Vector3 startPosition;
    private float movementFactor; // Factor to calculate current position in sine wave movement

    protected override void PlayerJumped(Rigidbody2D playerRb)
    {
        base.PlayerJumped(playerRb);
    }

    void Start()
    {
        startPosition = transform.position; // Store the starting position
    }

    void Update()
    {
        // Sine wave for side-to-side movement
        movementFactor = movementAmplitude * Mathf.Sin(Time.timeSinceLevelLoad * movementFrequency);
        transform.position = startPosition + new Vector3(movementFactor, 0, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.gameObject.GetComponent<Player>().isUsingItem) {
                Destroy(gameObject);
                return;
            }

            // Check the direction of the collision
            Vector2 hit = collision.contacts[0].normal;
            float angle = Vector2.Angle(hit, Vector2.up);

            if (angle >= 135 && angle <= 225) // Monster is hit from above
            {
                Destroy(gameObject); // Destroys the monster GameObject
                PlayerJumped(collision.gameObject.GetComponent<Rigidbody2D>());
            }
            else // Player hits the monster from any other direction
            {
                Destroy(collision.gameObject); // Kill the player
            }
        }
    }
}
