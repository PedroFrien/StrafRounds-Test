using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(FPController))]
public class FPPlayer : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] FPController FPController;

    #region Input Handling


    void OnMove(InputValue value)
    {
        if (!IsOwner) return;
        Debug.Log("Player" + OwnerClientId + " is attempting to move.");

        FPController.MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        if (!IsOwner) return;
        FPController.LookInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        if (!IsOwner) return;
        FPController.SprintInput = value.isPressed;
    }

    void OnItemUse(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("ItemUseCalled");
            FPController.UseItem();
        }
    }

    //void OnAbilityWheelPress(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        FPController.OpenWheel();
    //    }
    //}

    //void OnAbilityWheelRelease(InputValue value)
    //{
    //    if (!value.isPressed)
    //    {
    //        FPController.CloseWheel();
    //    }
    //}

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (!IsOwner) return;
            FPController.TryJump();
        }
    }

    void OnCrouch(InputValue value)
    {
        if (!IsOwner) return;
        FPController.CrouchInput = value.isPressed;
    }

    void OnReload(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("Trying to Reload");
            FPController.Reload();
        }
    }

    void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            FPController.TryInteract();
        }
    }


    void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            FPController.Pause();
        }
    }


    //void OnWeapon1(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        FPController.Item1();
    //    }
    //}

    //void OnWeapon2(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        FPController.Item2();
    //    }
    //}

    #endregion


    #region Unity Methods

    private void OnValidate()
    {
        
    }

    private void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
            return;
        }
        if (FPController == null)
        {
            FPController = GetComponent<FPController>();
        }

        
        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
    }

    #endregion
}
