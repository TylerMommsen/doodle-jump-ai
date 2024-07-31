using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCollide : Monster
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Player player = collision.transform.gameObject.GetComponent<Player>();
            if (player.isUsingItem) {
                Destroy(gameObject.transform.parent.gameObject);
                return;
            }

            // Check the direction of the collision
            Vector2 hit = collision.contacts[0].normal;
            float angle = Vector2.Angle(hit, Vector2.up);

            if (angle >= 135 && angle <= 225) // Monster is hit from above
            {
                PlayerJumped(collision.transform.GetComponent<Rigidbody2D>());
                Destroy(gameObject.transform.parent.gameObject); // Destroys the monster GameObject
            }
            else // Player hits the monster from any other direction
            {
                if (player.isAlive) {
                    player.diedToMonster = true;
                    player.isAlive = false;
                    gameManager.aliveCounter--;
                    gameManager.UpdateAlive();
                    player.gameObject.SetActive(false);
                    // Destroy(collision.gameObject); // Kill the player
                }
            }
        }
    }
}
