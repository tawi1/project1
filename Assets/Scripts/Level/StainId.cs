using UnityEngine;

public class StainId : Photon.PunBehaviour
{
    private int ownerId = 0;

    void Start()
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

    public int OwnerId
    {
        get
        {
            return ownerId;
        }
        set
        {
            ownerId = 0;
        }
    }

    public void Launch(int id)
    {
        ownerId = id;
    }
}
