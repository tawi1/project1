using UnityEngine;
using System.Collections;

public class DistanceHitter : Photon.PunBehaviour
{
    private Collider2D currentNode;
    [SerializeField]
    private int currentIndex = 0;
    private DistanceManager distanceManager;

    private CarScript carscript;
    private CarLaps carLaps;
    private int viewId = -1;
    private bool isColliding = false;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (isColliding) return;
        isColliding = true;
        if (c.name == "DistanceNode")
        {
            if (currentNode != c)
            {
                currentNode = c;
                int i = c.GetComponent<DistanceNode>().Index;

                currentIndex = i;
                if (distanceManager != null)
                    if (carLaps.Laps != carLaps.MaxLaps)
                        if (viewId != -1)
                            distanceManager.SetIndex(viewId, i);
            }
        }
    }

    void Start()
    {
        StartCoroutine(WaitForPV());
    }

    private IEnumerator WaitForPV()
    {
        while (photonView == null)
        {
            yield return null;
        }
        viewId = photonView.viewID;
        carLaps = GetComponent<CarLaps>();
        carscript = GetComponent<CarScript>();
        distanceManager = FindObjectOfType<DistanceManager>();
    }

    void FixedUpdate()
    {
        isColliding = false;
    }

    public int Index { get { return currentIndex; } }
}

