using System.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public abstract class BaseGun : BaseWeapon
{

    public Transform firePoint;
    public int magSize;
    public int reserveAmmo;
    public int currentAmmo;
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




    private void Awake()
    {
        currentAmmo = magSize;
    }
    public void FindReferences()
    {
        playerCamera = transform.parent.Find("PlayerCamera").GetComponent<Camera>();
    }
    public override void Use()
    {
        if (!IsServer) return;
        if (!canAttack) return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo, not able to use");
            return;
        }

        Debug.Log("Using Gun");

        canAttack = false;



        Ray ray = playerCamera.ScreenPointToRay(ScreenCenter());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, hitLayer))
        {
            BaseCharacter character = hit.collider.GetComponent<BaseCharacter>();
            if (character != null)
            {
                character.TakeDamage(bulletDamage);
            }

            SpawnTrail(hit.point);

            //if (hit.collider.tag == "Environment") SpawnDecal(hit);

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
        StartCoroutine(Reload());
    }

    private void ResetAttack()
    {
        canAttack = true;
    }
    public IEnumerator Reload()
    {
        if (currentAmmo >= magSize || reserveAmmo <= 0 || reloading)
        {
            yield break;
        }

        reloading = true;
        canAttack = false;







        yield return new WaitForSeconds(reloadSpeed);

        int ammoNeeded = magSize - currentAmmo;
        if (reserveAmmo >= ammoNeeded)
        {
            reserveAmmo -= ammoNeeded;
            currentAmmo = magSize;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }

        reloading = false;
        canAttack = true;

    }

    public void DecreaseAmmo()
    {
        currentAmmo -= 1;
        if (currentAmmo <= 0)
        {
            canAttack = false;
        }
    }


    public Vector3 ScreenCenter()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        Vector3 screenCenter = new Vector3(centerX, centerY, 0);
        return screenCenter;
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
