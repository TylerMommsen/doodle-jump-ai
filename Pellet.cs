using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    public Player player;
    private float duration = 0;
    private float maxDuration = 0.4f;

    void Update()
    {
        duration += Time.deltaTime;

        if (duration >= maxDuration) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster")) {
            if (player != null && player.monsterIsInSight) {
                player.monstersKilled++;
            }
            Destroy(collision.transform.parent.gameObject); // destroy monster
            Destroy(gameObject); // destroy pellet
        }
    }
}
