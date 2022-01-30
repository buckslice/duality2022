using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour {
    private Vector3 targetPos;
    public float moveSpeed;

    List<Transform> followTargets = new List<Transform>();

    public void AddTarget(Transform t) {
        followTargets.Add(t);
    }

    void Update() {
        Vector3 p = Vector3.zero;
        for (int i = 0; i < followTargets.Count; ++i) {
            p += followTargets[i].position;
        }
        if (followTargets.Count > 0) {
            p /= followTargets.Count;
        }

        targetPos = new Vector3(p.x, p.y, transform.position.z);
        Vector3 velocity = targetPos - transform.position;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 1.0f, moveSpeed * Time.deltaTime);
    }
}
