using Unity.Netcode;
using UnityEngine;

public class FlashRed : NetworkBehaviour
{
    private Material normalMat;
    [SerializeField] private Material redMat;

    [SerializeField] private float flashDuration;

    private void Awake()
    {
        normalMat = GetComponent<Renderer>().material;
    }

    [ClientRpc]
    public void FlashClientRpc()
    {
        GetComponent<Renderer>().material = redMat;
        Invoke("EndFlash", flashDuration);
    }

    private void EndFlash()
    {
        GetComponent<Renderer>().material = normalMat;
    }
}
