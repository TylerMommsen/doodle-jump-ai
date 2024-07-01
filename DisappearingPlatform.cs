using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : Platform
{
    private float fadeDuration = 0.5f;

    protected override void PlayerJumped(Rigidbody2D playerRb)
    {
        base.PlayerJumped(playerRb);
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        float elapsedTime = 0;

        // gradually decrease opacity of the platform
        while (elapsedTime < fadeDuration) {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        Destroy(gameObject);
    }
}
