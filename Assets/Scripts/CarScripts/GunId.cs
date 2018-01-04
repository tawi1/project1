using UnityEngine;

public class GunId : MonoBehaviour
{
    private int id = -1;

    public int Id
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }
}
