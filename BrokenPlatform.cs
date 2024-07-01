using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenPlatform : MonoBehaviour
{
    public Sprite brokenPlatformSprite;
    private float fadeDuration = 0.3f;
    private float moveDownSpeed = 0.2f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (collision.CompareTag("Player") && rb.velocity.y <= 0.1f) {
            BreakPlatform();
        }
    }

    void BreakPlatform() {
        spriteRenderer.sprite = brokenPlatformSprite;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut() {
        Color originalColor = spriteRenderer.color;
        float elapsedTime = 0;

        Vector3 startPos = transform.position;
        float moveDistance = moveDownSpeed * fadeDuration;

        // gradually decrease opacity of the platform
        while (elapsedTime < fadeDuration) {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            float newY = Mathf.Lerp(startPos.y, startPos.y - moveDistance, elapsedTime / fadeDuration);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        Destroy(gameObject);
    }
}
