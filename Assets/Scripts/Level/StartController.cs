using UnityEngine;

public class StartController : MonoBehaviour
{
    [SerializeField]
    private bool reverse = false;

    void Awake()
    {
        CheckPoint[] cp = GetComponentsInChildren<CheckPoint>();

        for (int i = 0; i < cp.Length; i++)
        {
            cp[i].Index = i;
        }
    }

    public bool Reverse
    {
        get
        {
            return reverse;
        }
    }
}
