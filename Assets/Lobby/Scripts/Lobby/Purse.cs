using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purse : MonoBehaviour
{
    private static int credit = 0;
    private static List<int> purchasedCars = new List<int>();

    public static bool Purchase(int count)
    {
        if (credit - count >= 0)
        {
            credit -= count;
            SetUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void AddCredit(int count)
    {
        credit += count;
        SetUI();
    }

    public static void AddCar(int id)
    {
        if (purchasedCars.Contains(id) == false)
            purchasedCars.Add(id);
    }

    public static bool Purchased(int id)
    {
        if (purchasedCars.Contains(id))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static int GetCredit()
    {
        return credit;
    }

    public static List<int> GetCars()
    {
        return purchasedCars;
    }

    public static void SetPurse(int newCredit, List<int> newPurchasedCars)
    {
        credit = newCredit;

        SetUI();
        purchasedCars = newPurchasedCars;
    }

    private static void SetUI()
    {
        var purse = FindObjectOfType<PurseUI>();

        if (purse != null)
        {
            purse.SetValue(credit);
        }
    }
}
