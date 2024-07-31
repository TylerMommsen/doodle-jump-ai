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

    // probabilities for items spawning, 7% spring, 2% propeller, 0.5% jetpack
    private float springProb = 0.07f;
    private float propellerProb = 0.02f;
    private float jetpackProb = 0.005f;

    public Transform player; // for when there is no ai, reference player here

    private float spawnY = -2.0f; // y pos for new platform
    private float spawnX; // x pos for new platform
    private float minSpacing = 0.5f; // minimum spacing between platforms
    private float maxSpacing = 0.5f; // maximum spacing between platforms
    private float score = 0; // holds the current highest score achieved by the current generation

    private bool createdBrokenPlatform = false; // ensure 2 broken platforms do not spawn one after another

    // probabilities for spawning different types of platforms, these are updated based on the current game score
    private float regularThreshold = 0.9f;
    private float brokenThreshold = 1f;
    private float movingThreshold = 0.99f;
    private float disappearingThreshold = 1f;

    private GameObject currentSpawnedPlatform;

    public GameObject[] monsters; // holds the monster prefab objects
    private bool spawnedMonster = false;
    private int timesWithoutSpawning = 0;
    private float lastCheckedScore = 0; // used to see how long its been since monster spawn

    public CameraFollow cameraFollow; // cam follow script

    public GameObject obstaclesContainer; // container for all obstacles/platforms

    // ai stuff
    private int popSize = 80;
    public int aliveCounter = 5;
    private int genCounter = 1;
    private float allTimeBestScore = 0; // holds the highest score achieved by ai
    public Population population; // population object
    public GameObject tempPopObject; // object which is used to temporarily hold the old population while calculations are done
    public NetworkVisualizer networkVisualizer;
    private float bestPlayerY = 0; // the current best player's y position
    public GameObject currentHighestPlayer; // the gameobject of the current highest player
    private bool resettingGen = false; // checking to see if starting next gen

    // segment ai training by toggling these phases
    private bool isSpawningMonsters = false;
    private bool isSpawningLotsOfMonsters = false;
    private bool isSpawningItems = false;

    // ui stuff
    public TextMeshProUGUI genCounterUI;
    public TextMeshProUGUI aliveCounterUI;
    public TextMeshProUGUI allTimeBestScoreUI;
    private bool visualizingTopPlayer = false;
    public TextMeshProUGUI scoreText;

    // initialize the first population of players
    void Awake() {
        population.InitializePopulation(popSize);
    }

    void Start()
    {
        // create initial platform at the start of the game
        for (int i = 0; i < 24; i++) {
            SpawnPlatform(true);
        }

        // create initial network visualization with the very first player
        networkVisualizer.BuildNetwork(population.population[0].brain);

        aliveCounter = popSize;
        UpdateAlive();
    }

    // main game loop
    void FixedUpdate()
    {
        if (population.gameObject.transform.childCount < popSize || tempPopObject.transform.childCount > 0) return;

        if (population.AllDead() == false) {
            population.UpdatePlayers();
            CalculateBestScoreAndHighestPlayer();

            // check if player has reached a certain height in order to spawn new platforms
            if (bestPlayerY > spawnY - (12 * minSpacing)) {
                SpawnPlatform(false);
            }

            if (isSpawningLotsOfMonsters && isSpawningMonsters) {
                if (score > 200 && score - lastCheckedScore >= 50 && spawnedMonster == false && score != 0) {
                    if (!spawnedMonster) {
                        spawnedMonster = true;
                        SpawnMonster();
                    }
                    lastCheckedScore = score;
                }
            } else if (isSpawningMonsters) {
                if (score > 2000 && score - lastCheckedScore >= 1000 && spawnedMonster == false && score != 0) {
                    if (!spawnedMonster) {
                        spawnedMonster = true;
                        SpawnMonster();
                    }
                    lastCheckedScore = score;
                }
            }

        } else {
            if (resettingGen) return;

            if (score > allTimeBestScore) {
                allTimeBestScore = score;
                allTimeBestScoreUI.text = "Best Score: " + score;
            }
            resettingGen = true;
            Reset();
            StartCoroutine(StartNextGen());
        }
    }

    private void Reset() {
        cameraFollow.Reset();

        score = 0;
        lastCheckedScore = 0;
        spawnY = -2.0f;
        bestPlayerY = 0;
        currentHighestPlayer = null;
        minSpacing = 0.5f;
        maxSpacing = 0.5f;

        regularThreshold = 0.9f;
        brokenThreshold = 1f;
        movingThreshold = 0.99f;
        disappearingThreshold = 1f;
    }

    private IEnumerator StartNextGen() {
        yield return new WaitForSeconds(3.1f);

        // destroy all existing platforms
        foreach (Transform child in obstaclesContainer.transform) {
            Destroy(child.gameObject);
        }

        // create the next population of players
        population.NaturalSelection();
        // update the neural network visualiztion
        networkVisualizer.BuildNetwork(population.bestPlayerBrain);

        // update counters
        genCounter++;
        UpdateGen();
        aliveCounter = popSize;
        UpdateAlive();
        scoreText.text = "" + score;

        // spawn new initial platforms
        for (int i = 0; i < 24; i++) {
            SpawnPlatform(true);
        }

        resettingGen = false;
    }

    void CalculateBestScoreAndHighestPlayer() {
        float currentHighestY = 0;
        Player highestPlayer = null;

        foreach (Player player in population.population) {
            // update global score
            if (player.score > score) {
                score = player.score;
                bestPlayerY = player.highestY;
                scoreText.text = "" + score;
                if (spawnedMonster) spawnedMonster = false;
            }

            // get the current highest player
            if (player.isAlive && player.gameObject.transform.position.y > currentHighestY) {
                currentHighestY = player.gameObject.transform.position.y;
                highestPlayer = player; // used by camera follow
            }
        }

        // if there is a new highest player, then move the camera to them
        if (highestPlayer != null && highestPlayer.gameObject != currentHighestPlayer) {
            if (currentHighestPlayer != null && highestPlayer.gameObject.transform.position.y < currentHighestPlayer.gameObject.transform.position.y) {
                if (visualizingTopPlayer) {
                    highestPlayer.visualizingRays = true;
                    currentHighestPlayer.GetComponent<Player>().visualizingRays = false;
                } 
                currentHighestPlayer = highestPlayer.gameObject;
                cameraFollow.MoveToNewHighestPlayer();
            }

            currentHighestPlayer = highestPlayer.gameObject;
        }
    }

    // update function for when there is no ai being used
    // void Update()
    // {
    //     if (player != null) {
    //         // check if player has reached a certain height in order to spawn new platforms
    //         if (player.position.y > spawnY - (12 * minSpacing)) {
    //             SpawnPlatform(false);
    //         }

    //         UpdateScore();
    //         if (score > 2000 && score - lastCheckedScore >= 1000 && spawnedMonster == false && score != 0) {
    //             if (!spawnedMonster) {
    //                 spawnedMonster = true;
    //                 SpawnMonster();
    //             }
    //             lastCheckedScore = score;
    //         }
    //     }
    // }

    void Update() {
        CheckUserInput();
    }

    void SpawnPlatform(bool isStart) {
        float randomValue = Random.value;

        createdBrokenPlatform = false;

        AdjustSpacing();
        CalculatePlatformThresholds();

        spawnX = Random.Range(-2.5f, 2.5f); // get random x pos for platform
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        // decide which kind of platform to spawn
        if (randomValue < regularThreshold) {
            currentSpawnedPlatform = Instantiate(regularPlatformPrefab, spawnPos, Quaternion.identity, obstaclesContainer.transform);
        } else if (randomValue < brokenThreshold && !isStart && score < 5000) {
            currentSpawnedPlatform = Instantiate(brokenPlatformPrefab, spawnPos, Quaternion.identity, obstaclesContainer.transform);
            createdBrokenPlatform = true;
        } else if (randomValue < movingThreshold && score > 3000) {
            currentSpawnedPlatform = Instantiate(movingPlatformPrefab, spawnPos, Quaternion.identity, obstaclesContainer.transform);
        } else if (randomValue < disappearingThreshold && score >= 7000) {
            currentSpawnedPlatform = Instantiate(disappearingPlatformPrefab, spawnPos, Quaternion.identity, obstaclesContainer.transform);
        } else {
            currentSpawnedPlatform = Instantiate(regularPlatformPrefab, spawnPos, Quaternion.identity, obstaclesContainer.transform);
        }

        if (!isStart && score > 1000 && isSpawningItems) {
            SpawnItemOnPlatform();
        }

        // increase y pos for next platform
        spawnY += Random.Range(minSpacing, maxSpacing);
    }

    // 30% chance to randomly spawn monster every 1000 score increase
    void SpawnMonster() {
        float rand = Random.value;
        if (rand < 0.3 || timesWithoutSpawning > 5) {
            timesWithoutSpawning = 0;
            int randMonster = Random.Range(0, 2);
            spawnX = Random.Range(-2.5f, 2.5f);
            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
            Instantiate(monsters[randMonster], spawnPos, Quaternion.identity, obstaclesContainer.transform);
        } else {
            timesWithoutSpawning++;
        }
    }

    // update score function for when there is no ai
    // void UpdateScore() {
    //     if (player.position.y > highestY) {
    //         highestY = player.position.y;
    //         score = Mathf.FloorToInt(highestY * 50);
    //         scoreText.text = "" + score;

    //         if (spawnedMonster) spawnedMonster = false;
    //     }
    // }

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
            minSpacing = 0.5f;
            maxSpacing = 0.5f;
        } else if (score < 2000) {
            minSpacing = 0.6f;
            maxSpacing = 0.8f;
        } else if (score < 3000) {
            minSpacing = 0.7f;
            maxSpacing = 0.9f;
        } else if (score < 4000) {
            minSpacing = 0.8f;
            maxSpacing = 1.1f;
        } else if (score < 6000) {
            minSpacing = 0.8f;
            maxSpacing = 1.3f;
        } else if (score < 10000) {
            minSpacing = 0.8f;
            maxSpacing = 1.5f;
        }
    }

    void UpdateGen() {
        genCounterUI.text = "Generation: " + genCounter.ToString();
    }

    public void UpdateAlive() {
        aliveCounterUI.text = "Alive: " + aliveCounter.ToString();
    }

    private void CheckUserInput() {
        // toggle visualization mode for top player (kinda works)
        if (Input.GetKeyDown(KeyCode.V)) {
            visualizingTopPlayer = !visualizingTopPlayer;

            if (visualizingTopPlayer) {
                currentHighestPlayer.GetComponent<Player>().visualizingRays = true;
            } else {
                currentHighestPlayer.GetComponent<Player>().visualizingRays = false;
            }
        }

        // toggle monster spawning mode
        if (Input.GetKeyDown(KeyCode.A)) {
            isSpawningMonsters = !isSpawningMonsters;
            CurrentPhase.Instance.isSpawningMonsters = isSpawningMonsters;
        }

        // toggle high monster spawn rate
        if (Input.GetKeyDown(KeyCode.S)) {
            isSpawningLotsOfMonsters = !isSpawningLotsOfMonsters;
            CurrentPhase.Instance.isSpawningLotsOfMonsters = isSpawningLotsOfMonsters;
        }

        // toggle spawning items
        if (Input.GetKeyDown(KeyCode.D)) {
            isSpawningItems = !isSpawningItems;
            CurrentPhase.Instance.isSpawningItems = isSpawningItems;
        }

        // timescale 1x
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Time.timeScale = 1.0f;
        }

        // timescale 2x
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Time.timeScale = 2.0f;
        }

        // timescale 3x
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Time.timeScale = 5.0f;
        }
    }
}
