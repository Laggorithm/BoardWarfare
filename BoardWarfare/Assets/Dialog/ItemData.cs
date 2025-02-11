using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public List<Item> itemList;
}

[System.Serializable]
public class Item
{
    public string name;
    public string description;
}
