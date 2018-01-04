using UnityEngine;

public class PathNode : MonoBehaviour
{
    [SerializeField]
    private bool nitro = false;

    public bool Nitro
    {
        get
        {
            return nitro;
        }
        set
        {
            nitro = value;
        }
    }
}
