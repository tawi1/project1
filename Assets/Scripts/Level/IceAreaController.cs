using UnityEngine;
using System.Collections;

public class IceAreaController : Photon.PunBehaviour
{
    private Transform t;
    private Vector2 startLocalScale;
    private float limitScale = 0.5f;
    private float incTime = 0.15f;
    private float inc = 0f;

    private float colorLimit = 100f;
    private float hideTime = 0.3f;
    private float hideInc = 0f;
    private bool hide = false;
    [SerializeField]
    private SpriteRenderer sprite;
    private int ownerId = 0;
    private bool launched = false;
    private ObjectManager objectManager;

    public void Init()
    {
        objectManager = FindObjectOfType<ObjectManager>();
        t = GetComponent<Transform>();

        startLocalScale = t.localScale;
        inc = (limitScale - startLocalScale.x) / (incTime * (1 / Time.fixedDeltaTime));
        hideInc = (255 - colorLimit) / (hideTime * (1 / Time.fixedDeltaTime));

        DeleteSounds();
    }


    private void DeleteSounds()
    {
        var data = photonView.instantiationData;

        if (data != null)
            if (data.Length > 0)
            if ((bool)data[0] == true)
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

    void FixedUpdate()
    {
        if (launched)
        {
            if (photonView.isMine && hide == false)
            {
                if (t.localScale.x < limitScale)
                {
                    Vector2 newScale = new Vector2(t.localScale.x + inc, t.localScale.y + inc);
                    t.localScale = newScale;
                }
                else
                {
                    photonView.RPC("SetHide", PhotonTargets.All);
                }
            }

            if (hide)
            {
                Color32 color = sprite.color;

                if (color.a > colorLimit)
                {
                    float newColor = color.a - hideInc;
                    Color32 tmp = color;
                    tmp.a = (byte)(color.a - hideInc);
                    color = tmp;
                    sprite.color = color;
                }
                else
                {
                    if (photonView.isMine)
                    {
                        PhotonNetwork.Destroy(gameObject);
                    }
                }
            }
        }
    }

    private IEnumerator Remove(float time)
    {
        yield return new WaitForSeconds(time);
        objectManager.RemoveArea(gameObject.GetPhotonView().viewID);
        yield return null;
    }

    public void Launch(int id)
    {
        ownerId = id;
        launched = true;
        StartCoroutine(Remove(hideTime + incTime));
    }

    [PunRPC]
    public void SetHide()
    {
        hide = true;
    }

    public int OwnerId
    {
        get
        {
            return ownerId;
        }
    }

    public void Reset()
    {
        ownerId = 0;
        launched = false;
        hide = false;
        t.localScale = startLocalScale;

        Color32 tmp = sprite.color;
        tmp.a = 255;
        sprite.color = tmp;
    }
}
