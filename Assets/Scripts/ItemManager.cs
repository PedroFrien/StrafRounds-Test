using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class ItemManager : NetworkBehaviour
{
    //[SerializeField] private NetworkObject holdPosPrefab;
    //private NetworkObject holdPos;
    [SerializeField] private Transform holdPosRef;
    [SerializeField] private Camera playerCam;
    [SerializeField] private NetworkObject player;

    private Rigidbody itemRb;



    public BaseItem equippedItem;




    //private void Awake()
    //{
    //    holdPos = Instantiate(holdPosPrefab, holdPosRef.position, holdPosRef.rotation);

    //    holdPos.TrySetParent(playerCamera);
    //}

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

        BaseGun gun = item.GetComponent<BaseGun>();
        if (gun != null) gun.FindReferences();

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
        var col = target.GetComponent<Collider>();
        if (col != null) col.enabled = false;

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

        item.OnPickup();
        //equippedItem.transform.position = holdPos.transform.position + equippedItem.posOffset;
        //equippedItem.transform.rotation = playerCamera.transform.rotation * equippedItem.rotOffset;
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
            var col = target.GetComponent<Collider>();
            if (col != null) col.enabled = true;
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





        if (equippedItem != null && equippedItem.IsOwner)
        {
            equippedItem.transform.position = holdPosRef.position + equippedItem.posOffset;
            equippedItem.transform.rotation = holdPosRef.rotation * equippedItem.rotOffset;
        }
    }


    [Rpc(SendTo.Server)]
    private void RequestUseItemServerRpc(ulong networkObjectId, Vector3 aimOrigin, Vector3 aimDirection)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target)) return;
        if (!target.TryGetComponent(out BaseItem item)) return;

        item.Use(aimOrigin, aimDirection);
    }

    public void UseItem()
    {
        if (equippedItem != null && IsOwner)
        {

            Ray ray = playerCam.ScreenPointToRay(ScreenCenter());
            RequestUseItemServerRpc(equippedItem.NetworkObjectId, ray.origin, ray.direction);
        }
    }
    [Rpc(SendTo.Server)]
    public void RequestReloadServerRPC(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject target)) return;

        BaseGun gun = target.GetComponent<BaseGun>();
        if (gun != null)
        {
            gun.StartReload();
        }
    }

    public Vector3 ScreenCenter()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        Vector3 screenCenter = new Vector3(centerX, centerY, 0);
        return screenCenter;
    }


}
