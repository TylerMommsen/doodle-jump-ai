using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // public Transform player;
    public float smoothSpeed = 0.05f;

    private Vector3 currVel;
    private bool isResetting = false;
    private Coroutine resetPosition;

    public GameManager gameManager;
    private Vector3 initialPos;
    private bool isTransitioning = false;

    void Start() {
        initialPos = transform.position;
    }

    // for ai
    void LateUpdate() {
        if (!isTransitioning && gameManager.currentHighestPlayer != null) {
            // Get the player's y position and camera's current y position
            float playerY = gameManager.currentHighestPlayer.transform.position.y;
            float cameraY = transform.position.y;

            // Ensure the camera only moves up
            if (playerY > cameraY) {
                if (isResetting) {
                    StopCoroutine(resetPosition);
                    isResetting = false;
                }
                Vector3 newPos = new Vector3(transform.position.x, playerY, transform.position.z);
                transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currVel, smoothSpeed * Time.deltaTime);
            }
        }
    }

    public void MoveToNewHighestPlayer() {
        if (gameManager.currentHighestPlayer != null) {
            StartCoroutine(TransitionToNewHighestPlayer());
        }
    }

    private IEnumerator TransitionToNewHighestPlayer() {
        isTransitioning = true;

        float duration = 0.4f;  // Total duration of the movement
        float elapsed = 0.0f;   // Time elapsed since the start of the interpolation

        Vector3 startPosition = transform.position;  // Store the starting position
        Vector3 highestPlayerPos = gameManager.currentHighestPlayer.transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, highestPlayerPos.y, startPosition.z);

        while (elapsed < duration) {
            float t = elapsed / duration;
            // Ease in with a cubic function
            t = t * t * t;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);  // Smoothly interpolate between the start and end positions based on t
            elapsed += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }

        transform.position = targetPosition;
        isTransitioning = false;
    }

    public void Reset() {
        if (isResetting) {
            StopCoroutine(resetPosition);
        }
        isResetting = true;
        resetPosition = StartCoroutine(ResetPosition());
    }

    private IEnumerator ResetPosition() {
        StopCoroutine(TransitionToNewHighestPlayer());
        float duration = 0.4f;  // Total duration of the movement
        float elapsed = 0.0f;   // Time elapsed since the start of the interpolation

        if (transform.position.y > 13) {
            // transform.position = new Vector3(0, 13, 0);
            duration = 3f;  // Total duration of the movement
        }

        Vector3 startPosition = transform.position;  // Store the starting position

        while (elapsed < duration) {
            float t = elapsed / duration;
            t = 1 - Mathf.Pow(1 - t, 5); // ease out

            transform.position = Vector3.Lerp(startPosition, initialPos, t);  // Smoothly interpolate between the start and end positions based on t
            elapsed += Time.deltaTime;
            yield return null;  // Wait for the next frame
        }

        transform.position = initialPos;
        isResetting = false;
    }

    // for no ai
    // void LateUpdate() {
    //     if (player != null) {
    //         if (player.position.y > transform.position.y) {
    //             Vector3 newPos = new Vector3(transform.position.x, player.position.y, transform.position.z);
    //             transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currVel, smoothSpeed * Time.deltaTime);
    //         }
    //     }
    // }
}
