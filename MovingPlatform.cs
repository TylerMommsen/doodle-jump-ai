using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : Platform
{
    private float speed;
    private bool movingRight;

    void Start() {
        speed = Random.Range(1.0f, 1.5f);
        movingRight = Random.value < 0.5 ? true : false;
    }

    void Update() {
        if (movingRight) {
            transform.position += Vector3.right * speed * Time.deltaTime;
            if (transform.position.x >= 2.5f) {
                movingRight = false;
            }
        }
        else {
            transform.position -= Vector3.right * speed * Time.deltaTime;
            if (transform.position.x <= -2.5f) {
                movingRight = true;
            }
        }
    }
}
