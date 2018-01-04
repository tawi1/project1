using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpecification
{
    public struct CarSpec
    {
        public int id;
        public int price;
    }

    private static Dictionary<int, CarSpec> cars = new Dictionary<int, CarSpec>();

    public static void AddCar(CarSpec car)
    {
        if (cars.ContainsKey(car.id) == false)
        {
            cars.Add(car.id, car);
        }
    }

    public static int GetPrice(int id)
    {
        return cars[id].price;
    }

    public static CarSpec GetCar(int id)
    {
        return cars[id];
    }

}
