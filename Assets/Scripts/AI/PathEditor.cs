using System.Collections.Generic;
using UnityEngine;

public class PathEditor : MonoBehaviour
{
    List<Transform> path = new List<Transform>();
    Transform[] arr;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        path.Clear();

        arr = GetComponentsInChildren<Transform>();
        foreach (Transform obj in arr)
        {
            if (obj != this.transform)
            {
                path.Add(obj);
            }
        }

        for (int i = 0; i < path.Count; i++)
        {
            Vector2 pos = path[i].position;
            if (i > 0)
            {
                Vector2 prev = path[i - 1].position;
                Gizmos.DrawLine(prev, pos);
                Gizmos.DrawWireSphere(pos, 0.3f);
            }
        }

        Gizmos.DrawLine(path[path.Count - 1].position, path[0].position);
        Gizmos.DrawWireSphere(path[0].position, 0.3f);
    }
}
