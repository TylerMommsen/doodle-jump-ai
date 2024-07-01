using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // all platforms
    public GameObject regularPlatformPrefab;
    public GameObject movingPlatformPrefab;
    public GameObject disappearingPlatformPrefab;
    public GameObject brokenPlatformPrefab;

    // items
    public GameObject spring;
    public GameObject propeller;
    public GameObject Jetpack;

    private float springProb = 0.07f;
    private float propellerProb = 0.02f;
    private float jetpackProb = 0.005f;

    public Transform player;
    public TextMeshProUGUI scoreText;

    private float spawnY = -2.0f;
    private float spawnX;
    private float platformSpacing = 0.5f; // distance between platforms
    private float highestY = 0; // track the highest point the player has reached
    private float score = 0;

    private bool createdBrokenPlatform = false; // ensure 2 broken platforms do not spawn one after another

    private float regularThreshold = 0.9f;
    private float brokenThreshold = 1f;
    private float movingThreshold = 0.99f;
    private float disappearingThreshold = 1f;

    private GameObject currentSpawnedPlatform;

    public GameObject[] monsters;
    private bool spawnedMonster = false;
    private int timesWithoutSpawning = 0;
    private float lastCheckedScore = 0;

    void Start()
    {
        // create initial platform at the start of the game
        for (int i = 0; i < 24; i++) {
            SpawnPlatform(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null) {
            // check if player has reached a certain height in order to spawn new platforms
            if (player.position.y > spawnY - (12 * platformSpacing)) {
                SpawnPlatform(false);
            }

            UpdateScore();
            if (score > 2000 && score - lastCheckedScore >= 1000 && spawnedMonster == false && score != 0) {
                if (!spawnedMonster) {
                    spawnedMonster = true;
                    SpawnMonster();
                }
                lastCheckedScore = score;
            }
        }
    }

    void SpawnPlatform(bool isStart) {
        float randomValue = Random.value;

        AdjustSpacing();
        CalculatePlatformThresholds();

        // create a new random value if a broken platform was already made
        while (randomValue < 0.8f && randomValue >= 0.7f && createdBrokenPlatform) {
            randomValue = Random.value;

            if (randomValue < 0.7f || randomValue >= 0.8f) {
                createdBrokenPlatform = false;
            }
        }

        spawnX = Random.Range(-2.5f, 2.5f);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        if (randomValue < regularThreshold) {
            currentSpawnedPlatform = Instantiate(regularPlatformPrefab, spawnPos, Quaternion.identity);
        } else if (randomValue < brokenThreshold && !isStart) {
            currentSpawnedPlatform = Instantiate(brokenPlatformPrefab, spawnPos, Quaternion.identity);
            createdBrokenPlatform = true;
        } else if (randomValue < movingThreshold && score > 3000) {
            currentSpawnedPlatform = Instantiate(movingPlatformPrefab, spawnPos, Quaternion.identity);
        } else if (randomValue < disappearingThreshold && score >= 7000) {
            currentSpawnedPlatform = Instantiate(disappearingPlatformPrefab, spawnPos, Quaternion.identity);
        } else {
            currentSpawnedPlatform = Instantiate(regularPlatformPrefab, spawnPos, Quaternion.identity);
        }

        SpawnItemOnPlatform();

        spawnY += Random.Range(0.5f, platformSpacing);
    }

    void SpawnMonster() {
        float rand = Random.value;
        if (rand < 0.3 || timesWithoutSpawning > 5) {
            timesWithoutSpawning = 0;
            int randMonster = Random.Range(0, 2);
            spawnX = Random.Range(-2.5f, 2.5f);
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
            Instantiate(monsters[randMonster], spawnPos, Quaternion.identity);
        } else {
            timesWithoutSpawning++;
        }
    }

    void UpdateScore() {
        if (player.position.y > highestY) {
            highestY = player.position.y;
            score = Mathf.FloorToInt(highestY * 50);
            scoreText.text = "" + score;

            if (spawnedMonster) spawnedMonster = false;
        }
    }

    void SpawnItemOnPlatform() {
        if (createdBrokenPlatform) return;

        float itemRand = Random.value;

        if (itemRand < jetpackProb) {
            Instantiate(Jetpack, new Vector3(spawnX, spawnY + 0.5f, 0), Quaternion.identity, currentSpawnedPlatform.transform);
        }
        else if (itemRand < propellerProb) {
            Instantiate(propeller, new Vector3(spawnX, spawnY + 0.3f, 0), Quaternion.identity, currentSpawnedPlatform.transform);
        }
        else if (itemRand < springProb) {
            Instantiate(spring, new Vector3(spawnX, spawnY + 0.22f, 0), Quaternion.identity, currentSpawnedPlatform.transform);
        }


    }

    // change platform spawn probabilities with higher scores
    void CalculatePlatformThresholds() {
        if (score < 3000) {
            regularThreshold = 0.9f;
            brokenThreshold = 1f;
        }
        else if (score < 7000) {
            regularThreshold = 0.75f;
            brokenThreshold = 0.85f;
            movingThreshold = 0.99f;
            disappearingThreshold = 1f;
        }
        else if (score > 10000) {
            regularThreshold = 0.7f;
            brokenThreshold = 0.8f;
            movingThreshold = 0.98f;
            disappearingThreshold = 1f;
        }
        else if (score > 15000) {
            regularThreshold = 0.65f;
            brokenThreshold = 0.75f;
            movingThreshold = 0.96f;
            disappearingThreshold = 1f;
        }
    }

    void AdjustSpacing() {
        // Adjust platform spacing based on the score
        if (score < 1000) {
            platformSpacing = 0.5f;
        } else if (score < 2000) {
            platformSpacing = 0.6f;
        } else if (score < 3000) {
            platformSpacing = 0.7f;
        } else if (score < 4000) {
            platformSpacing = 0.8f;
        } else if (score < 6000) {
            platformSpacing = 0.9f;
        } else if (score < 10000) {
            platformSpacing = 1.0f;
        } else if (score < 11000) {
            platformSpacing = 1.1f;
        } else if (score < 12000) {
            platformSpacing = 1.2f;
        } else if (score < 13000) {
            platformSpacing = 1.3f;
        } else if (score < 15000) {
            platformSpacing = 1.5f;
        }
    }
}
