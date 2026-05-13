using Unity.Netcode.Components;
using UnityEngine;

public class NewMonoBehaviourScript : BaseItem
{
    private ComponentController m_componentController;

    protected override void ApplyAvailabilityState(bool newValue)
    {
        //if(IsServer)
        //{
        //    m_componentController.SetEnabled(newValue);
        //}
    }

    public override void Use()
    {
        Debug.Log("Using Test Weapon");
    }
}
