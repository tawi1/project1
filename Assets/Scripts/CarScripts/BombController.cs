using UnityEngine;

public class BombController : Photon.PunBehaviour
{
    private ObjectManager objectManager;
    [SerializeField]
    private int ownerId = 0;

    public void Init()
    {
        objectManager = FindObjectOfType<ObjectManager>();
    }

    public void DestroyBomb()
    {
        if (objectManager)
        {
            objectManager.CreateArea(AreaObject.AreaType.FireArea, false, transform.position, ownerId);
            photonView.RPC("RPCDestroyBomb", PhotonTargets.All, gameObject.GetPhotonView().viewID);
        }
    }

    public void Launch(int id)
    {
        ownerId = id;
    }

    [PunRPC]
    public void RPCDestroyBomb(int viewId)
    {
        objectManager.RemoveArea(viewId);
    }

    public int OwnerId
    {
        get
        {
            return ownerId;
        }
        set
        {
            ownerId = value;
        }
    }
}
