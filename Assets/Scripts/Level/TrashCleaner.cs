using UnityEngine;

public class TrashCleaner : MonoBehaviour
{
    private int limit = 50;
    private int clearCount = 5;
    private int trashCount = 0;

    void Update()
    {
        if (trashCount > limit)
        {
            Transform[] g = gameObject.GetComponentsInChildren<Transform>();
            if (g != null)
            {
                if (g.Length - 1 > limit)
                {
                    for (int i = 1; i <= g.Length - limit + clearCount; i++)
                    {
                        Destroy(g[i].gameObject);
                    }
                }
                trashCount -= clearCount;
            }
        }
    }

    public void IncTrash(int count)
    {
        trashCount += count;
    }

    public void ClearTrack()
    {
        Transform[] g = gameObject.GetComponentsInChildren<Transform>();
        if (g != null)
        {
            for (int i = 1; i < g.Length; i++)
            {
                Destroy(g[i].gameObject);
            }
        }
    }
}
