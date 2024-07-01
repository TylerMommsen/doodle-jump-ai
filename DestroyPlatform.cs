using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlatform : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") || collision.gameObject.layer == LayerMask.NameToLayer("Monster")) {
            Destroy(collision.gameObject);
        }
    }
}
