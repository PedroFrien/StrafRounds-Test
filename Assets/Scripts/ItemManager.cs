using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    [SerializeField] private Transform holdPos;

    private Rigidbody itemRb;



    public BaseItem equippedItem;


    public void EquipItem(BaseItem item)
    {
        if (equippedItem != null)
        {
            DropItem();
        }

        equippedItem = item;
        itemRb = equippedItem.GetComponent<Rigidbody>();
        itemRb.isKinematic = true;
        



        
    }

    private void LateUpdate()
    {
        Debug.Log("Player" + OwnerClientId + " has an ItemManager script.");


        if (holdPos != null && equippedItem != null)
        {
            equippedItem.transform.position = holdPos.position + equippedItem.posOffset;
            equippedItem.transform.rotation = holdPos.rotation * equippedItem.rotOffset;
        }
    }

    public void UseItem()
    {
        if (equippedItem != null)
        equippedItem.Use();
    }

    public void DropItem()
    {
        itemRb.isKinematic = false;
        itemRb = null;
        equippedItem = null;
        
    }
}
