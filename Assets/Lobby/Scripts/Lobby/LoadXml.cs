using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

[ExecuteInEditMode]
[Serializable]
public static class RyXmlTools
{
    private static XmlDocument loadXml()
    {
        XmlDocument xmlDoc = new XmlDocument();

        var str = Resources.Load("CarsXml").ToString();

        try
        {
            xmlDoc.LoadXml(str);
        }
        catch (Exception ex)
        {
            Debug.Log("Error loading " + ":\n" + ex.Message);
        }

        return xmlDoc;
    }

    public static List<int> LoadFromXml()
    {
        List<int> list = new List<int>();
        var doc = RyXmlTools.loadXml();

        XmlNode xNode = doc.ChildNodes.Item(1).ChildNodes.Item(0);

        foreach (XmlNode node in xNode.ChildNodes)
        {
            if (node.LocalName == "TextKey")
            {
                string idStr = node.Attributes.GetNamedItem("name").Value;
                string priceStr = string.Empty;
                foreach (XmlNode specNode in node)
                {
                    if (specNode.LocalName == "price")
                    {
                        priceStr = specNode.InnerText;
                    }
                }

                int id = int.Parse(idStr);
                int price = int.Parse(priceStr);

                CarSpecification.CarSpec car;

                car.id = id;
                car.price = price;

                CarSpecification.AddCar(car);
                list.Add(id);

                if (price == 0)
                {
                    Purse.AddCar(id);
                }
            }
        }

        return list;
    }

}