using UnityEngine;
using System.Collections.Generic;

public class Atlas : Photon.PunBehaviour
{
    [SerializeField]
    private MatchController matchController;
    [SerializeField]
    private Texture2D ice;
    Dictionary<int, Sprite> frozenCars = new Dictionary<int, Sprite>();
    Dictionary<int, Sprite> spriteCars = new Dictionary<int, Sprite>();
    Dictionary<int, GameObject[]> brokenCars = new Dictionary<int, GameObject[]>();
    Dictionary<int, GameObject[]> frozenBrokenCars = new Dictionary<int, GameObject[]>();

    [SerializeField]
    private int destroyedCarsCount = 4;

    public void PaintCar(int idCar, int idColor, int carIndex)
    {
        photonView.RPC("RPCPaintCar", PhotonTargets.All, idCar, idColor, carIndex);
    }

    [PunRPC]
    private void RPCPaintCar(int idCar, int idColor, int carIndex)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        Color32[] colors = TextureLoader.GetTextureCar(carIndex).GetPixels32();

        Color32 targetColor = new Color32(230, 230, 230, 5);

        Color32 targetColor2 = new Color32(168, 168, 168, 5);

        float darker = 0.20f;

        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].r >= targetColor.r && colors[i].g >= targetColor.g && colors[i].b >= targetColor.b && colors[i].a >= targetColor.a)
            {
                colors[i] = CarColors.Colors[idColor];
            }
            else if (colors[i].r >= targetColor2.r - 25 && colors[i].r <= targetColor2.r + 25
                && colors[i].g >= targetColor2.g - 25 && colors[i].g <= targetColor2.g + 25
                && colors[i].b >= targetColor2.b - 25 && colors[i].b <= targetColor2.b + 25
                && colors[i].a >= targetColor2.a)
            {
                Color32 currentColor = CarColors.Colors[idColor];
                byte r = (byte)(currentColor.r - (currentColor.r * darker));
                if (r < 0)
                    r = 0;
                byte g = (byte)(currentColor.g - (currentColor.g * darker));
                if (g < 0)
                    g = 0;
                byte b = (byte)(currentColor.b - (currentColor.b * darker));
                if (b < 0)
                    b = 0;
                colors[i].r = r;
                colors[i].g = g;
                colors[i].b = b;
            }
        }

        Texture2D newTex = new Texture2D(TextureLoader.GetTextureCar(carIndex).width, TextureLoader.GetTextureCar(carIndex).height, TextureFormat.ARGB32, false);

        newTex.SetPixels32(colors);
        newTex.Apply();

        Sprite carSprite = Sprite.Create(newTex, new Rect(0, 0, TextureLoader.GetTextureCar(carIndex).width, TextureLoader.GetTextureCar(carIndex).height), new Vector2(0.5f, 0.5f));

        Texture2D frozenTex = ImageHelpers.AlphaBlend(newTex, ice);
        Sprite frozenSprite = Sprite.Create(frozenTex, new Rect(0, 0, frozenTex.width, frozenTex.height), new Vector2(0.5f, 0.5f));
        frozenCars.Add(idCar, frozenSprite);

        car.GetComponent<CarSkin>().SetModel(carSprite, TextureLoader.GetTextureCar(carIndex).width, TextureLoader.GetTextureCar(carIndex).height);
        GameObject destroyedCars = GameObject.Find("DestroyedCars");
        if (destroyedCars == null)
        {
            destroyedCars = new GameObject();
            destroyedCars.name = "DestroyedCars";
        }

        Transform t = destroyedCars.transform;

        GameObject newFrozenBrokenCar = CreateCar(car, frozenSprite, t);
        CreateDestroyedCar(newFrozenBrokenCar, idCar, destroyedCarsCount, false, t);

        spriteCars.Add(idCar, carSprite);

        GenerateAtlas();
    }

    private void GenerateAtlas()
    {
        if (spriteCars.Keys.Count >= matchController.PlayersCount())
        {
            Texture2D[] atlasTextures = new Texture2D[spriteCars.Keys.Count];

            int i = 0;
            foreach (KeyValuePair<int, Sprite> entry in spriteCars)
            {
                Sprite temp = entry.Value;
                atlasTextures[i] = temp.texture;
                i++;
            }

            Texture2D atlas = new Texture2D(2048, 2048);
            Rect[] rects = atlas.PackTextures(atlasTextures, 2, 2048);

            Dictionary<int, Sprite> tempDict = new Dictionary<int, Sprite>();
            i = 0;

            GameObject destroyedCars = GameObject.Find("DestroyedCars");

            foreach (KeyValuePair<int, Sprite> entry in spriteCars)
            {
                Rect newRect = rects[i];

                newRect.x = Mathf.FloorToInt(newRect.x * atlas.width);
                newRect.y = Mathf.FloorToInt(newRect.y * atlas.height);
                newRect.height = Mathf.FloorToInt(newRect.height * atlas.height);
                newRect.width = Mathf.FloorToInt(newRect.width * atlas.width);

                Sprite newSprite = Sprite.Create(atlas, newRect, new Vector2(0.5f, 0.5f));

                PhotonView carView = PhotonView.Find(entry.Key);
                if (carView != null)
                {
                    GameObject car = carView.gameObject;
                    car.GetComponent<CarSkin>().SetModel(newSprite, atlasTextures[i].width, atlasTextures[i].height);
                    tempDict.Add(entry.Key, newSprite);

                    Transform t = destroyedCars.transform;

                    GameObject newBrokenCar = CreateCar(car, newSprite, t);
                    CreateDestroyedCar(newBrokenCar, entry.Key, destroyedCarsCount, true, t);
                }

                i++;
            }

            spriteCars = tempDict;
        }
    }

    private GameObject CreateCar(GameObject car, Sprite carSprite, Transform cars)
    {
        GameObject g = new GameObject();

        SpriteRenderer spriteRenderer = g.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 3;
        spriteRenderer.sprite = carSprite;

        Transform model = car.GetComponent<CarSkin>().GetTransformModel();

        g.transform.localScale = new Vector2(model.localScale.x * car.transform.localScale.x, model.localScale.y * car.transform.localScale.y);
        g.name = "DestroyedCar";
        g.AddComponent<Rigidbody2D>();
        g.GetComponent<Rigidbody2D>().gravityScale = 0;

        g.AddComponent<BoxCollider2D>();
        Explodable explodable = g.AddComponent<Explodable>();
        explodable.allowRuntimeFragmentation = true;
        explodable.extraPoints = 5;
        explodable.orderInLayer = 1;
        g.transform.parent = cars;

        return g;
    }

    private void CreateDestroyedCar(GameObject car, int idCar, int count, bool broken, Transform cars)
    {
        GameObject[] objects = new GameObject[count + 1];

        for (int i = 0; i < objects.Length - 1; i++)
        {
            GameObject g = Instantiate(car);
            objects[i] = g;

            Explodable explodable = g.GetComponent<Explodable>();

            explodable.GenerateFragments();
            g.transform.parent = cars;
            g.SetActive(false);
        }

        car.GetComponent<Explodable>().GenerateFragments();
        objects[objects.Length - 1] = car;
        car.SetActive(false);

        if (broken)
            brokenCars.Add(idCar, objects);
        else
            frozenBrokenCars.Add(idCar, objects);
    }

    public Sprite GetFrozenCar(int id)
    {
        return frozenCars[id];
    }

    public Sprite GetSpriteCar(int id)
    {
        return spriteCars[id];
    }

    public GameObject GetBrokenCar(int id)
    {
        GameObject[] temp = brokenCars[id];
        GameObject tempCar = temp[Random.Range(0, temp.Length)];
        return tempCar;
    }

    public GameObject GetFrozenBrokenCar(int id)
    {
        GameObject[] temp = frozenBrokenCars[id];
        GameObject tempCar = temp[Random.Range(0, temp.Length)];
        return tempCar;
    }
}