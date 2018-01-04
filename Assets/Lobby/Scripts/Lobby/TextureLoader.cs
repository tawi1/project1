using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TextureLoader : MonoBehaviour
{
    private static Dictionary<int, Texture2D> cars = new Dictionary<int, Texture2D>();
    private static List<Texture2D> maps = new List<Texture2D>();

    public static void Init()
    {
        LoadCars();
        LoadMaps();
    }

    private static void LoadCars()
    {
        if (cars.Keys.Count == 0)
        {
            List<Texture2D> carsList = new List<Texture2D>();
            Texture2D[] carsArr = Resources.LoadAll<Texture2D>("Cars/");

            carsList = carsArr.OrderBy(t => t.name.Length).ThenBy(t => t.name).ToList();

            Dictionary<int, Texture2D> carsDict = new Dictionary<int, Texture2D>();

            foreach (var car in carsList)
            {
                int id = int.Parse(car.name);
                carsDict.Add(id, car);
            }

            cars = carsDict;
        }
    }

    private static void LoadMaps()
    {
        if (maps.Count == 0)
        {
            Texture2D[] mapsArr = Resources.LoadAll<Texture2D>("MapIcons/");

            maps = mapsArr.OrderBy(t => t.name).ToList();
            /* foreach (var map in mapsArr)
             {
                 maps.Add(map);
             }*/
        }
    }

    public static Sprite GetSpriteCar(int id)
    {
        Sprite carSprite = Sprite.Create(cars[id], new Rect(0, 0, cars[id].width, cars[id].height), new Vector2(0.5f, 0.5f));
        return carSprite;
    }

    public static Texture2D GetTextureCar(int id)
    {
        return cars[id];
    }

    public static List<Texture2D> GetMaps()
    {
        return maps;
    }

    public static Sprite GetMapSprite(int index)
    {
        Sprite sprite = Sprite.Create(maps[index], new Rect(0, 0, maps[index].width, maps[index].height), new Vector2(0.5f, 0.5f));
        return sprite;
    }    

    public static Texture2D GetMap(int index)
    {
        return maps[index];
    }

    public static string GetMapName(int index)
    {
        return maps[index].name;
    }

    public static int Count()
    {
        return cars.Count;
    }

    public static int GetRandom()
    {
        return cars.Keys.ElementAt(Random.Range(0, cars.Count));
    }
}

