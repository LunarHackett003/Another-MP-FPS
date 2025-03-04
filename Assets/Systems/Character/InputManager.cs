using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonBehaviour<InputManager>
{
    private Vector2 lookInput;
    private bool crouchInput;

    public PlayerInput playerInput;
    private Vector2 moveInput;
    private bool fireInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool aimInput;
    private bool altAimInput;
    private bool slowWalkInput;

    public static Vector2 MoveInput { get => Instance.moveInput; set => Instance.moveInput = value; }
    public static Vector2 LookInput { get => Instance.lookInput; set => Instance.lookInput = value; }
    public static bool FireInput { get => Instance.fireInput; set => Instance.fireInput = value; }
    public static bool JumpInput { get => Instance.jumpInput; set => Instance.jumpInput = value; }
    public static bool SprintInput { get => Instance.sprintInput; set => Instance.sprintInput = value; }
    public static bool CrouchInput { get => Instance.crouchInput; set => Instance.crouchInput = value; }
    public static bool AimInput { get => Instance.aimInput; set => Instance.aimInput = value; }
    public static bool AltAimInput { get => Instance.altAimInput; set => Instance.altAimInput = value; }
    public static bool SlowWalkInput { get => Instance.slowWalkInput; set => Instance.slowWalkInput = value; }

    public bool gamepadCrouchToggle = true;
    public bool altAimToggle = true;

    #region Input
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void GetLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    public void GetFireInput(InputAction.CallbackContext context)
    {
        fireInput = context.ReadValueAsButton();
    }
    public void GetJumpInput(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }
    public void GetSprintInput(InputAction.CallbackContext context)
    {
        sprintInput = context.ReadValueAsButton();
    }
    public void GetCrouchInput(InputAction.CallbackContext context)
    {
        crouchInput = context.ReadValueAsButton();
    }
    public void GetAimInput(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValueAsButton();
    }
    public void GetCrouchToggleInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            crouchInput = !crouchInput;
    }
    public void GetAltAimInput(InputAction.CallbackContext context)
    {

        if (altAimToggle)
        {
            if (context.performed)
                altAimInput = !altAimInput;
        }
        else
        {
            altAimInput = context.ReadValueAsButton();
        }

    }
    public void GetSlowWalkToggleInput(InputAction.CallbackContext context)
    {
        if(context.performed)
            slowWalkInput = !slowWalkInput;
    }
    public void GetSlowWalkInput(InputAction.CallbackContext context)
    {
        slowWalkInput = context.ReadValueAsButton();
    }
    public void GetCrouchGamepad(InputAction.CallbackContext context)
    {
        if (gamepadCrouchToggle)
        {
            if (context.performed)
                crouchInput = !crouchInput;
        }
        else
        {
            crouchInput = context.ReadValueAsButton();
        }
    }
    #endregion

    private void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
