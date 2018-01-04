using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Explodable))]
public abstract class ExplodableAddon : MonoBehaviour {
    protected Explodable explodable;
	// Use this for initialization
	void Start () {
        explodable = GetComponent<Explodable>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
	}

    public abstract void OnFragmentsGenerated(List<GameObject> fragments);
}
