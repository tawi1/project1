using UnityEngine;

public class ObjectSpawnNode : MonoBehaviour
{
    [SerializeField]
    private int index = -1;
    [SerializeField]
    private bool weapon = false;

    public int Index
    {
        get
        {
            return index;
        }
    }

    public bool Weapon
    {
        get
        {
            return weapon;
        }
    }
}
