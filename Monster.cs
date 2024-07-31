using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Platform
{
    private float movementAmplitude = 0.05f; // The maximum distance the monster moves side to side
    private float movementFrequency = 30.0f; // The speed of the side to side movement

    public GameManager gameManager;
    private Vector3 startPosition;
    private float movementFactor; // Factor to calculate current position in sine wave movement

    protected override void PlayerJumped(Rigidbody2D playerRb)
    {
        base.PlayerJumped(playerRb);
    }

    void Start()
    {
        startPosition = transform.position; // Store the starting position
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        // Sine wave for side-to-side movement
        movementFactor = movementAmplitude * Mathf.Sin(Time.timeSinceLevelLoad * movementFrequency);
        transform.position = startPosition + new Vector3(movementFactor, 0, 0);
    }
}
