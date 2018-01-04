using UnityEngine;

public class AreaObject : MonoBehaviour
{
    public enum AreaType
    {
        IceArea,
        FireArea,
        Bomb,
        Stain
    }

    private float startVolume = 0;
    [SerializeField]
    private AreaType type;
    private AudioSource audioSource;
    private ObjectManager objectManager;
    private bool spawned = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            startVolume = audioSource.volume;
            audioSource.volume = 0;
        }

        objectManager = FindObjectOfType<ObjectManager>();
        if (GetComponent<ExplosionController>() != null)
        {
            GetComponent<ExplosionController>().Init();
            type = AreaType.FireArea;
        }
        else if (GetComponent<IceAreaController>() != null)
        {
            GetComponent<IceAreaController>().Init();
            type = AreaType.IceArea;
        }
        else if (GetComponent<BombController>() != null)
        {
            GetComponent<BombController>().Init();
            type = AreaType.Bomb;
        }
        else if (GetComponent<StainId>() != null)
        {
            type = AreaType.Stain;
        }

        objectManager.AddArea(this);
        transform.parent = objectManager.GetAreaObjectPool();
        gameObject.SetActive(false);
    }

    public void Launch(int id)
    {
        gameObject.SetActive(true);
        if (audioSource != null)
            audioSource.volume = startVolume;

        if (type == AreaType.FireArea)
        {
            GetComponent<ExplosionController>().Launch(id);
        }
        else if (type == AreaType.IceArea)
        {
            GetComponent<IceAreaController>().Launch(id);
        }
        else if (type == AreaType.Bomb)
        {
            GetComponent<BombController>().Launch(id);
        }
        else if (type == AreaType.Stain)
        {
            GetComponent<StainId>().Launch(id);
        }
    }

    public AreaType Type
    {
        get
        {
            return type;
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
