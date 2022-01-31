using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 3.0f;
    public float jumpStrength = 5.0f;
    public Vector2 move;

    public PlayerInput input { get; private set; }

    Rigidbody2D body;
    Collider2D col;
    Transform model;
    SpriteRenderer left;
    SpriteRenderer right;

    PlayerController other;

    Transform joinImage;
    Transform partImage;

    public GameObject joinParticles;
    Image cancelImg;

    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        model = transform.GetChild(0);
        left = model.GetChild(0).GetComponent<SpriteRenderer>();
        right = model.GetChild(1).GetComponent<SpriteRenderer>();

        cancelImg = GameObject.Find("CancelImg").GetComponent<Image>();

        var pcs = FindObjectsOfType<PlayerController>();
        foreach (var pc in pcs) {
            if (pc != this) {
                other = pc;
            }
        }

        var joiners = GameObject.Find("JoinerImages").transform;
        joinImage = joiners.GetChild(0);
        partImage = joiners.GetChild(1);

        joinImage.gameObject.SetActive(false);
        partImage.gameObject.SetActive(false);

    }

    public void SetInput(PlayerInput input) {
        this.input = input;
        if (input) {
            input.actions["Move"].performed += OnMove;
            input.actions["Move"].canceled += OnMoveCancelled;
            input.actions["Jump"].performed += OnJump;
            input.actions["OfferSide"].performed += OnOfferSide;
            input.actions["OfferSide"].canceled += OnOfferSideCancel;
            input.actions["Cancel"].performed += OnCancel;
            input.actions["Cancel"].canceled += OnCancelCancel;
            input.actions["Swap"].performed += OnSwap;
        }
    }

    void OnEnable() {
        SetInput(input);
    }

    void OnDisable() {
        if (input) {
            input.actions["Move"].performed -= OnMove;
            input.actions["Move"].canceled -= OnMoveCancelled;
            input.actions["Jump"].performed -= OnJump;
            input.actions["OfferSide"].performed -= OnOfferSide;
            input.actions["OfferSide"].canceled -= OnOfferSideCancel;
            input.actions["Cancel"].performed -= OnCancel;
            input.actions["Cancel"].canceled -= OnCancelCancel;
            input.actions["Swap"].performed -= OnSwap;
        }
    }

    bool grounded = false;
    static RaycastHit2D[] results = new RaycastHit2D[16];
    void FixedUpdate() {
        timeSinceFlip += Time.deltaTime;
        jumpCooldown -= Time.deltaTime;
        cancelHoldTime += Time.deltaTime;
        timeSinceHitRock += Time.deltaTime;
        treeTime += Time.deltaTime;
        timeSinceStayed += Time.deltaTime;

        Vector3 m = move;
        if (joined) {
            m += new Vector3(other.move.x, other.move.y, 0.0f);
            m /= 2.0f;
        }

        body.velocity = new Vector3(m.x * moveSpeed, body.velocity.y, 0.0f);

        if (jumpCooldown < 0f) {
            int count = Physics2D.RaycastNonAlloc(transform.position, Vector2.down, results, 0.7f);
            for (int i = 0; i < count; ++i) {
                var g = results[i].collider.gameObject;
                if (g != gameObject && g != model.gameObject && (g.tag != "Cat" && g.tag != "Goat" && g.tag != "Bird" && g.tag != "Fish")) {
                    grounded = true;
                    return;
                }
            }
        }
    }

    bool readyToJoin = false;
    bool joined = false;
    void Update() {
        // right player initiates the transfer
        readyToJoin = false;
        if (offer != 0 && other.offer != 0 && offer != other.offer) {
            if (Vector3.Distance(transform.position, other.transform.position) < 2.0f) {
                readyToJoin = true;
            } else {
                joinImage.gameObject.SetActive(false);
            }
        } else {
            joinImage.gameObject.SetActive(false);
        }

        if (readyToJoin && offer == 1) {
            joinImage.position = (transform.position + other.transform.position) / 2.0f + Vector3.up;
            joinImage.gameObject.SetActive(true);
        }

        if (joined && right.enabled) { // head decides
            if (cancelDown && other.cancelDown) {
                float minHold = Mathf.Min(cancelHoldTime, other.cancelHoldTime);
                cancelImg.fillAmount = minHold;
                if (minHold > 1.0f) {
                    UnJoinSequence();
                }
            }
        }

        if (model.tag == "Cat" && treeTime < 0.5f) {
            if (body.simulated) {
                if (body.velocity.y < 0.0f) {
                    Vector3 v = body.velocity;
                    v.y = 0.0f;
                    body.velocity = v;
                }
            } else {
                if (other.body.velocity.y < 0.0f) {
                    Vector3 v = other.body.velocity;
                    v.y = 0.0f;
                    other.body.velocity = v;
                }
            }
        }
    }

    void OnOfferSideCancel(InputAction.CallbackContext ctx) {
        right.color = left.color = Color.white;
        offer = 0;
        joinImage.gameObject.SetActive(false);
        partImage.gameObject.SetActive(false);
    }
    public int offer = 0;
    void OnOfferSide(InputAction.CallbackContext ctx) {
        if (joined) {
            offer = 0;
            return;
        }
        Color tintColor = Color.grey;
        float f = ctx.ReadValue<float>();
        offer = (int)f;
        if (model.localScale.x < 0) {
            offer *= -1;
        }
        if (offer == 1) { // offer right
            left.color = tintColor;
        } else if (offer == -1) { // offer left
            right.color = tintColor;
        }

    }

    float timeSinceFlip = 0.0f;
    void OnMove(InputAction.CallbackContext ctx) {
        move = ctx.ReadValue<Vector2>();
        if (timeSinceFlip > 0.05f && !(joined && left.enabled)) {
            if (move.x < 0) {
                model.localScale = new Vector3(-1, 1, 1);
                if (joined && right.enabled) {
                    other.model.localScale = new Vector3(-1, 1, 1);
                }
            } else if (move.x > 0) {
                model.localScale = new Vector3(1, 1, 1);
                if (joined && right.enabled) {
                    other.model.localScale = new Vector3(1, 1, 1);
                }
            }
            timeSinceFlip = 0.0f;
        }
    }

    void OnMoveCancelled(InputAction.CallbackContext ctx) {
        move = Vector2.zero;
    }

    float jumpCooldown = 0.0f;
    void OnJump(InputAction.CallbackContext ctx) {
        if (readyToJoin) {
            if (offer == 1) {
                InitiateJoinSequence();
            } else if (other.offer == 1) {
                other.InitiateJoinSequence();
            }

        } else if (jumpCooldown < 0.0f) {
            var bodyUse = body.simulated ? body : other.body;
            var groundUse = body.simulated ? grounded : other.grounded;

            if (model.tag == "Cat") {
                if (groundUse) {
                    bodyUse.velocity = new Vector3(bodyUse.velocity.x, jumpStrength, 0.0f);
                    grounded = false;
                    other.grounded = false;
                    jumpCooldown = 0.25f;
                }
            } else if (model.tag == "Goat") {
                if (groundUse || timeSinceHitRock < 0.25f) {
                    bodyUse.velocity = new Vector3(bodyUse.velocity.x, jumpStrength, 0.0f);
                    grounded = false;
                    other.grounded = false;
                    jumpCooldown = 0.25f;
                }
            } else if (model.tag == "Bird") {
                bodyUse.velocity = new Vector3(bodyUse.velocity.x, 0.0f, 0.0f);
                jumpCooldown = 0.25f;
            } else if (model.tag == "Fish") {

            }
        }
    }

    public void SetJoinVariables() {
        if (offer == 1) {
            left.enabled = false;
        } else {
            right.enabled = false;
        }
        readyToJoin = false;
        joined = true;
    }

    public void SetUnjoinVariables() {
        left.enabled = true;
        right.enabled = true;
        readyToJoin = false;
        joined = false;
    }

    public void InitiateJoinSequence() {
        other.body.simulated = false;
        other.col.enabled = false;

        SetJoinVariables();
        other.SetJoinVariables();

        other.transform.parent = transform;
        other.transform.localPosition = Vector3.zero;
        other.transform.localScale = Vector3.one;

        other.model.localScale = model.localScale;

        SpawnClouds();
    }

    public void UnJoinSequence() {
        other.transform.parent = null;
        other.transform.position = transform.position + Vector3.right * 0.1f;

        other.body.simulated = true;
        other.col.enabled = true;

        SetUnjoinVariables();
        other.SetUnjoinVariables();

        cancelImg.fillAmount = 0.0f;

        SpawnClouds();
    }

    void SpawnClouds() {
        // spawn a particles
        Destroy(Instantiate(joinParticles, transform.position, Quaternion.identity, transform), 2.5f);
    }

    public bool cancelDown = false;
    public float cancelHoldTime = 0.0f;
    // hold B / X to cancel join
    void OnCancel(InputAction.CallbackContext ctx) {
        cancelHoldTime = 0.0f;
        cancelDown = true;
    }

    void OnCancelCancel(InputAction.CallbackContext ctx) {
        cancelDown = false;
        cancelImg.fillAmount = 0.0f;
    }

    Transform stayModel = null;
    float timeSinceStayed = 5.0f;
    float treeTime = 5.0f;
    private void OnTriggerStay2D(Collider2D collider) {
        if (collider.tag == "Tree") {
            treeTime = 0.0f;
        }
        if (collider.transform.parent == null &&
            (collider.tag == "Cat" ||
            collider.tag == "Bird" ||
            collider.tag == "Goat" ||
            collider.tag == "Fish")) {
            timeSinceStayed = 0.0f;
            stayModel = collider.gameObject.transform;
        }
    }

    float timeSinceHitRock = 0.0f;
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.tag == "Rock") {
            timeSinceHitRock = 0.0f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.collider.tag == "Rock") {
            timeSinceHitRock = 0.0f;
        }
    }

    void OnSwap(InputAction.CallbackContext ctx) {
        if (joined) {
            return;
        }
        if (timeSinceStayed < 0.1f) {
            if (stayModel != null && stayModel.transform.parent == null) {
                model.parent = null;
                model.transform.position = stayModel.position;
                stayModel.parent = transform;
                stayModel.SetAsFirstSibling();
                stayModel.localPosition = Vector3.zero;
                stayModel.localScale = model.localScale;

                var newleft = stayModel.GetChild(0).GetComponent<SpriteRenderer>();
                var newright = stayModel.GetChild(1).GetComponent<SpriteRenderer>();
                newleft.sortingOrder = left.sortingOrder;
                newright.sortingOrder = right.sortingOrder;
                stayModel = null;

                model = transform.GetChild(0);
                left = model.GetChild(0).GetComponent<SpriteRenderer>();
                right = model.GetChild(1).GetComponent<SpriteRenderer>();

            }
        }

    }
}
