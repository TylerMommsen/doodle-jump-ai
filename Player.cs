using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    private float moveInput;
    private float moveSpeed = 6f;

    public SpriteRenderer spriteRenderer;

    public Sprite faceLeftJumping;
    public Sprite faceLeftFalling;
    public Sprite shootingJumpingSprite;
    public Sprite shootingFallingSprite;
    public GameObject shootingPipe;

    public GameObject pelletPrefab;

    private bool isShooting = false;
    private float shootingTimer = 0.3f;

    public bool isUsingItem = false;

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");

        HandleScreenWrapping();
        UpdateSprite();

        if (Input.GetKeyDown(KeyCode.W) && !isUsingItem) {
            Shoot();
        }

        if (shootingTimer <= 0) {
            isShooting = false;
        } else {
            shootingTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate() {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void HandleScreenWrapping() {
        if (transform.position.x < -3.1f) {
            transform.position = new Vector3(3.1f, transform.position.y, transform.position.z);
        } else if (transform.position.x > 3.1) {
            transform.position = new Vector3(-3.1f, transform.position.y, transform.position.z);
        }
    }

    void UpdateSprite() {
        bool isJumping = rb.velocity.y > 6f;

        if (isShooting == false) {
            if (isJumping) {
                spriteRenderer.sprite = faceLeftJumping;
            } else {
                spriteRenderer.sprite = faceLeftFalling;
            }
        }

        if (moveInput > 0) {
            transform.localScale = new Vector3(-1, 1, 1);  // Flip horizontally
        } else if (moveInput < 0) {
            transform.localScale = new Vector3(1, 1, 1);   // Normal orientation
        }
    }

    void Shoot() {
        isShooting = true;
        GameObject pellet = Instantiate(pelletPrefab, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Quaternion.identity);
        Rigidbody2D pelletRb = pellet.GetComponent<Rigidbody2D>();
        pelletRb.velocity = Vector2.up * 10f;

        StartCoroutine(ChangeToShootingSprite());
    }

    IEnumerator ChangeToShootingSprite() {
        if (rb.velocity.y > 6f) {
            spriteRenderer.sprite = shootingJumpingSprite;
        } else {
            spriteRenderer.sprite = shootingFallingSprite;
        }

        GameObject pipe = Instantiate(shootingPipe, new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), Quaternion.identity, gameObject.transform);
        shootingTimer = 0.3f;
        yield return new WaitForSeconds(0.3f);
        Destroy(pipe);
    }
}
