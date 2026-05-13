using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    [SerializeField] private Transform holdPos;

    [SerializeField] private BaseItem equippedItem;


    public void EquipItem(BaseItem item)
    {
        if (equippedItem != null)
        {
            DropItem();
        }

        equippedItem = item;

        item.transform.parent = holdPos;
        item.transform.position = item.posOffset;
        item.transform.rotation = item.rotOffset;

        
    }

    public void UseItem()
    {
        if (equippedItem != null)
        equippedItem.Use();
    }

    public void DropItem()
    {
        equippedItem.transform.parent = null;
        equippedItem = null;
    }
}
