using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private int index = 0;

    public int Index
    {
        get
        {
            return index;
        }
        set
        {
            index = value;
        }
    }
}
