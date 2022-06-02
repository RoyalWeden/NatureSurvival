using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    // Player type
    public GameManager.GenderType gender;

    // Movement
    private float moveSpeed;
    private bool isMoving;

    // Spear
    private Transform spear;
    private Transform spearHold;
    private bool throwingSpear;
    private float spearMoveSpeed;
    private Vector2 throwDirectionVect;
    private Vector2 spearEndThrowPos;
    private SpriteRenderer spearSpriteRenderer;

    // Player animation
    private Animator animator;

    // Health / Hit
    private int health;
    private Slider healthSlider;
    private Transform hitEffect;
    private Animator hitEffectAnimator;
    private SpriteRenderer hitEffectRenderer;
    private float invincibleTime;
    private float hitEffectAnimationTime;

    // Other
    private Vector2 mousePos;
    private Vector2 mouseWorldPos;
    private SpriteRenderer spriteRenderer;

    void Start() {
        // Object hierarchy locations
        spearHold = transform.Find("Spear Hold");
        spear = spearHold.Find("Spear");
        animator = GetComponent<Animator>();

        hitEffect = transform.Find("HitEffect");
        hitEffectAnimator = hitEffect.GetComponent<Animator>();
        hitEffectRenderer = hitEffect.GetComponent<SpriteRenderer>();

        spriteRenderer = transform.Find("Skin").GetComponent<SpriteRenderer>();
        spearSpriteRenderer = spear.Find("Spear Skin").GetComponent<SpriteRenderer>();

        healthSlider = GameObject.FindObjectOfType<Canvas>().transform.Find("Health Panel").Find("Health Bar Holder").GetComponent<Slider>();
        
        // Instantiate values
        health = 10;
        healthSlider.maxValue = healthSlider.value = health;
        moveSpeed = 0.25f;
        spearMoveSpeed = 1f;
        hitEffectRenderer.enabled = false;

        // Player look
        if(gender == GameManager.GenderType.Male) {
            animator.SetTrigger("isMan");
        }
        if(gender == GameManager.GenderType.Female) {
            animator.SetTrigger("isWoman");
        }
    }

    void Update() {
        mousePos = Input.mousePosition;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        DoPlayerActions();
        HideHitEffect();
        spearSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        hitEffectRenderer.sortingOrder = spriteRenderer.sortingOrder + 2;
    }

    // Reset player entirely
    public void ResetPlayer(Vector2 resetPos) {
        transform.position = resetPos;
        health = 10;
        if(healthSlider == null) {
            healthSlider = GameObject.FindObjectOfType<Canvas>().transform.Find("Health Panel").Find("Health Bar Holder").GetComponent<Slider>();
        }
        healthSlider.maxValue = healthSlider.value = health;
        ResetSpear();
    }

    // (1) Move player, (3) change facing direction, (3) throw spear, (4) update animation
    private void DoPlayerActions() {
        // Move position
        if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
            isMoving = true;
            transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * moveSpeed * Time.deltaTime;
            transform.position = new Vector2(Mathf.Min(Mathf.Max(transform.position.x, -.8f), .8f), Mathf.Min(Mathf.Max(transform.position.y, -.4f), .4f));
        } else {
            isMoving = false;
        }

        // Change facing direction
        if(mouseWorldPos.x < transform.position.x) {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        } else if(mouseWorldPos.x > transform.position.x) {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }

        // Throw spear
        if(Input.GetMouseButtonDown(0) && !throwingSpear) {
            ThrowSpear();
        }
        if(throwingSpear) {
            spear.position += (Vector3)(throwDirectionVect * spearMoveSpeed * Time.deltaTime);
            if(Vector2.Distance(spear.position, spearEndThrowPos) < 0.1f) {
                ResetSpear();
            }
        }

        // Moving changes
        if(isMoving) {
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Move");
        } else {
            animator.ResetTrigger("Move");
            animator.SetTrigger("Idle");
        }
    }

    // Move spear to original location & rotation
    public void ResetSpear() {
        throwingSpear = false;
        if(spear == null) {
            spearHold = transform.Find("Spear Hold");
            spear = spearHold.Find("Spear");
        }
        spear.parent = spearHold;
        spear.localPosition = Vector2.zero;
        spear.localRotation = Quaternion.identity;
        spear.localScale = Vector3.one;
    }

    // Throw spear at mouse location
    private void ThrowSpear() {
        spear.parent = null;
        Vector2 heading = mouseWorldPos - (Vector2)spear.position;
        float distance = heading.magnitude;
        throwDirectionVect = heading / distance;
        spearEndThrowPos = throwDirectionVect * 2.5f + (Vector2)spearHold.position;
        spear.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan(throwDirectionVect[1] / throwDirectionVect[0]));
        throwingSpear = true;
    }

    // Player hit animation and lose health
    public void GetHit(GameManager.HitType hitType, Vector2 objectLocation, float hitDistance, int damage = 1) {
        if(Time.fixedTime > invincibleTime && health > 0) {
            health -= damage;
            healthSlider.value = health;
            if(hitType != GameManager.HitType.None) {
                hitEffectRenderer.enabled = true;
            }
            hitEffectAnimationTime = Time.fixedTime + 1.25f;
            if(hitType == GameManager.HitType.Bite) {
                hitEffectAnimator.ResetTrigger("Claw");
                hitEffectAnimator.SetTrigger("Bite");
            } else if(hitType == GameManager.HitType.Claw) {
                hitEffectAnimator.ResetTrigger("Bite");
                hitEffectAnimator.SetTrigger("Claw");
            }
            Vector2 heading = (Vector2)transform.position - objectLocation;
            float distance = heading.magnitude;
            Vector2 hitDirection = heading / distance;
            transform.position += (Vector3)hitDirection * hitDistance;
        }
        invincibleTime = Time.fixedTime + 3f;
    }

    // Hide hit effect after animation completed
    private void HideHitEffect() {
        if(Time.fixedTime > hitEffectAnimationTime) {
            hitEffectRenderer.enabled = false;
        }
    }


    /* Returns */

    // Return player health
    public int GetHealth() {
        return health;
    }
}