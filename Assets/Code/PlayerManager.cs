using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerManager : MonoBehaviour {
    public PlayerUIController p1;
    public PlayerUIController p2;
    public Button playButton;

    public PlayerController controllerPrefab;

    // Start is called before the first frame update
    void Awake() {
        p1.gameObject.SetActive(false);
        p2.gameObject.SetActive(false);
        playButton.interactable = false;
    }

    public void OnPlayerJoined(PlayerInput pi) {

        StartCoroutine(JoinNextFrame(pi));
    }

    IEnumerator JoinNextFrame(PlayerInput pi) {
        yield return null;

        if (!p1.gameObject.activeSelf) {
            p1.gameObject.SetActive(true);
            p1.SetAction(pi);
        } else {
            p2.gameObject.SetActive(true);
            p2.SetAction(pi);
            //playButton.interactable = true;
        }
    }

    void Update() {
        if (playButton) {
            playButton.interactable = p1.locked && p2.locked;
        }
    }

    public void OnPlay() {
        GameObject.Find("Canvas").SetActive(false);

        ConvertToPlay(p1);
        ConvertToPlay(p2);

        Destroy(p1);
        Destroy(p2);

        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    void ConvertToPlay(PlayerUIController uiplayer) {
        uiplayer.input.SwitchCurrentActionMap("Player");

        // new controller
        var cont = Instantiate(controllerPrefab);
        cont.transform.position = uiplayer.curPrefab.transform.position;
        uiplayer.curPrefab.transform.SetParent(cont.transform, false);
        uiplayer.input.transform.SetParent(cont.transform, false);
        var pc = cont.GetComponent<PlayerController>();
        pc.SetInput(uiplayer.input);

        Camera.main.GetComponent<CameraFollow>().AddTarget(cont.transform);

        // make sure these zerod
        uiplayer.input.transform.localPosition = Vector3.zero;
        uiplayer.curPrefab.transform.localPosition = Vector3.zero;
    }

}
