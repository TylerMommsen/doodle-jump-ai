using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    private float duration = 0;
    private float maxDuration = 1f;

    void Update()
    {
        duration += Time.deltaTime;

        if (duration >= maxDuration) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Monster")) {
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
    }
}
