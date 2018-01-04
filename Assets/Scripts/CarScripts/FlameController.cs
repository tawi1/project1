using UnityEngine;

public class FlameController : Photon.PunBehaviour
{
    private int localId = -1;
    [SerializeField]
    private PhotonView pV;
    private bool isColliding = false;

    private void OnParticleCollision(GameObject obj)
    {
        if (isColliding) return;
        isColliding = true;
        if (pV.isMine)
        {
            if (obj.tag == "Player")
            {
                if ((!obj.GetPhotonView().isMine) || (obj.GetPhotonView().isMine && localId != obj.GetComponent<CarScript>().LocalId))
                {
                    obj.GetComponent<Health>().Destroyed(pV.ownerId);
                }
            }
        }
        else
        {
            if (obj.tag == "Player")
            {
                if (obj.GetPhotonView().isMine)
                {
                    obj.GetComponent<Health>().Destroyed(pV.ownerId);
                }
            }
        }
    }

    void FixedUpdate()
    {
        isColliding = false;
    }

    public int LocalId
    {
        set
        {
            localId = value;
        }
    }
}
