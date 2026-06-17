using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using System;





[RequireComponent(typeof(CharacterController))]
public class FPController : BaseCharacter
{






    [Header("Movement Parameters")]
    public float MaxUprightSpeed => SprintInput ? SprintSpeed : WalkSpeed;
    public float MaxCrouchSpeed => SprintInput ? CrouchSprintSpeed : CrouchWalkSpeed;

    public float MaxSpeed => Crouched ? MaxCrouchSpeed : MaxUprightSpeed;





    public float Acceleration = 15f;

    [SerializeField] float WalkSpeed = 3.5f;
    [SerializeField] float SprintSpeed = 8f;
    [SerializeField] float CrouchWalkSpeed = 1.5f;
    [SerializeField] float CrouchSprintSpeed = 3.5f;
    [SerializeField] LayerMask environmentLayer;



    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight;

    public bool MovementEnabled = true;
    public bool CameraEnabled = true;
    public bool LookEnabled = true;

    [Space(15)]
    [Tooltip("This is how high the character can jump.")]
    [SerializeField] float BaseJumpHeight = 3f;




    [SerializeField] LayerMask pickup;






    public bool Sprinting
    {
        get
        {
            return SprintInput && CurrentSpeed > 0.1f;
        }
    }

    [Header("Looking Parameters")]
    public Vector2 LookSensitivity = new Vector2(0.1f, 0.1f);

    public float PitchLimit = 85f;

    [SerializeField] float currentPitch = 0f;

