using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 3.0f;
    public float jumpStrength = 5.0f;
    Vector2 move;

    public PlayerInput input { get; private set; }

    Rigidbody2D body;
    Transform model;
    SpriteRenderer left;
    SpriteRenderer right;

    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody2D>();

        model = transform.GetChild(0);
        left = model.GetChild(0).GetComponent<SpriteRenderer>();
        right = model.GetChild(1).GetComponent<SpriteRenderer>();
    }

    public void SetInput(PlayerInput input) {
        this.input = input;
        if (input) {
            input.actions["Move"].performed += OnMove;
            input.actions["Move"].canceled += OnMoveCancelled;
            input.actions["Jump"].performed += OnJump;
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
        }
    }

    bool grounded = false;
    static RaycastHit2D[] results = new RaycastHit2D[16];
    void FixedUpdate() {
        body.velocity = new Vector3(move.x * moveSpeed, body.velocity.y, 0.0f);
        timeSinceFlip += Time.deltaTime;

        int count = Physics2D.RaycastNonAlloc(transform.position, Vector2.down, results, 0.65f);
        for (int i = 0; i < count; ++i) {
            if (results[i].collider.gameObject != gameObject) {
                grounded = true;
                return;
            }
        }
    }

    float timeSinceFlip = 0.0f;
    public void OnMove(InputAction.CallbackContext ctx) {
        move = ctx.ReadValue<Vector2>();
        if (timeSinceFlip > 0.25f) {
            if (move.x < 0) {
                model.localScale = new Vector3(-1, 1, 1);
            } else if (move.x > 0) {
                model.localScale = new Vector3(1, 1, 1);
            }
            timeSinceFlip = 0.0f;
        }
    }
    public void OnMoveCancelled(InputAction.CallbackContext ctx) {
        move = Vector2.zero;
    }

    public void OnJump(InputAction.CallbackContext ctx) {
        if (grounded) {
            body.velocity = new Vector3(body.velocity.x, jumpStrength, 0.0f);
            grounded = false;
        }
    }
}
