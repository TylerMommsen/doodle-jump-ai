using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerItem : MonoBehaviour
{
    public Sprite[] sprites;
    private float topSpeed = 10f;
    private float accelerationTime = 1f;
    private float duration = 3.0f;

    private SpriteRenderer spriteRenderer;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null) {
                if (other.transform.childCount > 0) return;

                Player player = other.GetComponent<Player>();
                player.isUsingItem = true;

                transform.SetParent(other.transform);
                transform.localPosition = new Vector3(0, 0.75f, 0);
                StartCoroutine(ApplyPropeller(playerRb, player));
            }
        }
    }

    private IEnumerator ApplyPropeller(Rigidbody2D playerRb, Player player) {
        float elapsedTime = 0f;
        float timeSinceLastSpriteChange = 0f;
        float spriteChangeInterval = 0.1f;  // How often to change sprites
        float initialVerticalSpeed = playerRb.velocity.y;

        int currentSpriteIndex = 0;
        spriteRenderer.sprite = sprites[currentSpriteIndex];

        while (elapsedTime < duration) {
            float currentSpeed = Mathf.Lerp(initialVerticalSpeed, topSpeed, elapsedTime / accelerationTime);
            playerRb.velocity = new Vector2(playerRb.velocity.x, currentSpeed);

            // Handle sprite changes independently of the main elapsed time
            if (timeSinceLastSpriteChange >= spriteChangeInterval) {
                currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
                spriteRenderer.sprite = sprites[currentSpriteIndex];
                timeSinceLastSpriteChange = 0; // Reset the time since the last sprite change
            }

            // Update timing variables
            elapsedTime += Time.deltaTime;
            timeSinceLastSpriteChange += Time.deltaTime;
            
            yield return null;
        }

        player.isUsingItem = false;
        Destroy(gameObject);
    }
}
