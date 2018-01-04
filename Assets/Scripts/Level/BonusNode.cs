using UnityEngine;

public class BonusNode : MonoBehaviour
{

    [SerializeField]
    private int bonusIndex = 0;
    private bool spawned = false;

    void Start()
    {
        ObjectManager objectManager = FindObjectOfType<ObjectManager>();
        transform.parent = objectManager.GetObjectPool();
        objectManager.AddBonus(this);
        if (spawned == false)
            gameObject.SetActive(false);

    }

    public int Index
    {
        get
        {
            return bonusIndex;
        }
    }

    public bool Spawned
    {
        get
        {
            return spawned;
        }
        set
        {
            spawned = value;
        }
    }
}
