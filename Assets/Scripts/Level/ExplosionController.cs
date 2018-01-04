using UnityEngine;
using System.Collections;

public class ExplosionController : Photon.PunBehaviour
{
    [SerializeField]
    private Transform t;
    [SerializeField]
    private Transform explode;
    [SerializeField]
    private AudioSource audioSource;
    private bool EnableExplosion = false;
    private float limit = 1f;
    private int ownerId = 0;
    [SerializeField]
    private CircleCollider2D collider;
    AudioListener audioListener;
    [SerializeField]
    private Animator animator;
    ObjectManager objectManager;
    private float expLength = 1f;

    private bool rocketExplode = true;

    public void Init()
    {
        if (objectManager == null)
        {
            audioListener = FindObjectOfType<AudioListener>();
            objectManager = FindObjectOfType<ObjectManager>();

            var data = photonView.instantiationData;
            if (data != null)
                if ((float)data[0] != 0)
                {
                    rocketExplode = false;
                    limit = (float)data[0];
                    SetStartSize((float)data[1]);
                }

            expLength = gameObject.GetComponent<AudioSource>().clip.length;

            if (data.Length > 2)
                if ((bool)data[2] == true)
                {
                    DistanceEqualizer[] equalizers = GetComponentsInChildren<DistanceEqualizer>();
                    for (int i = 0; i < equalizers.Length; i++)
                    {
                        Destroy(equalizers[i]);
                    }

                    AudioSource[] sources = GetComponentsInChildren<AudioSource>();
                    for (int i = 0; i < sources.Length; i++)
                    {
                        Destroy(sources[i]);
                    }
                }
        }
    }

    void Update()
    {
        if (EnableExplosion == false)
        {
            if (t.localScale.x > 0.8f && t.localScale.x != 1f)
            {
                EnableExplosion = true;
            }
        }
        else if (t.localScale.x < limit)
        {
            if (collider.enabled)
                collider.enabled = false;
        }
    }

    public void Launch(int id)
    {
        ownerId = id;
        animator.gameObject.SetActive(true);
        animator.Play(0, -1, 0);
        float distance = Vector3.Distance(transform.position, audioListener.transform.position);
        if (distance > 40)
        {
            if (audioSource != null)
                audioSource.volume = 0;
        }
        StartCoroutine(Remove(expLength));
    }

    private IEnumerator Remove(float time)
    {
        yield return new WaitForSeconds(time);
        objectManager.RemoveArea(gameObject.GetPhotonView().viewID);
        yield return null;
    }

    public float Limit
    {
        set
        {
            limit = value;
        }
    }

    public void SetStartSize(float multi)
    {
        explode.localScale = explode.localScale * multi;
    }

    public void Reset()
    {
        animator.gameObject.SetActive(false);
        EnableExplosion = false;
        Transform[] t = animator.gameObject.GetComponentsInChildren<Transform>();
        collider.enabled = true;
        ownerId = 0;
        for (int i = 1; i < t.Length; i++)
        {
            t[i].localScale = Vector3.one;
        }
    }

    public int OwnerId
    {
        get
        {
            return ownerId;
        }

    }

    public bool RocketExplode
    {
        get
        {
            return rocketExplode;
        }
    }
}
