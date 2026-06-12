using System.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public abstract class BaseGun : BaseWeapon
{

    public Transform firePoint;
    public int magSize;
    public int reserveAmmo;

    [SerializeField] protected NetworkVariable<int> currentAmmo = new(6, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
 
    public float bulletDamage;
    public float bulletSpeed;
    public float attackSpeed;
    public float range;
    public int bounces;
    public float reloadSpeed;

    public bool reloading = false;
    public bool canAttack = true;
    public LayerMask hitLayer;
    public GameObject bulletTrail;

    public Camera playerCamera;

    public ItemManager owningPlayer;




    public override void OnNetworkSpawn()
    {
        if (IsServer) // Server initializes the value once
        {
            currentAmmo.Value = magSize;
            canAttack = currentAmmo.Value > 0;
        }
    }

    public override void OnPickup()
    {
        //if (currentAmmo.Value > 0)
        //{
        //    canAttack = true;
        //}
    }
    public void FindReferences()
    {
        playerCamera = transform.parent.Find("PlayerCamera").GetComponent<Camera>();
    }
    // BaseGun
    public override void Use(Vector3 aimOrigin, Vector3 aimDirection)
    {
        if (!IsServer) return;
        if (!canAttack || currentAmmo.Value <= 0) return;

        canAttack = false;

        Ray ray = new Ray(aimOrigin, aimDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, hitLayer))
        {
            BaseCharacter character = hit.collider.GetComponent<BaseCharacter>();
            if (character != null) character.TakeDamage(bulletDamage);
            SpawnTrail(hit.point);
        }
        else
        {
            SpawnTrail(ray.GetPoint(range));
        }

        DecreaseAmmo();
        Invoke("ResetAttack", attackSpeed);
    }

    public virtual void StartReload()
    {
        if (!IsServer) return;
        StartCoroutine(Reload());
    }

    private void ResetAttack()
    {
        canAttack = true;
    }
    public IEnumerator Reload()
    {
        if (currentAmmo.Value >= magSize || reserveAmmo <= 0 || reloading)
        {
            yield break;
        }

        reloading = true;
        canAttack = false;







        yield return new WaitForSeconds(reloadSpeed);

        int ammoNeeded = magSize - currentAmmo.Value;
        if (reserveAmmo >= ammoNeeded)
        {
            reserveAmmo -= ammoNeeded;
            currentAmmo.Value = magSize;
        }
        else
        {
            currentAmmo.Value += reserveAmmo;
            reserveAmmo = 0;
        }

        reloading = false;
        canAttack = true;

    }

    public void DecreaseAmmo()
    {
        currentAmmo.Value -= 1;
        if (currentAmmo.Value <= 0)
        {
            canAttack = false;
        }
    }


    

    // Called locally, then tells all clients to spawn the trail
    public void SpawnTrail(Vector3 hitPoint)
    {
        SpawnTrailClientRpc(firePoint.position, hitPoint);
    }

    [ClientRpc]
    private void SpawnTrailClientRpc(Vector3 startPoint, Vector3 hitPoint)
    {
        if (bulletTrail == null)
        {
            Debug.Log("No Trail set! Returning...");
            return;
        }
        GameObject line = Instantiate(bulletTrail, startPoint, Quaternion.identity);
        LineRenderer lineRen = line.GetComponent<LineRenderer>();
        lineRen.useWorldSpace = true;
        lineRen.SetPosition(0, startPoint);
        lineRen.SetPosition(1, hitPoint);
    }


}
