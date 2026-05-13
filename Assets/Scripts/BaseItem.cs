using System;
using Unity.Netcode;
using UnityEditor.XR;
using UnityEngine;

public abstract class BaseItem : NetworkBehaviour, IInteractable
{
    protected NetworkVariable<bool> m_isAvailable = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool CanBePickedUp => m_isAvailable.Value;

    public void OnInteract()
    {
        Pickup();
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

    private void OnAvailabilityChanged(bool previousValue, bool newValue)
    {
        ApplyAvailabilityState(newValue);
    }

    protected abstract void ApplyAvailabilityState(bool newValue);
    

    public void Pickup()
    {
        if (IsServer == false)
        {
            return;
        }
        m_isAvailable.Value = false;
        OnPickedUp();
    }

    protected abstract void OnPickedUp();
}
