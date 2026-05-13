using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;


public interface IInteractable
{

    void ToggleSelected(bool isSelected);
    NetworkObject NetworkObject { get; }

    public abstract void OnInteract();
}
