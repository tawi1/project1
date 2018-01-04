using UnityEngine;

public class CameraController : MonoBehaviour
{
    int distance = -1;
    Transform g;

    public void setTarget(Transform car)
    {
        g = car;
    }

    private void FixedUpdate()
    {
        if (g != null)
        {
            transform.position = new Vector3(0f, 0f, distance) + g.position;
            transform.LookAt(g.position);
        }
        else
        {
            int i = 0;
            CarScript[] rb = GameObject.FindObjectsOfType<CarScript>();
            if (rb != null)
            {
                if (rb.Length > 0)
                {
                    for (i = 0; i < rb.Length; i++)
                    {
                        if (rb[i].gameObject.GetComponent<PhotonView>().isMine == true)
                        {
                            if (rb[i].gameObject.GetComponent<CarScript>().AI == false)
                                break;
                        }
                    }

                    if (rb.Length > i)
                        setTarget(rb[i].gameObject.transform);
                }
            }
        }
    }
}
