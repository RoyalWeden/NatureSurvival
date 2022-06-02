using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour {
    // Animal attributes
    public GameManager.AnimalType animalType;

    // Movement
    private float moveSpeed;
    private float generalMoveSpeed;
    private bool isMoving;
    private float nextMoveTime;
    private Vector2 nextMovePos;

    // Animal animation
    private Animator animator;

    // Health / Hit
    private int health;
    private BoxCollider2D animalCollider;
    private Transform hitEffect;
    private Animator hitEffectAnimator;
    private SpriteRenderer hitEffectRenderer;
    private float invincibleTime;
    private float hitDistance;

    // Attacking
    private bool canAttack;
    private bool isAttacking;
    private float attackCooldownLength;
    private float nextAttackTime;
    private float attackRange;
    private Transform itemHold;
    private Transform item;
    private SpriteRenderer itemRenderer;
    private BoxCollider2D itemCollider;
    private Vector2 playerLastPos;
    private float itemMoveSpeed;
    private bool itemReachedDestination;

    // Player
    private Transform player;
    private BoxCollider2D playerCollider;
    private PlayerController playerController;
    private BoxCollider2D spearCollider;

    // Other
    private SpriteRenderer spriteRenderer;
    private float generalHitDistance;

    void Start() {
        // Object hierarchy locations
        spriteRenderer = transform.Find("Skin").GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animalCollider = GetComponent<BoxCollider2D>();

        hitEffect = transform.Find("HitEffect");
        hitEffectAnimator = hitEffect.GetComponent<Animator>();
        hitEffectRenderer = hitEffect.GetComponent<SpriteRenderer>();

        itemHold = transform.Find("Item Hold");
        item = itemHold.Find("Item");
        itemRenderer = item.Find("Item Skin").GetComponent<SpriteRenderer>();
        itemCollider = itemRenderer.GetComponent<BoxCollider2D>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = player.GetComponent<BoxCollider2D>();
        playerController = player.GetComponent<PlayerController>();
        spearCollider = player.Find("Spear Hold").Find("Spear").GetComponent<BoxCollider2D>();

        // Instance values
        itemHold.gameObject.SetActive(false);
        SetAnimalAttributes();
        hitEffectRenderer.enabled = false;
        nextAttackTime = 5f + Time.fixedTime;
        generalMoveSpeed = 0.05f;
        nextMoveTime = 1.5f + Time.fixedTime;
        itemMoveSpeed = 0.65f;
        generalHitDistance = 0.01f;

        // Fix parent scaling
        if(transform.parent != null || !transform.parent.name.Contains("Pond")) {
            Transform tempParent = transform.parent;
            transform.parent = null;
            transform.localScale = new Vector3(1, 1, 1);
            transform.parent = tempParent;
        }
    }

    void Update() {
        AttackActions();
        AnimalMovement();
        itemRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        hitEffectRenderer.sortingOrder = spriteRenderer.sortingOrder + 2;
        GetHit();
    }

    private void GetHit() {
        if(animalCollider == null || spearCollider == null || player == null) {
            animalCollider = GetComponent<BoxCollider2D>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            try {
                spearCollider = player.Find("Spear Hold").Find("Spear").GetComponent<BoxCollider2D>();
            } catch {
                
            }
        }
        if(Time.fixedTime > invincibleTime && (animalCollider.bounds.Intersects(spearCollider.bounds) || (animalType == GameManager.AnimalType.Hippo && Vector2.Distance(spearCollider.transform.position, transform.position) < 0.095f))) {
            invincibleTime = Time.fixedTime + 1.5f;
            health--;
            playerController.ResetSpear();
            Vector2 heading = (Vector2)transform.position - (Vector2)spearCollider.transform.position;
            float distance = heading.magnitude;
            Vector2 hitDirection = heading / distance;
            transform.position += (Vector3)hitDirection * generalHitDistance;
        }
        if(health <= 0) {
            GameManager.IncreaseScore();
            Destroy(item.gameObject);
            Destroy(gameObject);
        }
    }

    private void AnimalMovement() {
        if(!isAttacking) {
            playerLastPos = player.position;
            if(isMoving) {
                Vector2 heading = nextMovePos - (Vector2)transform.position;
                float distance = heading.magnitude;
                Vector2 moveDir = heading / distance;
                transform.position += (Vector3)(moveDir * generalMoveSpeed * Time.deltaTime);
                if(moveDir.x < transform.position.x) {
                    // Fix parent scaling
                    if(transform.parent != null || !transform.parent.name.Contains("Pond")) {
                        Transform tempParent = transform.parent;
                        transform.parent = null;
                        transform.localScale = new Vector3(-1, 1, 1);
                        transform.parent = tempParent;
                    } else {
                        transform.localScale = new Vector3(-1, 1, 1);
                    }
                } else if(moveDir.x > transform.position.x) {
                    // Fix parent scaling
                    if(transform.parent != null || !transform.parent.name.Contains("Pond")) {
                        Transform tempParent = transform.parent;
                        transform.parent = null;
                        transform.localScale = new Vector3(1, 1, 1);
                        transform.parent = tempParent;
                    } else {
                        transform.localScale = new Vector3(1, 1, 1);
                    }
                }
                if(Vector2.Distance(transform.position, nextMovePos) < 0.05f) {
                    isMoving = false;
                    nextMoveTime += Random.Range(4.5f, 7f);
                }
            } else if(Mathf.Abs(nextMoveTime - Time.fixedTime) < 0.1f) {
                isMoving = true;
                float idleMoveRange = 0.2f;
                if(transform.parent == null || !transform.parent.name.Contains("Pond")) {
                    nextMovePos = new Vector2(Random.Range(Mathf.Max(-.8f, transform.position.x - idleMoveRange), Mathf.Min(.8f, transform.position.x + idleMoveRange)), Random.Range(Mathf.Max(-.4f, transform.position.y - idleMoveRange), Mathf.Max(.4f, transform.position.y + idleMoveRange)));
                } else {
                    float parentScale = transform.parent.localScale.x;
                    Vector2 parentPos = transform.parent.position;
                    nextMovePos = new Vector2(Random.Range(Mathf.Max(parentPos.x - .125f * parentScale, transform.position.x - idleMoveRange), Mathf.Min(parentPos.x + .125f * parentScale, transform.position.x + idleMoveRange)), Random.Range(Mathf.Max(parentPos.y - .125f * parentScale, transform.position.y - idleMoveRange), Mathf.Max(parentPos.y + .125f * parentScale, transform.position.y + idleMoveRange)));
                }
            }
        }

        // Animation
        if(!isMoving) {
            animator.ResetTrigger("Move");
            animator.SetTrigger("Idle");
        } else {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Move");
        }
    }

    private void AttackActions() {
        if(Mathf.Abs(nextAttackTime - Time.fixedTime) < 0.1f) {
            canAttack = true;
        }
        if(canAttack && DistToPlayer() <= attackRange) {
            canAttack = false;
            isAttacking = true;
            nextAttackTime = Time.fixedTime + attackCooldownLength;
        }
        if(isAttacking) {
            switch(animalType) {
                case GameManager.AnimalType.Monkey:
                    item.parent = null;
                    Vector2 heading;
                    Vector2 throwDirectionVect = Vector2.zero;
                    float distance;
                    if(Vector2.Distance(item.position, playerLastPos) < 0.01f || itemReachedDestination) {
                        itemReachedDestination = true;
                        if(Vector2.Distance(item.position, itemHold.position) < 0.01f) {
                            isAttacking = false;
                        } else {
                            heading = (Vector2)itemHold.position - (Vector2)item.position;
                            distance = heading.magnitude;
                            throwDirectionVect = heading / distance;
                        }
                    } else {
                        heading = playerLastPos - (Vector2)item.position;
                        distance = heading.magnitude;
                        throwDirectionVect = heading / distance;
                    }
                    
                    // Check item collision
                    if(itemCollider.bounds.Intersects(playerCollider.bounds)) {
                        playerController.GetHit(GameManager.HitType.None, item.position, hitDistance);
                        itemReachedDestination = true;
                    }
                    item.position += (Vector3)(throwDirectionVect * itemMoveSpeed * Time.deltaTime);
                    item.Rotate(Vector3.back, 720 * Time.deltaTime);
                    break;
                case GameManager.AnimalType.Tiger:
                    isMoving = true;
                    Vector2 tigerHeading = playerLastPos - (Vector2)transform.position;
                    float tigerDistance = tigerHeading.magnitude;
                    Vector2 tigerDirectionVect = tigerHeading / tigerDistance;
                    if(tigerDirectionVect.x < transform.position.x) {
                        transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                    } else if(tigerDirectionVect.x > transform.position.x) {
                        transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                    }
                    if(animalCollider.bounds.Intersects(playerCollider.bounds)) {
                        playerController.GetHit(GameManager.HitType.Claw, transform.position, hitDistance * 2);
                        isAttacking = false;
                    }
                    if(Vector2.Distance(transform.position, playerLastPos) < 0.01f) {
                        isAttacking = false;
                    }
                    transform.position += (Vector3)(tigerDirectionVect * moveSpeed * Time.deltaTime);
                    break;
                case GameManager.AnimalType.Hippo:
                    isMoving = true;
                    float parentScale = transform.parent.localScale.x;
                    Vector2 parentPos = transform.parent.position;
                    Vector2 hippoPlayerLastPos = new Vector2(Mathf.Min(Mathf.Max(playerLastPos.x, parentPos.x - .125f * parentScale), parentPos.x + .125f * parentScale), Mathf.Min(Mathf.Max(playerLastPos.y, parentPos.y - .125f * parentScale), parentPos.y + .125f * parentScale));
                    Vector2 hippoHeading = hippoPlayerLastPos - (Vector2)transform.position;
                    float hippoDistance = hippoHeading.magnitude;
                    Vector2 hippoDirectionVect = hippoHeading / hippoDistance;
                    if(hippoDirectionVect.x < transform.position.x) {
                        // Fix parent scaling
                        if(transform.parent != null || !transform.parent.name.Contains("Pond")) {
                            Transform tempParent = transform.parent;
                            transform.parent = null;
                            transform.localScale = new Vector3(-1, 1, 1);
                            transform.parent = tempParent;
                        }
                    } else if(hippoDirectionVect.x > transform.position.x) {
                        // Fix parent scaling
                        if(transform.parent != null || !transform.parent.name.Contains("Pond")) {
                            Transform tempParent = transform.parent;
                            transform.parent = null;
                            transform.localScale = new Vector3(1, 1, 1);
                            transform.parent = tempParent;
                        }
                    }
                    if(Vector2.Distance(transform.position, player.Find("Bottom").position) < 0.095f) {
                        playerController.GetHit(GameManager.HitType.Bite, transform.position, hitDistance * 4, 2);
                        isAttacking = false;
                    }
                    if(Vector2.Distance(transform.position, hippoPlayerLastPos) < 0.01f) {
                        isAttacking = false;
                    }
                    transform.position += (Vector3)(hippoDirectionVect * moveSpeed * Time.deltaTime);
                    break;
            }
        } else {
            ResetItem();
            itemReachedDestination = false;
        }
    }

    private void SetAnimalAttributes() {
        switch(animalType) {
            case GameManager.AnimalType.Monkey:
                health = 3;
                moveSpeed = 0.125f;
                attackCooldownLength = 4.5f;
                attackRange = 0.85f;
                itemHold.gameObject.SetActive(true);
                animalCollider.offset = new Vector2(0.0255027f, -0.01939558f);
                animalCollider.size = new Vector2(0.1096883f, 0.1218548f);
                hitDistance = 0.01f;
                animator.ResetTrigger("isHippo");
                animator.ResetTrigger("isTiger");
                animator.SetTrigger("isMonkey");
                break;
            case GameManager.AnimalType.Tiger:
                health = 5;
                moveSpeed = 0.45f;
                attackCooldownLength = 10f;
                attackRange = 1.35f;
                animalCollider.offset = new Vector2(0.01013122f, -0.04489828f);
                animalCollider.size = new Vector2(0.1404313f, 0.07084939f);
                hitDistance = 0.03f;
                animator.ResetTrigger("isHippo");
                animator.ResetTrigger("isMonkey");
                animator.SetTrigger("isTiger");
                break;
            case GameManager.AnimalType.Hippo:
                health = 7;
                moveSpeed = 0.075f;
                attackCooldownLength = 4f;
                attackRange = 0.35f;
                animalCollider.offset = new Vector2(0f, -0.03654182f);
                animalCollider.size = new Vector2(0.1762694f, 0.08756233f);
                animator.ResetTrigger("isMonkey");
                animator.ResetTrigger("isTiger");
                animator.SetTrigger("isHippo");
                break;
            default:
                break;
        }
    }

    // Move item to original location & rotation
    private void ResetItem() {
        item.parent = itemHold;
        item.localPosition = Vector2.zero;
        item.localRotation = Quaternion.identity;
        item.localScale = Vector3.one;
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    /* Returns */

    // Get player distance
    private float DistToPlayer() {
        if(player == null) {
            try {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            } catch {
                GameManager.SetPlayer(GameObject.Find("Land Animals").GetComponent<EntitySpawner>().SpawnPlayer());
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }
        return Vector2.Distance(transform.position, player.position);
    }
}
