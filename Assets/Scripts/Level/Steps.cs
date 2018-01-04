using UnityEngine;

public class Steps : MonoBehaviour
{
    private int limit = 50;
    private int clearCount = 10;
    private int stepsCount = 0;

    void Update()
    {
        if (stepsCount > limit)
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
                stepsCount -= clearCount;
            }
        }
    }

    public void IncSteps()
    {
        stepsCount += 2;
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