    public float CurrentPitch
    {
        get => currentPitch;

        set
        {
            currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit);
        }
    }

    [Header("Camera Parameters")]
    [SerializeField] float CameraNormalFOV = 60f;
    [SerializeField] float CameraSprintFOV = 80f;
    [SerializeField] float CameraFOVSmoothing = 1f;

    float TargetCameraFOV
    {
        get
        {
            return Sprinting ? CameraSprintFOV : CameraNormalFOV;
        }
    }

    [Header("Physics Parameters")]
    [SerializeField] float GravityScale = 3f;

    public float VerticalVelocity = 0f;

    public Vector3 CurrentVelocity { get; private set; }
    public float CurrentSpeed { get; private set; }

    public bool IsGrounded => characterController.isGrounded;


    [Header("Input")]
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool SprintInput;
    public bool CrouchInput;
    public bool Crouched;
    private Vector3 initialCameraPos;




    public bool WheelInput;

    [Header("Interacting")]
    [SerializeField] float InteractDistance = 5f;
    [SerializeField] private IInteractable selectedInteractable;

    [Header("Coyote Time")]
    [SerializeField] float CoyoteTimeDuration = 0.15f; // Adjust this value as needed
    private float coyoteTimeCounter = 0f;





    [Header("Components")]
    [SerializeField] private CinemachineBrain fpCamBrain;
    [SerializeField] CinemachineCamera fpCamera;
    [SerializeField] CharacterController characterController;
    [SerializeField] Image interactPopup;
    [SerializeField] private Sprite interactSprite;




    [Header("Network Stuff")]
    private GameManager gameManager;

    private ItemManager itemManager;

    private FlashRed flashRed;

    private TMP_Text healthText;

    










    // Start is called once before the first execution of Update after the MonoBehaviour is created

    #region Unity Methods

    

    private void Update()
    {
        
        if (!IsOwner)
        {
            Debug.Log("Player" + OwnerClientId + " is not the owner");
            return;
        }

        Debug.Log(selectedInteractable);
        if (MovementEnabled)
        {
            MoveUpdate();
        }
        if (CameraEnabled)
        {
            CameraUpdate();
        }

        if (LookEnabled) LookUpdate();

        CheckForInteract();

        // Update coyote time counter
        if (IsGrounded)
        {
            coyoteTimeCounter = CoyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }



        



    }



    
    #endregion

    #region Controller Methods



    public void UseItem()
    {
        itemManager.UseItem();
    }




    public void TryJump()
    {
        if (coyoteTimeCounter <= 0f || MovementEnabled == false)
        {
            return;
        }



        VerticalVelocity = Mathf.Sqrt(BaseJumpHeight * -2f * Physics.gravity.y * GravityScale);


        // Reset coyote time so player can't jump again mid-air
        coyoteTimeCounter = 0f;
    }



    public void AddVelocity(float force, Vector3 dir)
    {
        float storedVelocity = force;

        CurrentVelocity = CurrentVelocity + (dir.normalized * storedVelocity);
    }



    //public void Pause()
    //{
    //    if (gameManager.paused)
    //    {
    //        gameManager.SetPauseMenu(false);
    //        //MovementEnabled = true;
    //        //CameraEnabled = true;
    //        //LookEnabled = true;
    //    }

    //    else
    //    {
    //        gameManager.SetPauseMenu(true);
    //        //MovementEnabled = false;
    //        //CameraEnabled = false;
    //        //LookEnabled = false;
    //    }


    //}

    void MoveUpdate()
    {
        if (CrouchInput) Crouched = true;

        if (CrouchInput == false)
        {
            RaycastHit hit;
            if (Physics.Raycast(fpCamera.transform.position, Vector3.up, out hit, characterController.height / 3, environmentLayer))
            {
                Crouched = true;
                Debug.Log("Something Overhead");
            }
            else
            {
                Crouched = false;
            }
        }


        var heightTarget = Crouched ? crouchHeight : standingHeight;






        fpCamera.transform.localPosition = new Vector3(0, heightTarget / 2, 0);
        characterController.height = heightTarget;





        Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        motion.y = 0f;
        motion.Normalize();



        if (motion.sqrMagnitude >= 0.01f)
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
        }
        else
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
        }

        if (IsGrounded && VerticalVelocity <= 0.01f)
        {
            VerticalVelocity = -3f;
        }
        else
        {
            VerticalVelocity += Physics.gravity.y * GravityScale * Time.deltaTime;
        }


        Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);

        characterController.Move(fullVelocity * Time.deltaTime);

        CurrentSpeed = CurrentVelocity.magnitude;
    }

    void LookUpdate()
    {
        Vector2 input = new Vector2(LookInput.x * LookSensitivity.x, LookInput.y * LookSensitivity.y);

        // looking up and down
        CurrentPitch -= input.y;

        fpCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);


        // looking left and right
        transform.Rotate(Vector3.up * input.x);
    }

    void CameraUpdate()
    {
        float targetFOV = CameraNormalFOV;

        if (Sprinting)
        {
            float speedRatio = CurrentSpeed / SprintSpeed;

            targetFOV = Mathf.Lerp(CameraNormalFOV, CameraSprintFOV, speedRatio);
        }


        fpCamera.Lens.FieldOfView = Mathf.Lerp(fpCamera.Lens.FieldOfView, targetFOV, CameraFOVSmoothing * Time.deltaTime);
    }

    public void Reload()
    {
        itemManager.RequestReloadServerRPC(itemManager.equippedItem.NetworkObjectId);
    }

    public void TryInteract()
    {
        Debug.Log("trying to interact");
        if (selectedInteractable != null)
        {
            selectedInteractable.OnInteract(gameObject);
        }
        else if (itemManager.equippedItem != null)
        {
            itemManager.TryDrop();
        }
    }

    public override void TakeDamage(float damage)
    {
   
        base.TakeDamage(damage);
        flashRed.FlashClientRpc();

        
        
    }

    //public void Item1()
    //{
    //    itemManager.SwapItem(0);
    //}

    //public void Item2()
    //{
    //    itemManager.SwapItem(0);
    //}






    #endregion












    #region Interact

    private void CheckForInteract()
    {
        RaycastHit hit;

        if (Physics.Raycast(fpCamera.transform.position, fpCamera.transform.forward, out hit, InteractDistance, pickup))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.Interactable == true)
            {
                interactPopup.gameObject.SetActive(true);



                selectedInteractable = hit.collider.GetComponent<IInteractable>();
            }
            else
            {
                interactPopup.gameObject.SetActive(false);
                selectedInteractable = null;
            }
        }
        else
        {
            interactPopup.gameObject.SetActive(false);
            selectedInteractable = null;
        }
    }

    #endregion

    #region Network Specific Functions 

    public override void OnNetworkSpawn()
    {   
        flashRed = GetComponent<FlashRed>();
        if (IsOwner)
        {
            fpCamera.Priority += 10;
            fpCamBrain.GetComponent<Camera>().depth += 10;
        }
        else
        {
            Destroy(fpCamBrain.gameObject.GetComponent<AudioListener>());

            return;
        }

            int channelIndex = (int)OwnerClientId + 1;
        OutputChannels channel = (OutputChannels)(1 << channelIndex);

        fpCamera.OutputChannel = channel;
        fpCamBrain.ChannelMask = channel;

        if (!IsOwner) return;

        gameManager = FindFirstObjectByType<GameManager>();

        itemManager = GetComponent<ItemManager>();

        standingHeight = characterController.height;

        initialCameraPos = fpCamera.transform.localPosition;

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        interactPopup = GameObject.Find("InteractPopup").GetComponent<Image>();
        healthText = GameObject.Find("Health").GetComponent<TMP_Text>();
        currentHealth.OnValueChanged += (oldVal, newVal) =>
        {
            if (IsOwner) healthText.text = newVal.ToString();
        };

        if (IsServer)
        {
            currentHealth.Value = maxHealth.Value;
        }
        healthText.text = currentHealth.Value.ToString();

    }



    #endregion

}
