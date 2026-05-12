using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FPController))]
public class FPPlayer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] FPController FPController;

    #region Input Handling


    void OnMove(InputValue value)
    {
        FPController.MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        FPController.LookInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        FPController.SprintInput = value.isPressed;
    }

    //void OnItemUse(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        FPController.UseItem();
    //    }
    //}

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
            FPController.TryJump();
        }
    }

    void OnCrouch(InputValue value)
    {
        FPController.CrouchInput = value.isPressed;
    }

    //void OnReload(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        Debug.Log("Trying to Reload");
    //        FPController.Reload();
    //    }
    //}

    //void OnInteract(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        FPController.TryInteract();
    //    }
    //}


    //void OnPause(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        FPController.Pause();
    //    }
    //}


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
        if (FPController == null)
        {
            FPController = GetComponent<FPController>();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;
    }

    #endregion
}
