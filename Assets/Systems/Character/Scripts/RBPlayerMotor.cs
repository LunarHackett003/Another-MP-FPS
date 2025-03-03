using Steamworks;
using Unity.Cinemachine;
using UnityEditor.Experimental;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class RBPlayerMotor : LunarScript
{
    #region Variables

    //Components
    [SerializeField, HideInInspector] protected Rigidbody rb;
    [SerializeField, HideInInspector] protected CapsuleCollider capsule;
    [SerializeField, Header("Components")] protected CinemachineCamera cineCam;

    //Transforms
    [SerializeField, Header("Transforms")] protected Transform head;

    //Aiming
    [SerializeField, Header("Aiming")] protected Vector2 lookSpeed = Vector2.one * 10;
    protected float lookPitch;
    protected Vector2 lookDelta;
    protected Vector2 oldLook;
    [SerializeField] protected float lookPitchClamp = 89f;
    [SerializeField] protected float aimLookModifier = 0.8f;
    [SerializeField] protected float aimModifierPerDegree = 0.99f;
    [SerializeField] protected bool canAim;
    [SerializeField] protected bool aiming;
    [SerializeField] protected bool altAiming;


    //View
    [SerializeField, Header("View")] protected float baseFOV = 80;
    [SerializeField, Tooltip("This value is subtracted from the Base FOV when it is in effect.")] protected float aimFOV = -10, altAimFOV = -5, sprintFOV = 5;
    [SerializeField, Tooltip("How quickly, per degree of FOV in the transition, your view moves towards the target fov.")] protected float fovMoveSpeed = 0.02f;
    [SerializeField] protected float crouchedHeadHeight = 0.1f;
    [SerializeField] protected float standingHeadHeight = 0.55f;
    [SerializeField] protected float slideHeadTiltAngle = 5;
    [SerializeField] protected float slideFOV = 5;
    protected float currentFOV;

    //Movement Parameters
    [SerializeField, Header("Movement On Foot")] protected float baseMoveForce = 15;
    [SerializeField] protected float walkDamping = 8;
    [SerializeField] protected float sprintForceMultiply = 1.5f;
    [SerializeField] protected float slowWalkForceMultiply = 0.7f;
    [SerializeField] protected float aimWalkMoveMultiply = 0.7f;
    [SerializeField] protected float groundPushForce = 2;
    [SerializeField] protected bool slowWalking;
    [SerializeField] protected bool sprinting;

    [SerializeField, Header("Movement On Foot - Crouching")] protected bool crouching;
    [SerializeField] protected float sideAimWalkMoveMultiply = 0.5f;
    [SerializeField] protected float crouchWalkForceMultiply = 0.8f;
    [SerializeField] protected float crouchedCapsuleHeight = 1.2f;
    [SerializeField] protected float standingCapsuleHeight = 1.9f;
    [SerializeField] protected float crouchHeadSpeed = 5;
    [SerializeField] protected Vector3 crouchedCapsuleCentre = new(0, -0.4f, 0), standingCapsuleCentre = Vector3.zero;
    protected float currentCrouchLerp;

    [SerializeField, Header("Movement On Foot - Platforms")] protected bool followPlatformPosition;
    [SerializeField] protected bool followPlatformRotation;

    [SerializeField, Header("Movement On Foot - Sliding")] protected bool canSlide;
    [SerializeField] protected bool isSliding;
    [SerializeField] protected float slideDamping = 0.1f;
    [SerializeField] protected float slidePushOffForce = 15f;
    [SerializeField] protected float slideSteerForce = 2f;

    [SerializeField, Header("Movement In Air")] protected float jumpForce = 7f;
    [SerializeField] protected float airborneDamping = 0.02f;
    [SerializeField] protected float airMoveForce = 2f;
    [SerializeField, Header("Movement In Air - Extras")] protected bool canMultiJump = false;
    [SerializeField] protected int multiJumps = 1;
    protected int multiJumpsRemaining = 1;
    protected float currentAirborneIgnoreDampTime;

    //Ground check
    [SerializeField, Header("Grounding - Ground Check")] protected bool isGrounded;
    [SerializeField] protected Vector3 groundCheckOrigin;
    [SerializeField] protected float groundCheckDistance = 1.2f, groundCheckRadius = 0.4f;
    [SerializeField] protected LayerMask groundChecklayermask;
    protected Vector3 groundNormal;


    [SerializeField, Header("Grounding - Coyote Time")] protected bool useCoyoteTime;
    [SerializeField] protected float coyoteTime;
    protected float currentCoyoteTime;

    [SerializeField, Header("Mantling"), Tooltip("It is important to note the difference between mantling and vaulting.\n" +
        "When the player mantles, they are climbing up a ledge. When a player vaults, they are moving over a smaller obstacle.")] protected bool canMantle;
    [SerializeField, Header("Vaulting"), Tooltip("See \"Mantling\" tooltip.")] protected bool canVault;

    public Rigidbody connectedBody, lastConnectedBody;
    Vector3 connectionVelocity, connectedWorldPosition, connectedLocalPosition;
    float connectionDeltaYaw, connectionYaw, connectionLastYaw;
    #endregion




    #region Unity Messages
    private void OnValidate()
    {
        if(rb == null)
            rb = GetComponent<Rigidbody>();
        if(capsule == null)
            capsule = GetComponent<CapsuleCollider>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + groundCheckOrigin, groundCheckRadius);
        Gizmos.DrawWireSphere(transform.position + groundCheckOrigin + (Vector3.down * groundCheckDistance), groundCheckRadius);
    }

    #endregion



    #region LunarScript
    public override void LTimestep()
    {
        CheckGround();
        CheckState();
        Jump();
        CrouchPlayer();
        UpdateConnectedBody();
        MovePlayer();

        if(connectedBody != null)
        {
            if (followPlatformPosition)
                transform.position += connectionVelocity * Time.fixedDeltaTime;
            
            if(followPlatformRotation)
                transform.rotation *= Quaternion.Euler(0, connectionDeltaYaw, 0);
        }
        
        lastConnectedBody = connectedBody;
    }
    void UpdateConnectedBody()
    {
        if (connectedBody == null)
            return;

        connectionYaw = connectedBody.transform.eulerAngles.y;
        if(connectedBody == lastConnectedBody)
        {
            if (followPlatformPosition)
            {
                Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectedLocalPosition) - connectedWorldPosition;
                connectionVelocity = connectionMovement / Time.fixedDeltaTime;
                //connectionVelocity = rb.linearVelocity - connectionVelocity;

            }
            if (followPlatformRotation)
            {
                connectionDeltaYaw = connectionYaw - connectionLastYaw;
            }
        }

        connectedWorldPosition = rb.position;
        connectedLocalPosition = connectedBody.transform.InverseTransformPoint(connectedWorldPosition);
        connectionLastYaw = connectionYaw;
    }
    public override void LUpdate()
    {
        base.LUpdate();
        CheckAimState();
        Look();
    }

    #endregion

    #region View Update

    void Look()
    {
        if (InputManager.LookInput != Vector2.zero)
        {
            Vector2 lookSpeed = this.lookSpeed * (aiming || altAiming ? aimLookModifier : 1);
            lookPitch -= Time.deltaTime * lookSpeed.y * InputManager.LookInput.y;
            lookPitch = Mathf.Clamp(lookPitch, -lookPitchClamp, lookPitchClamp);
            transform.rotation *= Quaternion.Euler(0, InputManager.LookInput.x * lookSpeed.x * Time.deltaTime, 0);
            head.localRotation = Quaternion.Euler(lookPitch, 0, 0);
            oldLook = new(transform.eulerAngles.x, lookPitch);
        }
        if (lookDelta != oldLook)
            lookDelta = new Vector2(transform.eulerAngles.x % 360, lookPitch) - oldLook;
    }
    void CheckAimState()
    {
        aiming = InputManager.AimInput;
        if (aiming)
        {
            sprinting = false;
        }

        //The mother of all ternary statements...
        float fov = baseFOV + 
            //Are we aiming?
            (altAiming ? altAimFOV :
            //Are we side aiming?
            aiming ? aimFOV :
            //Are we sliding
            isSliding ? slideFOV :
            //Are we sprinting or sliding?
            ((sprinting && InputManager.MoveInput != Vector2.zero) || isSliding) ? sprintFOV :
            //Are we moving normally or crouching?
            0);

        currentFOV = Mathf.Lerp(currentFOV, fov, Time.deltaTime * fovMoveSpeed);
        cineCam.Lens.FieldOfView = currentFOV;
    }
    #endregion

    #region Movement
    void CheckGround()
    {
        if (Physics.SphereCast(transform.position + groundCheckOrigin, groundCheckRadius, -transform.up, out RaycastHit hit, groundCheckDistance, groundChecklayermask))
        {
            isGrounded = hit.normal.y > 0.4f;
            isGrounded |= useCoyoteTime && currentCoyoteTime <= coyoteTime;
            if (isGrounded)
            {
                if (canMultiJump)
                {
                    multiJumpsRemaining = multiJumps;
                }
                if (useCoyoteTime)
                {
                    currentCoyoteTime = 0;
                }
                if (hit.rigidbody && hit.rigidbody.isKinematic)
                    connectedBody = hit.rigidbody;
                else
                    connectedBody = null;
                groundNormal = hit.normal;
                return;
            }
        }
        isGrounded = false;
        if (connectedBody)
        {
            connectedBody = null;
            rb.AddForce(connectionVelocity, ForceMode.VelocityChange);
        }
        if (canMultiJump && multiJumps == multiJumpsRemaining)
        {
            multiJumpsRemaining = multiJumps - 1;
        }
        return;
    }

    void CheckState()
    {
        if (!isGrounded)
        {
            rb.linearDamping = currentAirborneIgnoreDampTime <= 0 ? 0 : airborneDamping;
        }
        else
        {
            rb.linearDamping = isSliding ? slideDamping : walkDamping;
        }
        //If we are sliding, there's some state checking we need to do for that.
        if (isSliding)
        {
            UpdateSlide();
        }
        else
        {
            //we'll assign Crouching first, and sprinting at the end of CheckState()
            //This means that sprinting starts the frame AFTER you press it, while crouching should start immediately upon pressing crouch.
            //Doing it this way allows me to detect slides better - if we're already sprinting and THEN we crouch, we'll slide. If we're crouching and then we sprint, we'll get up and sprint.
            crouching = InputManager.CrouchInput;
            if(sprinting)
            {
                //If we try to crouch while sprinting, we'll slide instead.
                if (canSlide && crouching)
                {
                    InputManager.CrouchInput = false;
                    crouching = false;
                    StartSlide();
                }
            }
            sprinting = InputManager.SprintInput && !aiming;
            if (crouching)
            {
                if (sprinting)
                {
                    crouching = false;
                    InputManager.CrouchInput = false;
                }
            }
        }
    }
    void StartSlide()
    {
        isSliding = true;
        rb.AddForce(transform.forward * slidePushOffForce);
    }
    void UpdateSlide()
    {
        if (!isGrounded || !InputManager.CrouchInput || rb.linearVelocity.sqrMagnitude < 2f)
        {
            StopSlide();
        }
    }
    void StopSlide()
    {
        isSliding = false;
        sprinting = false;
        crouching = true;
    }
    void CrouchPlayer()
    {
        currentCrouchLerp = Mathf.MoveTowards(currentCrouchLerp, crouching || isSliding ? 1 : 0, crouchHeadSpeed * Time.fixedDeltaTime);
        head.localPosition = Vector3.Lerp(Vector3.up * standingHeadHeight, Vector3.up * crouchedHeadHeight, currentCrouchLerp);
        capsule.height = Mathf.Lerp(standingCapsuleHeight, crouchedCapsuleHeight, currentCrouchLerp);
        capsule.center = Vector3.Lerp(standingCapsuleCentre, crouchedCapsuleCentre, currentCrouchLerp);
    }

    void MovePlayer()
    {
        if (isGrounded)
        {
            if (isSliding)
            {
                rb.AddForce(Vector3.ProjectOnPlane(transform.right * slideSteerForce, groundNormal));
            }
            else
            {
                Vector3 moveForce = (aiming ? aimWalkMoveMultiply : altAiming ? sideAimWalkMoveMultiply : 1) 
                    * (sprinting ? sprintForceMultiply : crouching ? crouchWalkForceMultiply :
                    slowWalking ? slowWalkForceMultiply : 1)
                    * baseMoveForce
                    * Vector3.ProjectOnPlane(new Vector3(InputManager.MoveInput.x, 0, InputManager.MoveInput.y), groundNormal);
                rb.AddForce(transform.rotation * moveForce);
                rb.AddForce(Vector3.ProjectOnPlane(-Physics.gravity, groundNormal) + groundNormal * groundPushForce);
            };
        }
        else
        {
            rb.AddForce(transform.rotation * new Vector3(InputManager.MoveInput.x, 0, InputManager.MoveInput.y) * airMoveForce);
        }
    }
    void Jump()
    {
        if (InputManager.JumpInput)
        {
            if(isGrounded || canMultiJump && multiJumpsRemaining > 0)
            {
                InputManager.JumpInput = false;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                if (canMultiJump)
                {
                    multiJumpsRemaining--;
                }
                if (useCoyoteTime)
                {
                    currentCoyoteTime = 0;
                }
            }
        }
    }
    #endregion

}
