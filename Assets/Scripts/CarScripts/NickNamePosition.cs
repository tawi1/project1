using UnityEngine;

public class NickNamePosition : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshRenderer>().sortingOrder = 5;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
