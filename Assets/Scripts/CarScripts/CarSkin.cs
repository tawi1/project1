using UnityEngine;

public class CarSkin : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer model;
    [SerializeField]
    private DistanceHitter distanceHitter;
    [SerializeField]
    private CarScript carScript;
    private Rigidbody2D rb;
    private Vector2 size = new Vector2(800, 397);
    private Atlas atlas;
    private Transform carTransform;
    private int sortCar;
    private TrashCleaner trashCleaner;

    private void Start()
    {
        carTransform = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        atlas = FindObjectOfType<Atlas>();
        trashCleaner = FindObjectOfType<TrashCleaner>();
        sortCar = model.sortingOrder;
    }

    public void SetModel(Sprite sprite, int width, int height)
    {
        model.sprite = sprite;
        model.gameObject.transform.localScale = new Vector2((size.x / width), (size.y / height));
    }

    public void EnableTransparent()
    {
        Color32 tmp = model.color;
        tmp.a = 100;
        model.color = tmp;
    }

    public void DisableTransparent()
    {
        Color32 tmp = model.color;
        tmp.a = 255;
        model.color = tmp;
    }

    public void ShowCar()
    {
        rb.freezeRotation = false;
        model.sortingOrder = sortCar;
        carScript.EnableRpm();

        MeshRenderer nickName = gameObject.GetComponentInChildren<MeshRenderer>();

        if (nickName != null)
            nickName.sortingOrder = sortCar + 1;

        gameObject.GetComponent<Collider2D>().isTrigger = false;
        distanceHitter.GetComponent<Collider2D>().isTrigger = false;
    }

    public void HideCar()
    {
        int sort = -1;

        if (model.sortingOrder > 0)
        {
            gameObject.GetComponent<Collider2D>().isTrigger = true;
            distanceHitter.GetComponent<Collider2D>().isTrigger = true;

            MeshRenderer nickName = gameObject.GetComponentInChildren<MeshRenderer>();

            if (nickName != null)
                nickName.sortingOrder = sort;

            model.sortingOrder = sort;

            Vector2 vel = rb.velocity;
            if (vel.magnitude > carScript.MaxSpeed)
            {
                vel = vel.normalized * (carScript.MaxSpeed * 2 / 3);
            }

            DigitalRuby.Threading.EZThread.ExecuteInBackground(CreateDestroyedCar(gameObject.GetPhotonView().viewID, carScript.Fragile, carTransform.position, carTransform.rotation, vel));

            rb.velocity = Vector2.zero;
            rb.freezeRotation = true;

            carScript.DisableButtons();
          }
    }

    private System.Action CreateDestroyedCar(int id, bool frozen, Vector2 pos, Quaternion rot, Vector2 vel)
    {
        GameObject prefab = atlas.GetBrokenCar(id);

        if (frozen)
            prefab = atlas.GetFrozenBrokenCar(id);

        GameObject g = Instantiate(prefab);
        g.SetActive(true);
        g.transform.position = pos;
        g.transform.rotation = rot;
        g.GetComponent<Rigidbody2D>().velocity = vel;
        g.GetComponent<Explodable>().explode(trashCleaner);
        return null;
    }

    public bool CarHided()
    {
        if (model.sortingOrder != -1)
            return false;
        else
            return true;
    }

    public void SetModel(Sprite sprite)
    {
        model.sprite = sprite;
    }

    public Sprite GetModel()
    {
        return model.sprite;
    }

    public SpriteRenderer GetModelRender()
    {
        return model.GetComponent<SpriteRenderer>();
    }

    public Transform GetTransformModel()
    {
        return model.transform;
    }

    public void SetModel(Material sprite)
    {
        model.material = sprite;
    }
}
