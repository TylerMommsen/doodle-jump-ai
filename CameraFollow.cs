using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.05f;

    private Vector3 currVel;

    void LateUpdate() {
        if (player != null) {
            if (player.position.y > transform.position.y) {
                Vector3 newPos = new Vector3(transform.position.x, player.position.y, transform.position.z);
                // Vector3 smoothedPosition = Vector3.Lerp(transform.position, newPos, smoothSpeed);
                // transform.position = smoothedPosition;
                transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currVel, smoothSpeed * Time.deltaTime);
            }
        }
    }
}
