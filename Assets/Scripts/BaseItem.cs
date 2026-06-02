using System;
using Unity.Netcode;
using UnityEditor.XR;
using UnityEngine;

public abstract class BaseItem : NetworkBehaviour, IInteractable
{
    public NetworkVariable<bool> m_isAvailable = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //public bool CanBePickedUp => m_isAvailable.Value;

    public bool Interactable { get => m_isAvailable.Value; }


    public Vector3 posOffset;
    public Quaternion rotOffset;

    public void OnInteract(GameObject interactingObject)
    {
        ItemManager itemManager = interactingObject.GetComponent<ItemManager>();

        if (itemManager != null)
        {
            PickupThis(itemManager);
        }
        else
        {
            Debug.Log("ItemManager not valid");
        }
    }

    public void ToggleSelected(bool isSelected)
    {
        
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_isAvailable.OnValueChanged += OnAvailabilityChanged;
        ApplyAvailabilityState(m_isAvailable.Value);
    }

    public override void OnNetworkDespawn()
    {
        m_isAvailable.OnValueChanged -= OnAvailabilityChanged;
        base.OnNetworkDespawn();
    }

    private void OnAvailabilityChanged(bool previousValue, bool newValue)
    {
        ApplyAvailabilityState(newValue);
    }

    protected abstract void ApplyAvailabilityState(bool newValue);
    

    public void PickupThis(ItemManager itemManager)
    {
        Debug.Log("PickupThis called");
        if (!m_isAvailable.Value) return;
        Debug.Log("PickupThis running");

        itemManager.EquipItemServerRpc(NetworkObjectId);

    }
    public abstract void Use();
}
