using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Energy energy;
    [Header("Movement Stats")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpForce;
    [SerializeField] float maxHorizontalSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float DashForce;
    [SerializeField] float pushDownStrenght;
    [SerializeField] bool  grounded;
    [Header("References Movement")]
    [SerializeField] LayerMask jumpMask;
    [SerializeField] float floorRayLength;
    [Header("Costs")]
    [SerializeField] float dashCost;
    [SerializeField] float wallJumpCost;
    [SerializeField] float jumpCost;
    [SerializeField] float grabCost;
    [Header("public data")]
    //Public data.
    public bool regensEnergy;
    public PhysicsMaterial2D physicsMaterial;
    //private data
    float inputHorizontal;
    float inputVertical;
    Vector3 inputVector;
    Vector3 fullInputVector;

    [Header("CurrentState")]
    private bool isJumping;
    private bool isFalling;
    private bool isWallJumping;
    private bool isDashing;
    enum State
    {
        General,
        Idle,
        Walking,
        Jumping,
        Walljumping,
        Falling,
        Dashing,
        Grabbing
    }
    [SerializeField] private State currentState;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        energy = GetComponent<Energy>();
    }
    private void Update()
    {
        //Order of execution in Update
        GetInput();// Get all player Input
        CheckIfGrounded(); // Check player body conditions
        StateHandler(); //Switch between States depending on situation
        MovePlayer(); // Move the player accordingly.
        StateEffects();
    }
    
    #region input
    private void GetInput()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
        fullInputVector = new Vector3(inputHorizontal, inputVertical);
        inputVector = new Vector3(inputHorizontal, 0);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("Spacebar registered, should jump xxx");
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Grab();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            rb.gravityScale = 3;
        }
    }

    #endregion

    #region States
    private void StateHandler() //manages which state should be active.
    {
        if (grounded && rb.velocity.x == 0) currentState = State.Idle;
        if (grounded && rb.velocity.x != 0) currentState = State.Walking;
        if (!grounded && isJumping) currentState = State.Jumping;
        if (!grounded && !isJumping && isWallJumping) currentState = State.Walljumping;
        if (!grounded && !isJumping && rb.velocity.y < 0) currentState = State.Falling;
    }

    private void StateEffects()
    {
        switch (currentState)
        {
            case State.General:
                regensEnergy = true;
                break;
            case State.Idle:
                regensEnergy = true;
                break;
            case State.Walking:
                regensEnergy = true;
                break;
            case State.Jumping:
                regensEnergy = false;
                break;
            case State.Walljumping:
                regensEnergy = false;
                break;
            case State.Falling:
                regensEnergy = false;
                //Accelerate fallspeed.
                break;
            case State.Dashing:
                regensEnergy = false;
                break;
            case State.Grabbing:
                regensEnergy = false;
                break;
        }
    }

    #endregion

    #region movement
    private void MovePlayer()
    {
        if (currentState == State.Dashing || currentState == State.Grabbing)
        {
            //Handle the velocity somewhere else
        }
        else
        {
            //check on slope? normal van slope pakken
            Vector2 movementVector = GetSlopeAngle();
            Debug.Log(movementVector);
            //if (movementVector.x == 1)
            //{
                float targetSpeed = inputHorizontal * maxHorizontalSpeed;
                //float speedDif = targetSpeed - rb.velocity.x; // how big is the difference

                float newSpeed = Mathf.MoveTowards(rb.velocity.x, targetSpeed, acceleration * Time.deltaTime);

                rb.velocity = new Vector2(newSpeed, rb.velocity.y);
            
            if (movementVector.x != 0) //On a slope
            {
                if (grounded)
                {
                    Debug.Log(" I'm on a slope: ");
                    Vector2 pushDownVector = new Vector2(movementVector.x *-1, movementVector.y *-1);
                    rb.AddForce(pushDownVector * pushDownStrenght);
                }
            }
        }
        #region olddd
        //rb.AddForce(inputVector * moveSpeed, ForceMode2D.Force);
        //if (rb.velocity.x > maxHorizontalSpeed)
        //{ // rb.velocity.x = maxHorizontalSpeed;
        //    rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxHorizontalSpeed, rb.velocity.y);
        //}
        //else if (rb.velocity.x < -maxHorizontalSpeed)
        //{
        //    rb.velocity = new Vector2(Mathf.Sign(-rb.velocity.x) * -maxHorizontalSpeed, rb.velocity.y);
        //}
        #endregion
    }

    private Vector2 GetSlopeAngle()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, -transform.up, floorRayLength, jumpMask);
        Vector2 normal = rayHit.normal;
        Vector2 movementNormal = new Vector2(normal.y, -normal.x).normalized;

        return normal;
    }

    private void CheckIfGrounded()
    {
        Vector3 offset = new Vector3(.3f, 0, 0);
        grounded = Physics2D.Raycast(transform.position, -transform.up, floorRayLength, jumpMask)
            || Physics2D.Raycast(transform.position + offset, -transform.up, floorRayLength, jumpMask)
            || Physics2D.Raycast(transform.position - offset, -transform.up, floorRayLength, jumpMask);



        Debug.DrawRay(transform.position + offset, -transform.up * floorRayLength, Color.green);
        Debug.DrawRay(transform.position, -transform.up * floorRayLength, Color.green);
        Debug.DrawRay(transform.position - offset, -transform.up * floorRayLength, Color.green);

        //If not grounded, set friction to 0.
        //if grounded, set friction to 0.4.
        if (grounded) { SetFriction(0); }
        else { SetFriction(0.4f); }
    }
    #endregion

    #region Jumping and walljumping
    private void Jump()
    {
        if (grounded && energy.energy >= jumpCost)
        {
            //Check which movement keys are being pressed.
            //Vector2 jumpDirectionVector = (transform.up + inputVector) / 2; //unused
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
            energy.UseEnergy(jumpCost);
            //StartCoroutine(IsUsingEnergyAbility());
            StartCoroutine(JumpTime());
        }
        else if (CanGrab())
        {
            WallJump();
        }
    }
    [SerializeField] float jumpTimer;
    private IEnumerator JumpTime()// resets isJumping after .2f
    {
        yield return new WaitForSeconds(jumpTimer);
        Debug.Log("Jump Timer Over!");
        isJumping = false;
    }

    private void WallJump()
    {
        if (energy.energy > wallJumpCost)
        {
            Vector2 jumpDirectionVector = (transform.up + transform.right * wallDir * -1) / 2;
            rb.AddForce(jumpDirectionVector * wallJumpForce, ForceMode2D.Impulse);
            energy.UseEnergy(wallJumpCost);
            //StartCoroutine(IsUsingEnergyAbility());
            if (rb.velocity.y < -0.1f)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0); // cant have downward motion while jumping, will reset to 0.
            }
            isWallJumping = true;
            StartCoroutine(WallJumpTime());
        }
    }

    private IEnumerator WallJumpTime()// resets isJumping after .2f
    {
        yield return new WaitForSeconds(.2f);
        isWallJumping = false;
    }
    #endregion

    #region adapt Physics
    float lastFriction;
    public void SetFriction(float newFriction)
    {
        if (lastFriction != newFriction) // If the friction to be set has changed.
        {
            if (rb.sharedMaterial != null)
            {
                rb.sharedMaterial.friction = newFriction;
                lastFriction = newFriction;
                //Debug.Log(lastFriction);
            }
        } //otherwise don't do anything.
    }

    #endregion

    #region grabbing
    //4 game objects, hands
    [Header("Hands")]
    [SerializeField] GameObject hand1L;
    [SerializeField] GameObject hand2L;
    [SerializeField] GameObject hand3R;
    [SerializeField] GameObject hand4R;
    int wallDir = 0;
    //check for each of them if they can grab.
    private bool CanGrab()
    {
        if (hand1L.GetComponent<HandCheckWalls>().canGrab ||
            hand2L.GetComponent<HandCheckWalls>().canGrab)
        {
            wallDir = -1;
            return true;
        }
        else if (hand3R.GetComponent<HandCheckWalls>().canGrab ||
            hand4R.GetComponent<HandCheckWalls>().canGrab)
        {
            wallDir = 1;
            return true;
        }
        return false;
    }

    private void Grab()
    {
        if (CanGrab())
        {
            if (energy.energy > 10)
            {
                rb.velocity = Vector3.zero;
                rb.gravityScale = 0.1f;
                energy.UseEnergy(grabCost * Time.deltaTime);
                currentState = State.Grabbing;
                //StartCoroutine(IsUsingEnergyAbility());

            }
            else rb.gravityScale = 3;
        }
        else rb.gravityScale = 3;
    }

    #endregion

    #region Dashing

    private void Dash()
    {
        if (energy.energy >= dashCost)
        {
            if (fullInputVector != Vector3.zero)//only dash when you have a direction.
            {
                currentState = State.Dashing;
                isDashing = true;
                rb.AddForce(fullInputVector.normalized * DashForce, ForceMode2D.Impulse);
                energy.UseEnergy(dashCost);
                //StartCoroutine(IsUsingEnergyAbility());
                StartCoroutine(DashTime());
            }
        }
    }

    private IEnumerator DashTime()// resets isJumping after .2f
    {
        yield return new WaitForSeconds(jumpTimer);
        //Debug.Log("Jump Timer Over!");
        isDashing = false;
        currentState = State.General;//back to base state empty
    }

    #endregion

    #region external Velocity
    bool externalVelocityActive;

    private void SetExternalVelocityActive(Vector3 velocityExternal)
    {
        externalVelocityActive = true;
        rb.velocity = velocityExternal;
    }

    #endregion

}
