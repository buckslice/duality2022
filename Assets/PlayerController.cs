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

    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody2D>();
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

    void FixedUpdate() {
        body.velocity = new Vector3(move.x, 0, 0) * moveSpeed;
    }

    public void OnMove(InputAction.CallbackContext ctx) {
        move = ctx.ReadValue<Vector2>();
    }
    public void OnMoveCancelled(InputAction.CallbackContext ctx) {
        move = Vector2.zero;
    }

    public void OnJump(InputAction.CallbackContext ctx) {
        Debug.Log("hello");
        body.velocity = new Vector3(body.velocity.x, jumpStrength, 0.0f);
    }
}
