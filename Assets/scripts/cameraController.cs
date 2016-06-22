using UnityEngine;
using System.Collections;

public class cameraController : MonoBehaviour
{

    public Transform target;
    int distance = -10;
    float lift = 1.5f;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(0f, lift, distance) + target.position;
        transform.LookAt(target);
    }
}
