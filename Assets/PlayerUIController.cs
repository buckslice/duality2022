using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerUIController : MonoBehaviour {
    // Start is called before the first frame update

    public GameObject[] playerPrefabs;
    int prefabIndex = 0;
    public GameObject curPrefab { get; private set; } = null;

    public PlayerInput input { get; private set; }
    public Transform spawn;

    TextMeshProUGUI txt;

    void Awake() {
        txt = GetComponent<TextMeshProUGUI>();
    }

    public void SetAction(PlayerInput input) {
        this.input = input;
        if (input) {
            input.actions["Submit"].performed += OnSubmit;
            input.actions["Cancel"].performed += OnCancel;
            input.actions["Navigate"].performed += OnNavigate;
        }
    }

    void OnEnable() {
        SetAction(input);
        if (!curPrefab) {
            SpawnPrefab();
            SpawnPrefab();
        }
    }

    void OnDisable() {
        if (input) {
            input.actions["Submit"].performed -= OnSubmit;
            input.actions["Cancel"].performed -= OnCancel;
            input.actions["Navigate"].performed -= OnNavigate;
        }
    }

    void SpawnPrefab() {
        if (curPrefab) {
            Destroy(curPrefab);
        }
        Vector3 v = Camera.main.ScreenToWorldPoint(spawn.position);
        v.z = 0.0f;
        curPrefab = Instantiate(playerPrefabs[prefabIndex], v, Quaternion.identity);
    }

    public bool locked { get; private set; } = false;
    public void OnSubmit(InputAction.CallbackContext ctx) {
        if (!locked) {
            locked = true;
            txt.text = $"{txt.text} LOCK";
        }
    }
    public void OnCancel(InputAction.CallbackContext ctx) {
        if (locked) {
            locked = false;
            txt.text = txt.text.Split(' ')[0];
        }
    }

    bool resetToZero = false;
    public void OnNavigate(InputAction.CallbackContext ctx) {
        if (locked) {
            return;
        }
        var nav = ctx.ReadValue<Vector2>();
        if (nav != Vector2.zero) {
            if (resetToZero) {
                bool shouldSpawn = false;
                if (nav.x > 0) {
                    prefabIndex++;
                    if (prefabIndex >= playerPrefabs.Length) {
                        prefabIndex = 0;
                    }
                    shouldSpawn = true;
                } else if (nav.x < 0) {
                    prefabIndex--;
                    if (prefabIndex < 0) {
                        prefabIndex = playerPrefabs.Length - 1;
                    }
                    shouldSpawn = true;
                }
                if (shouldSpawn) {
                    SpawnPrefab();
                }
                resetToZero = false;
            }
        } else {
            resetToZero = true;
        }
    }
}
