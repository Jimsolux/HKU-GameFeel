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
    [SerializeField] bool grounded;
    [SerializeField] float slidingGravity;
    [SerializeField] float airDragMultiplier;
    [Header("References Movement")]
    [SerializeField] LayerMask jumpMask;
    [SerializeField] float floorRayLength;
    [SerializeField] float jumpShortForce;
    [Header("Costs")]
    [SerializeField] float dashCost;
    [SerializeField] float wallJumpCost;
    [SerializeField] float jumpCost;
    [SerializeField] float grabCost;

    [Header("public data")]
    [SerializeField] GameObject spriteObject;
    //Public data.
    public bool regensEnergy;
    public PhysicsMaterial2D physicsMaterial;
    //private data
    public float inputHorizontal;
    float inputVertical;
    Vector3 inputVector;
    Vector3 fullInputVector;
    private bool facingRight;

    [Header("CurrentState")]
    private bool isJumping;
    private bool isFalling;
    private bool isWallJumping;
    private bool isDashing;
    private bool isSlidingDown;
    private bool isAttacking;
    [SerializeField] private bool isGrabbing;
    private float normalGravity = 3f;
    public enum State
    {
        General,
        Idle,
        Walking,
        Jumping,
        Walljumping,
        Falling,
        Dashing,
        Grabbing,
        SlidingDown,
        Attacking
    }
    [SerializeField] public State currentState;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        energy = GetComponent<Energy>();
        facingRight = true;
    }
    private void Update()
    {
        //Order of execution in Update
        GetInput();// Get all player Input
        CheckIfGrounded(); // Check player body conditions
        StateHandler(); //Switch between States depending on situation
        Flipping(); //Flip sprite
        MovePlayer(); // Move the player accordingly.
        StateEffects();
    }

    private void FixedUpdate()
    {

    }

    #region input
    private void GetInput()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (!grounded)
        {
            inputHorizontal *= airDragMultiplier;
        }
        inputVertical = Input.GetAxisRaw("Vertical");
        fullInputVector = new Vector3(inputHorizontal, inputVertical);
        inputVector = new Vector3(inputHorizontal, 0);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("Spacebar registered, should jump xxx");
            Jump();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0)//going up
            {
                rb.AddForce((1f - 0.6f) * jumpShortForce * rb.velocity.y * Vector2.down, ForceMode2D.Impulse);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
        if (Input.GetKey(KeyCode.C) || Input.GetMouseButton(1))
        {
            Grab();
        }
        if (Input.GetKeyUp(KeyCode.C) || Input.GetMouseButtonUp(1))
        {
            ResetGravityScale();
            isGrabbing = false; glove1.SetActive(false); glove2.SetActive(false);
            //Debug.Log("Not grab cus keyUp");
        }
        if (Input.GetMouseButtonDown(0)) // mouse L?
        {
            StartCoroutine(IAmAttacking());
        }
        //Object flipping

    }

    #endregion

    #region animation
    private void Flipping()
    {
        if (currentState == State.SlidingDown || currentState == State.Grabbing)
        {
            if (wallDir < 0 && inputHorizontal < 0)//wall on left - face right.
            {
                if (!facingRight)
                {
                    Vector3 transsform = spriteObject.transform.localScale;
                    transsform.x *= -1;
                    spriteObject.transform.localScale = transsform;
                    //CreateDust();
                }
                facingRight = true;
            }
            if (wallDir > 0 && inputHorizontal > 0)//wall on right - face left.
            {
                if (facingRight)
                {
                    Vector3 transsform = spriteObject.transform.localScale;
                    transsform.x *= -1;
                    spriteObject.transform.localScale = transsform;
                    //CreateDust();
                }
                facingRight = false;
            }
        }
        else
        {
            if (inputHorizontal > 0)
            {
                if (!facingRight)
                {
                    Vector3 transsform = spriteObject.transform.localScale;
                    transsform.x *= -1;
                    spriteObject.transform.localScale = transsform;
                }
                facingRight = true;
            }
            if (inputHorizontal < 0)
            {
                if (facingRight)
                {
                    Vector3 transsform = spriteObject.transform.localScale;
                    transsform.x *= -1;
                    spriteObject.transform.localScale = transsform;
                }
                facingRight = false;
            }

        }

    }
    #endregion

    #region States
    private void StateHandler() //manages which state should be active.
    {
        if (!isAttacking) // attacking locks movement and such, the other states should not run until player is done attacking.
        {
            if (grounded) currentState = State.Idle;
            if (grounded && rb.velocity.x != 0 && inputHorizontal != 0) currentState = State.Walking;
            if (!grounded && isJumping) currentState = State.Jumping;
            if (!grounded && !isJumping && isWallJumping) currentState = State.Walljumping;
            if (!grounded && !isJumping && rb.velocity.y < 0) currentState = State.Falling;
            if ( CanGrab() && !isGrabbing  && !isJumping && !isWallJumping && !grounded && rb.velocity.y < 0) currentState = State.SlidingDown; isSlidingDown = true;
            if( CanGrab() && isGrabbing) currentState = State.Grabbing;
            if(isGrabbing && isWallJumping) currentState = State.Walljumping;
        }
        if (isAttacking && !CanGrab()) currentState = State.Attacking;
    }


    private void StateEffects()
    {
        switch (currentState)
        {
            case State.General:
                regensEnergy = false;
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
            case State.SlidingDown:
                StartSlidingDown();
                regensEnergy = false;
                break;
            case State.Attacking:
                regensEnergy = false;
                break;
        }
        if (currentState != State.SlidingDown) ResetGravityScale();
    }

    #endregion

    #region movement
    private void MovePlayer()
    {
        if (currentState == State.Dashing || currentState == State.Grabbing || currentState == State.Attacking)
        {
            //Handle the velocity somewhere else, or be doing nothing
        }
        else
        {
            //check on slope? normal van slope pakken
            Vector2 movementVector = GetSlopeAngle();

            if (grounded)
            {
                float targetSpeed = inputHorizontal * maxHorizontalSpeed;
                float speedDifference = targetSpeed - rb.velocity.x;
                speedDifference /= 3;
                if (speedDifference < 0) speedDifference *= -1;//Always a positive value.
                if (movementVector.x != 0) //On a slope
                {
                    speedDifference = Mathf.Min(speedDifference, 2);//low speed multiplier on slopes when turning around.
                }
                float newSpeed = Mathf.MoveTowards(rb.velocity.x, targetSpeed, acceleration * speedDifference * Time.deltaTime);

                rb.velocity = new Vector2(newSpeed, rb.velocity.y);
            }
            else if (!grounded)
            {
                float targetSpeed = inputHorizontal * maxHorizontalSpeed;
                float speedDifference = Mathf.Abs(targetSpeed - rb.velocity.x);
                if (speedDifference < 0) speedDifference *= -1;//Always a positive value.

                float newSpeed = Mathf.MoveTowards(rb.velocity.x, targetSpeed, acceleration / 2 * Time.deltaTime);

                rb.velocity = new Vector2(newSpeed, rb.velocity.y);
            }


            if (movementVector.x != 0) //On a slope
            {
                if (grounded)
                {
                    //Debug.Log(" I'm on a slope: ");
                    Vector2 pushDownVector = new Vector2(movementVector.x * -1, movementVector.y * -1);
                    rb.AddForce(pushDownVector * pushDownStrenght);
                }
            }

            //if player is attacking and in air, hold the last frame of the animation until grounded. 


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
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, -transform.up, floorRayLength + 1f, jumpMask);
        Vector2 normal = rayHit.normal;
        Vector2 movementNormal = new Vector2(normal.y, -normal.x).normalized;

        return normal;
    }

    private void CheckIfGrounded()
    {
        Vector3 offset = new Vector3(.45f, 0, 0);
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

    #region Attacking

    private IEnumerator IAmAttacking()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            yield return new WaitForSeconds(.5f);
            isAttacking = false;
            currentState = State.General;
        }
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
            CreateDust();
        }
        else if (CanGrab())
        {
            WallJump();
            CreateDust();
        }
    }
    [SerializeField] float jumpTimer;
    private IEnumerator JumpTime()// resets isJumping after .2f
    {
        yield return new WaitForSeconds(jumpTimer);
        //Debug.Log("Jump Timer Over!");
        isJumping = false;
    }

    private void WallJump()
    {
        if (energy.energy > wallJumpCost)
        {
            Vector2 jumpDirectionVector = ((transform.up *2) + transform.right * wallDir * -1) / 3;
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

    private void ResetGravityScale()
    {
        if (rb.gravityScale != normalGravity)
        {
            rb.gravityScale = normalGravity;
        }
    }
    private void StartSlidingDown()
    {
        //Change the rb.gravityscale down
        if (rb.gravityScale != slidingGravity) rb.gravityScale = slidingGravity;
    }

    #endregion

    #region grabbing
    //4 game objects, hands
    [Header("Hands")]
    [SerializeField] GameObject hand1L;
    [SerializeField] GameObject hand2L;
    [SerializeField] GameObject hand3R;
    [SerializeField] GameObject hand4R;
    //gloves visual indication.
    [SerializeField] GameObject glove1;
    [SerializeField] GameObject glove2;

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
            if (energy.energy > 1)
            {
                //Debug.Log(energy.energy);
                rb.velocity = Vector3.zero;
                rb.gravityScale = 0.1f;
                energy.UseEnergy(grabCost * Time.deltaTime);
                isGrabbing = true;
                currentState = State.Grabbing;
                //StartCoroutine(IsUsingEnergyAbility());
                if(wallDir < 0) glove1.SetActive(true);
                if(wallDir > 0) glove2.SetActive(true);

            }
            else if(energy.energy < 1) rb.gravityScale = 3; isGrabbing = false; Debug.Log("not grabbing cus low energy."); glove1.SetActive(false); glove2.SetActive(false);

        }
        else if(!CanGrab()) rb.gravityScale = 3; isGrabbing = false; Debug.Log("not grab cus not cangrab"); glove1.SetActive(false); glove2.SetActive(false);

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
                CreateDashDust();
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

    #region AudioControl

    private bool grabAudioIsPlaying;
    private bool slideAudioIsPlaying;
    private bool footStepAudioIsPlaying;

    //Single shot audio:
    //Jump, Dash, Hit floor, 

    #endregion

    #region Particles
    [SerializeField] ParticleSystem dust;
    [SerializeField] ParticleSystem dashDust;

    private void CreateDust()
    {
        dust.Play();
    }

    private void CreateDashDust()
    {
        dashDust.Play();
    }

    #endregion

}
