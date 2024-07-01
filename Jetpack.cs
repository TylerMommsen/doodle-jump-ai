using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : MonoBehaviour
{
    public Sprite[] sprites;
    private float speed = 15f;
    private float duration = 3f;

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
                transform.localPosition = new Vector3(0.4f, 0, 0);
                StartCoroutine(ApplyJetpack(playerRb, player));
            }
        }
    }

    private IEnumerator ApplyJetpack(Rigidbody2D playerRb, Player player) {
        float elapsedTime = 0f;
        int lastFrameIndex = -1;  

        while (elapsedTime < duration) {
            playerRb.velocity = new Vector2(playerRb.velocity.x, speed);

            int frameIndex = (int)(elapsedTime / duration * sprites.Length);
            if (frameIndex != lastFrameIndex && frameIndex < sprites.Length) {
                spriteRenderer.sprite = sprites[frameIndex];
                lastFrameIndex = frameIndex;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.isUsingItem = false;
        Destroy(gameObject);
    }
}
