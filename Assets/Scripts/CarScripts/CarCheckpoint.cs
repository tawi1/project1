using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCheckpoint : MonoBehaviour
{
    [SerializeField]
    private CarScript carScript;
    private DistanceHitter distanceHitter;
    [SerializeField]
    private CarLaps carLaps;

    private Rigidbody2D rb;
    private List<int> distNodes = new List<int>();

    [SerializeField]
    private bool[] checkP;
    private CheckPoint[] cp;
    private Vector3[] track;
    private Transform carTransform;
    private float startLockTime = 3f;
    private float lockTime;
    private bool locked = false;
    private bool reverse=false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        carTransform = GetComponent<Transform>();
        distanceHitter = GetComponent<DistanceHitter>();

        lockTime = startLockTime;
        SetCps();
        AddTrack();
        SetNodes();
    }

    private void FixedUpdate()
    {
        if (locked)
        {
            lockTime -= Time.fixedDeltaTime;

            if (lockTime < 0)
            {
                locked = false;
                lockTime = startLockTime;
            }
        }
    }        

    private void SetCps()
    {
        GameObject start = GameObject.Find("Start");
        if (start != null)
        {
            reverse = start.GetComponent<StartController>().Reverse;
            cp = start.GetComponentsInChildren<CheckPoint>();
            checkP = new bool[cp.Length];
        }
    }

    private void AddTrack()
    {
        GameObject t = GameObject.Find("Track");
        LineRenderer lineRenderer = t.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            track = new Vector3[lineRenderer.positionCount];

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                track[i] = lineRenderer.GetPosition(i);
            }
        }
    }

    private void SetNodes()
    {
        GameObject t = GameObject.Find("Line3");
        if (t != null)
        {
            BoxCollider2D[] lineCol = t.GetComponentsInChildren<BoxCollider2D>();
            for (int i = 0; i < lineCol.Length; i++)
            {
                int currentIndex = -1;
                float minDist = 999999;
                float dist = 0;
                lineCol[i].gameObject.GetComponent<DistanceNode>().Index = i;
                Vector2 pos = lineCol[i].gameObject.transform.position;

                for (int j = 0; j < track.Length; j++)
                {
                    if (j - 1 > 0)
                    {
                        dist = Vector2.Distance(pos, track[j - 1]) + Vector2.Distance(pos, track[j]);
                    }
                    else
                        dist = Vector2.Distance(pos, track[track.Length - 1]) + Vector2.Distance(pos, track[j]);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        currentIndex = j;
                    }
                }
                distNodes.Add(currentIndex);
            }
        }
    }

    public void Respawn()
    {
        rb.velocity = new Vector2(0, 0);

        if (carScript.AI)
        {
            if (carScript.PathFollower != null)
            {
                carScript.PathFollower.Stuck = 0;
                carScript.PathFollower.DisableReset();
            }
            else
            {
                carScript.AddAiModule();
            }
        }

        int currentIndex = -1;
        if (track != null)
        {
            int startIndex = distanceHitter.Index;

            if (startIndex < 0)
                startIndex = distNodes.Count - 1;

            int endIndex = startIndex + 1;
            if (endIndex >= distNodes.Count)
                endIndex = 0;

            float minDist = 999999;
            float dist = 0;

            int count = distNodes[endIndex] - distNodes[startIndex];

            if (count < 0)
                count += track.Length;
            int currentCount = 0;
            int i = distNodes[startIndex];
            while (currentCount < count)
            {
                if (i >= track.Length)
                    i = 0;
                if (i - 1 > 0)
                {
                    dist = Vector2.Distance(carTransform.position, track[i - 1]) + Vector2.Distance(carTransform.position, track[i]);
                }
                else
                    dist = Vector2.Distance(carTransform.position, track[track.Length - 1]) + Vector2.Distance(carTransform.position, track[i]);

                if (dist < minDist)
                {
                    minDist = dist;
                    currentIndex = i;
                }
                currentCount++;
                i++;
            }

            if (currentIndex != -1)
            {
                carTransform.position = track[currentIndex];
                Vector2 dir = Vector2.zero;

                if (currentIndex - 1 > 0)
                    dir = track[currentIndex] - track[currentIndex - 1];
                else
                    dir = track[currentIndex] - track[track.Length - 1];

                int side = 1;
                if (reverse)
                    side = -1;

                carTransform.localRotation = Quaternion.FromToRotation(Vector2.right, side * dir);

                if (checkP.Length > 2)
                {
                    int index = GetLastCP();
                    if (index > 0)
                    {
                        Vector2 dirCp = cp[index].gameObject.transform.position - carTransform.position;

                        if (dirCp.magnitude > 20f)
                        {
                            float dot = Vector2.Dot(carScript.GetCentralAxis(), dirCp);

                            if (dot < 0)
                            {
                                side = side * -1;
                            }
                            carTransform.localRotation = Quaternion.FromToRotation(Vector2.right, side * dir);
                        }
                    }
                }
            }
        }
    }

    public void OnCheckpointEnter(int index)
    {
        int currentIndex = index;
        bool match = false;

        if (checkP != null)
        {
            if (currentIndex == 0)
            {
                match = true;
            }
            else
            {
                for (int i = 0; i < currentIndex; i++)
                {
                    if (checkP[i] == false)
                    {
                        match = false;
                        break;
                    }
                    else
                        match = true;
                }
            }

            if (match == true)
            {
                checkP[currentIndex] = true;
                if (carScript.AI)
                {
                    if (carScript.PathFollower != null)
                    {
                        carScript.PathFollower.ResetCpTarget();
                    }
                }
            }
        }
    }

    public void OnFinishEnter()
    {
        if (GetAllCP())
        {
            if (locked == false)
            {
                locked = true;
                carLaps.LapsCounter();
            }
            ResetAllCP();
        }

        if (cp.Length < 6)
        {
            ResetAllCP();
        }
    }

    public bool GetCP(int index)
    {
        if (checkP != null)
            return checkP[index];
        else
            return false;
    }

    public void SetCP2(int index, bool value)
    {
        checkP[index] = value;
    }

    public bool GetAllCP()
    {
        bool match = false;

        if (checkP != null)
            for (int i = 0; i < checkP.Length; i++)
            {
                if (checkP[i] == false)
                {
                    match = false;
                    break;
                }
                else
                    match = true;
            }
        return match;
    }

    public void ResetAllCP()
    {
        if (checkP != null)
            for (int i = 0; i < checkP.Length; i++)
            {
                checkP[i] = false;
            }
        if (carScript.AI)
        {
            if (carScript.PathFollower != null)
            {
                carScript.PathFollower.ResetCpTarget();
            }
        }
    }

    public int GetLastCP()
    {
        int j = -1;

        if (checkP != null)
            for (int i = 0; i < checkP.Length; i++)
            {
                if (checkP[i] == false)
                {
                    j = i;
                    break;
                }
            }
        return j;
    }

    public int CheckPCount
    {
        get
        {
            return checkP.Length;
        }
    }

    public CheckPoint[] GetCp()
    {
        return cp;
    }

    public List<int> GetDistNodes()
    {
        return distNodes;
    }

    public Vector3[] GetTrack()
    {
        return track;
    }

}
