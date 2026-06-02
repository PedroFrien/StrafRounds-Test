using Unity.Netcode;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
    [SerializeField] private Transform holdPos;
    [SerializeField] private NetworkObject player;

    private Rigidbody itemRb;



    public BaseItem equippedItem;


    

    [Rpc(SendTo.Server)]
    public void EquipItemServerRpc(ulong networkObjectId)
    {
        Debug.Log("EquipItemServerRpc called");
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target)) return;
        if (!target.TryGetComponent(out BaseItem item)) return;
        if (!item.m_isAvailable.Value) return;
        Debug.Log("EquipItemServerRpc running");

        target.ChangeOwnership(OwnerClientId);

        item.m_isAvailable.Value = false;

        target.TrySetParent(player);

        EquipItemClientRpc(target.NetworkObjectId);

        
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EquipItemClientRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target)) return;
        if (!target.TryGetComponent(out BaseItem item)) return;

        // ALL clients suppress physics so NetworkTransform can sync cleanly
        var rb = target.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Only the owner does local attach logic
        if (!IsOwner) return;
        EquipItem(item);
    }

    public void EquipItem(BaseItem item)
    {
        if (equippedItem != null)
        {
            DropItem();
        }

        equippedItem = item;
        itemRb = equippedItem.GetComponent<Rigidbody>();
        itemRb.isKinematic = true;
        equippedItem.transform.position = holdPos.position + equippedItem.posOffset;
        equippedItem.transform.rotation = holdPos.rotation * equippedItem.rotOffset;
    }

    [Rpc(SendTo.Server)]
    private void DropItemServerRPC(ulong networkObjectId)
    {
        Debug.Log("DropItemServerRPC called");
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target)) return;
        Debug.Log("DropItemServerRPC running");


        target.RemoveOwnership();
        target.TryRemoveParent();

        if (target.TryGetComponent(out BaseItem item))
        {
            item.m_isAvailable.Value = true;
        }

        

        DropItemClientRPC(networkObjectId);


    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DropItemClientRPC(ulong networkObjectId)
    {
        // ALL clients re-enable physics on drop
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target))
        {
            var rb = target.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
        }

        if (!IsOwner) return;
        itemRb = null;
        equippedItem = null;
    }

    public void DropItem()
    {
        if (!IsOwner) return;
        if (itemRb != null)
        {
            itemRb.isKinematic = false;
        }

        itemRb = null;
        equippedItem = null;

    }

    public void TryDrop()
    {

        if (equippedItem != null && IsOwner)
        {
            DropItemServerRPC(equippedItem.NetworkObjectId);
        }
        
    }

    private void LateUpdate()
    {
        Debug.Log("Player" + OwnerClientId + " has an ItemManager script.");

        if (!IsOwner) return;
       
            
        


        //if (holdPos != null && equippedItem != null && equippedItem.IsOwner)
        //{
        //    equippedItem.transform.position = holdPos.position + equippedItem.posOffset;
        //    equippedItem.transform.rotation = holdPos.rotation * equippedItem.rotOffset;
        //}
    }

    public void UseItem()
    {
        if (equippedItem != null && IsOwner)
        equippedItem.Use();
    }

    
}
