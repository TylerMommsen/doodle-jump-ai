using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private float moveInput;
    private float moveSpeed = 6f;

    public SpriteRenderer spriteRenderer;

    // sprites for jumping, falling, shooting etc
    public Sprite faceLeftJumping;
    public Sprite faceLeftFalling;
    public Sprite shootingJumpingSprite;
    public Sprite shootingFallingSprite;

    public GameObject shootingPipePrefab; // shooting pipe prefab object
    private GameObject shootingPipe = null; // the shooting pipe attached to player when shooting

    public GameObject pelletPrefab; // pellet/bullets that players shoot

    private bool isShooting = false;
    public bool isUsingItem = false;

    // ai stuff
    public bool isAlive = true;
    private int inputsCount = 27;
    public List<float> vision;
    public List<float> decision;
    public Genome brain = new Genome(27, 4);
    public float fitness = 0;
    public float score = 0;
    public GameObject playerPrefab;
    private float timeWithoutIncreasingHeight = 0;
    public float highestY = 0;
    private GameManager gameManager;
    private float timeSpentMovingLeft = 0f;
    private float timeSpentMovingRight = 0f;
    private float timeSpentWithoutMoving = 0f;
    public int monstersKilled = 0;
    private int totalTimesShot = 0;
    public bool diedToMonster = false;

    // rays stuff
    private LineRenderer lineRenderer;
    private int numOfRays = 10;
    private float rayDistance = 6f;
    public LayerMask platformMask;
    public LayerMask monsterMask;
    public LayerMask itemMask;
    public bool visualizingRays = false; 
    public bool monsterIsInSight = false; // is monster above player

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.positionCount = numOfRays * 2; // Each ray needs two points
        gameManager = FindObjectOfType<GameManager>();

        Reset();
    }

    // the bottom 2 functions are for when there is no ai being used

    // void Update()
    // {
    //     moveInput = Input.GetAxis("Horizontal");

    //     HandleScreenWrapping();
    //     UpdateSprite();

    //     if (Input.GetKeyDown(KeyCode.W) && !isUsingItem) {
    //         Shoot();
    //     }

    //     if (shootingTimer <= 0) {
    //         isShooting = false;
    //     } else {
    //         shootingTimer -= Time.deltaTime;
    //     }
    //     CastRays();
    // }

    // void FixedUpdate() {
    //     rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    // }

    // teleport player to other side of screen when they go past boundaries
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
            transform.localScale = new Vector3(-1, 1, 1);  // flip horizontally (right)
        } else if (moveInput < 0) {
            transform.localScale = new Vector3(1, 1, 1);   // normal orientation (left)
        }
    }

    void Shoot() {
        if (isUsingItem || isShooting) return;

        if (!monsterIsInSight) {
            totalTimesShot++;
        }
        
        isShooting = true;
        GameObject pellet = Instantiate(pelletPrefab, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Quaternion.identity);
        Rigidbody2D pelletRb = pellet.GetComponent<Rigidbody2D>();
        Pellet pelletScript = pellet.GetComponent<Pellet>();

        // link player to pellet so when pellet hits a monster, monstersKilled variable increases
        if (pelletScript != null) {
            pelletScript.player = this;
        }
        pelletRb.velocity = Vector2.up * 20f;

        // update player sprite
        ChangeToShootingSprite();
    }

    private void ChangeToShootingSprite() {
        if (rb.velocity.y > 6f) {
            spriteRenderer.sprite = shootingJumpingSprite;
        } else {
            spriteRenderer.sprite = shootingFallingSprite;
        }

        if (shootingPipe != null) {
            Destroy(shootingPipe);
        }

        // create the shooting pipe and attach to player
        shootingPipe = Instantiate(shootingPipePrefab, new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), Quaternion.identity, gameObject.transform);

        StartCoroutine(DestroyShootingPipe());
    }

    IEnumerator DestroyShootingPipe() {
        yield return new WaitForSeconds(0.2f);
        Destroy(shootingPipe);
        shootingPipe = null;
        isShooting = false;
    }

    // ai stuff

    // reset player values at the start of every generation
    public void Reset() {
        highestY = 0;
        score = 0;
        isAlive = true;
        isShooting = false;
        fitness = 0;
        isUsingItem = false;
        moveInput = 0;
        gameObject.SetActive(true);
        transform.position = new Vector3(0, 0, 0);
        visualizingRays = false;
        monstersKilled = 0;
        totalTimesShot = 0;
        diedToMonster = false;
    }

    List<float> CastRays() {
        List<float> rayInputs = new List<float>();
        CastPlatformRays(rayInputs);
        CastMonsterRays(rayInputs);
        CastItemRays(rayInputs);
        return rayInputs;
    }

    // main raycasting function for detecting platforms, monsters and items
    void CastRaysOfType(List<float> rayInputs, float[] angles, LayerMask mask, string targetLayer) {
        if (targetLayer == "Monster") monsterIsInSight = false;

        for (int i = 0; i < angles.Length; i++) {
            Vector2 rayDirection = Quaternion.Euler(0, 0, angles[i]) * transform.up;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, rayDistance, mask);

            if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer(targetLayer)) {
                rayInputs.Add(hit.distance / rayDistance); // Normalize distance

                // if the ray going directly above the player hits a monster, then flag that there is a monster there
                if (targetLayer == "Monster" && i == 0) {
                    rayInputs.Add(1);
                    monsterIsInSight = true;
                }
            } else {
                rayInputs.Add(1); // max distance normalized
            }

            if (visualizingRays && targetLayer == "Monster") {
                SetRayVisualization(i, rayDirection, hit);
            }

            if (i == 0 && targetLayer == "Monster" && !monsterIsInSight) {
                rayInputs.Add(0);
            }
        }
    }

    void CastPlatformRays(List<float> rayInputs) {
        float[] angles = {0f, 45f, 75f, 105f, 135f, 180f, 225f, 255f, 285f, 315f};
        CastRaysOfType(rayInputs, angles, platformMask, "Platform");
    }

    void CastMonsterRays(List<float> rayInputs) {
        float[] angles = {0f, 15f, 45f, 90f, 105f, 255f, 270f, 315f, 345f};
        CastRaysOfType(rayInputs, angles, monsterMask, "Monster");
    }

    void CastItemRays(List<float> rayInputs) {
        float[] angles = {0f, 45f, 135f, 180f, 225f, 315f};
        CastRaysOfType(rayInputs, angles, itemMask, "Item");
    }

    // used for visualizing the rays for the top player
    void SetRayVisualization(int rayIndex, Vector2 rayDirection, RaycastHit2D hit) {
        int lineIndex = rayIndex * 2;
        lineRenderer.SetPosition(lineIndex, transform.position);

        Vector3 endPosition = hit.collider != null ? (Vector3)hit.point : transform.position + (Vector3)rayDirection * rayDistance;
        lineRenderer.SetPosition(lineIndex + 1, endPosition);
    }
 
    public void UpdatePlayer()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        UpdateScore();
        HandleScreenWrapping();
        UpdateSprite();

        // this is used to keep track of how long players are doing 1 thing for
        // so if they go right continuously for 5+ seconds, then kill em
        if (moveInput == -1) {
            timeSpentMovingLeft += Time.deltaTime;
            timeSpentMovingRight = 0f;
            timeSpentWithoutMoving = 0f;
        } else if (moveInput == 1) {
            timeSpentMovingLeft = 0f;
            timeSpentMovingRight += Time.deltaTime;
            timeSpentWithoutMoving = 0f;
        } else {
            timeSpentMovingLeft = 0f;
            timeSpentMovingRight = 0f;
            timeSpentWithoutMoving += Time.deltaTime;
        }

        // this is where they are killed for doing the same thing for too long
        if (timeSpentMovingLeft > 5f || timeSpentMovingRight > 5f || timeSpentWithoutMoving > 5f) {
            isAlive = false;
            gameManager.aliveCounter--;
            gameManager.UpdateAlive();
            gameObject.SetActive(false);
        }

        // if player falls below bounds, kill em
        if (transform.position.y < highestY - 5f) {
            isAlive = false;
            gameManager.aliveCounter--;
            gameManager.UpdateAlive();
            gameObject.SetActive(false);
        }
        // if players don't go up for more than 4 seconds, kill em
        if (timeWithoutIncreasingHeight >= 4f) {
            isAlive = false;
            gameManager.aliveCounter--;
            gameManager.UpdateAlive();
            gameObject.SetActive(false);
        } 
        timeWithoutIncreasingHeight += Time.deltaTime;
    }

    // update the player score when the player goes higher than it's highest height
    void UpdateScore() {
        if (transform.position.y > highestY) {
            highestY = transform.position.y;
            timeWithoutIncreasingHeight = 0f;
            score = Mathf.FloorToInt(highestY * 50);
        }
    }

    public void Look() {
        if (rb == null) return;

        vision = new List<float>(inputsCount);
        List<float> rayInputs = CastRays();

        vision.Add(1 / (1 + Mathf.Exp(-rb.velocity.x))); // x velocity
        vision.Add(1 / (1 + Mathf.Exp(-rb.velocity.y))); // y velocity
        vision.AddRange(rayInputs); // rays in all directions for platforms, monsters and items
    }

    public void Think() {
        if (rb == null) return;

        decision = brain.FeedForward(vision);

        float maxDecisionValue = decision.Max();
        int actionIndex = decision.IndexOf(maxDecisionValue);

        // if the ai is not confident, then don't move
        // if (maxDecisionValue < 0.55f) {
        //     moveInput = 0;
        //     return;
        // } 

        // take an action
        switch (actionIndex)
        {
            case 0:
                moveInput = -1; // move left
                break;
            case 1:
                moveInput = 1; // move right
                break;
            case 2:
                moveInput = 0;
                break; // don't move
            case 3:
                Shoot(); // you get it
                break;
            default:
                moveInput = 0;
                break;
        }
    }

    // calculate fitness based on score and monsters killed
    public void CalculateFitness() {
        fitness = score / 100.0f; // score for height reached

        fitness -= (totalTimesShot * 2); // punish uneccessary shooting

        // reward killing monsters
        if (monstersKilled > 0) {
            fitness += (monstersKilled * 30);
        }

        if (diedToMonster) fitness *= 0.7f; // punish for hitting monsters
        if (fitness < 0) fitness = 0;

        fitness = fitness * fitness; // square the fitness
    }

    // clone the player
    public Player Clone(int name, GameObject parentContainer) {
        GameObject cloneObject = Instantiate(playerPrefab, parentContainer.transform);
        cloneObject.SetActive(false);
        cloneObject.name = name.ToString();
        Player clone = cloneObject.GetComponent<Player>();
        clone.rb = cloneObject.GetComponent<Rigidbody2D>();
        clone.brain = brain.Clone();
        clone.brain.GenerateNetwork();
        return clone;
    }

    // crossover this player with another player
    public Player Crossover(Player parent2, int name, GameObject parentContainer) {
        GameObject childObject = Instantiate(playerPrefab, parentContainer.transform);
        childObject.SetActive(false);
        childObject.name = name.ToString();
        Player child = childObject.GetComponent<Player>();
        child.rb = childObject.GetComponent<Rigidbody2D>();
        child.brain = brain.Crossover(parent2.brain);
        child.brain.GenerateNetwork();
        return child;
    }
}
