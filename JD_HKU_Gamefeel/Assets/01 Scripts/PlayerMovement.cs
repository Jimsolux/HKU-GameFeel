using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Energy energy;
    [SerializeField] PixelPerfectCamera ppc;
    [SerializeField] GameObject forwardObjectToAdd;
    [Header("Movement Stats")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpForce;
    [SerializeField] float maxHorizontalSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float DashForce;
    [SerializeField] float glideForce;
    [SerializeField] float maxGlideSpeed;
    [SerializeField] float glideAcceleration;
    [SerializeField] float pushDownStrenght;
    [SerializeField] bool grounded;
    [SerializeField] float slidingGravity;
    [SerializeField] float airDragMultiplier;
    [SerializeField] float glideGravity;
    [SerializeField] float glideDashForce;
    [Header("References Movement")]
    [SerializeField] LayerMask jumpMask;
    [SerializeField] float floorRayLength;
    [SerializeField] float jumpShortForce;
    [Header("Costs")]
    [SerializeField] float dashCost;
    [SerializeField] float wallJumpCost;
    [SerializeField] float jumpCost;
    [SerializeField] float grabCost;
    [SerializeField] float glideCost;

    [Header("Public data")]
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
    private bool isGliding;
    private bool isGrabbing;
    private float normalGravity = 3f;
    [SerializeField] private bool isDashingFromGlide;
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
        Attacking,
        Gliding
    }
    [SerializeField] public State currentState;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        energy = GetComponent<Energy>();
        facingRight = true;
    }
    private void Update()
    {
        //Order of execution in Update
        GetInput();// Get all player Input
        PlayActiveAudio();
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
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(IAmAttacking());
        }
        //Object flipping

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartGliding();
        }
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            StopGliding();
            isDashingFromGlide = false;
        }
    }

    #endregion

    #region animation
    private void Flipping()
    {
        //if (currentState != State.Gliding) // Cant flip if gliding.
        //{
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
                    CreateDust();
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
                    CreateDust();
                    }
                    facingRight = false;
                }

            }
        //}

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
            if (CanGrab() && !isGrabbing && !isJumping && !isWallJumping && !grounded && rb.velocity.y < 0) currentState = State.SlidingDown; isSlidingDown = true;
            if (CanGrab() && isGrabbing) currentState = State.Grabbing;
            if (isGrabbing && isWallJumping) currentState = State.Walljumping;
            if (!grounded && isGliding &&!CanGrab() && !isDashingFromGlide) currentState = State.Gliding;
            if(!grounded && isDashing) currentState = State.Dashing;
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
                CreateDust();

                regensEnergy = false;
                break;
            case State.Attacking:
                regensEnergy = false;
                break;
            case State.Gliding:
                GlidingBehaviour();
                regensEnergy = false;
                break;
        }
        if (currentState != State.SlidingDown && currentState != State.Gliding) ResetGravityScale();
    }



    #endregion

    #region movement
    private void MovePlayer()
    {
        if (currentState == State.Dashing || currentState == State.Grabbing || currentState == State.Attacking || currentState == State.Gliding)
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
                    rb.AddForce(pushDownVector * pushDownStrenght * Time.deltaTime);
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
            AudioManager.instance.PlaySFX("Jump");
        }
        else if (CanGrab())
        {
            WallJump();
            AudioManager.instance.PlaySFX("Jump");
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
        if (energy.energy > wallJumpCost && !isDashing)
        {
            Vector2 jumpDirectionVector = ((transform.up * 1.5f) + transform.right * wallDir * -1) / 2.5f;
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
        if (rb.gravityScale != normalGravity && currentState != State.Gliding)
        {
            rb.gravityScale = normalGravity;
            Debug.Log("reset gravity scale");
        }
    }
    private void StartSlidingDown()
    {
        //Change the rb.gravityscale down
        if (currentState != State.Gliding)
        {
            if (rb.gravityScale != slidingGravity) rb.gravityScale = slidingGravity;
        }
    }

    private void SetGlidingGravity()
    {
        if (rb.gravityScale != glideGravity)
        {
            rb.gravityScale = glideGravity;
        }
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
                //rb.velocity = Vector3.zero;
                //rb.gravityScale = 0.1f;
                //energy.UseEnergy(grabCost * Time.deltaTime);
                //isGrabbing = true;
                //currentState = State.Grabbing;
                //if(wallDir < 0) glove1.SetActive(true);
                //if(wallDir > 0) glove2.SetActive(true);

            }
            else if (energy.energy < 1) rb.gravityScale = 3; isGrabbing = false; Debug.Log("not grabbing cus low energy."); glove1.SetActive(false); glove2.SetActive(false);

        }
        else if (!CanGrab()) rb.gravityScale = 3; isGrabbing = false; Debug.Log("not grab cus not cangrab"); glove1.SetActive(false); glove2.SetActive(false);

    }

    #endregion

    #region Dashing

    private void Dash()
    {
        if (energy.energy >= dashCost)
        {
            if (fullInputVector != Vector3.zero || rb.velocity != Vector2.zero)//only dash when you have a direction.
            {
                
                isDashing = true;
                if(currentState != State.Gliding)   //if im not gliding
                {
                    if (fullInputVector != Vector3.zero)
                    {
                        rb.velocity = Vector2.zero;
                        rb.AddForce(fullInputVector.normalized * DashForce, ForceMode2D.Impulse);
                    }
                    else if (fullInputVector == Vector3.zero)
                    {
                        rb.velocity = Vector2.zero;
                        rb.AddForce(rb.velocity.normalized * DashForce, ForceMode2D.Impulse);

                    }
                }
                else if (currentState == State.Gliding) //if im  not gliding again??
                {
                    if (fullInputVector != Vector3.zero)
                    {
                        Debug.Log("IsDashingFromGlideNow");
                        StopGliding();
                        rb.velocity = Vector2.zero;
                        rb.AddForce(fullInputVector.normalized * DashForce, ForceMode2D.Impulse);
                        isDashingFromGlide = true;

                    }
                    else if (fullInputVector == Vector3.zero)
                    {
                        Debug.Log("IsDashingFromGlideNow");
                        StopGliding();
                        rb.velocity = Vector2.zero;

                        rb.AddForce(rb.velocity.normalized * DashForce, ForceMode2D.Impulse);
                        isDashingFromGlide = true;
                    }
                    StopGliding();
                }
                currentState = State.Dashing;
                energy.UseEnergy(dashCost);
                //StartCoroutine(IsUsingEnergyAbility());
                StartCoroutine(DashTime());
                CreateDashDust();
                AudioManager.instance.PlayDash("Dash");
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

    #region Gliding

    private void StartGliding()
    {
        if (!grounded && !isDashing && !isWallJumping && !isDashingFromGlide)
        {
            if (!isGliding) isGliding = true;
            currentState = State.Gliding;
            SetGlidingGravity();
            StartCoroutine(AccelerateGliding());
            //Check for high velocity down, make it smaller.
            Debug.Log("Yo im gliding now!" );
            if (rb.velocity.y < -4)
            {
                float maxDownVelocity = -4;
                Vector2 v2 = new Vector2(rb.velocity.x, maxDownVelocity);
                rb.velocity = v2;
            }
            //currentZoom = normalZoom;
            AddForwardObjectToCamera();
            CreateGlideDust();

            //make the zoom softer by introducing a camera scaling?
            //StartCoroutine(GlidingZoomOut());
        }
    }

    private void GlidingBehaviour()
    {
        if (!isGliding) isGliding = true;
        //Low gravityscale

        //Push player forward in forward direction.
        Vector2 forward = Vector2.right;
        if (facingRight) forward = Vector2.right;
        else if (!facingRight) forward = Vector2.left;
        CreateGlideDust();

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            rb.AddForce(forward * glideForce * Time.deltaTime, ForceMode2D.Impulse);
        }
    }

    private void StopGliding()
    {
        
        isGliding = false;
        currentState = State.General;
        //reset gravityscale and remove forward push if still there.
        ResetGravityScale();
        //Stop Sounds

        //Stop zoom
        RemoveForwardObjectFromCamera();
        StopCoroutine(GlidingZoomOut());
        //ppc.assetsPPU = normalZoom;

    }
    private IEnumerator AccelerateGliding()
    {
        if (!isGliding && currentState != State.Gliding)
        {
            StopCoroutine(AccelerateGliding());
            glideForce = 500;
        }
        else if (currentState == State.Gliding && isGliding)
        {
            if (glideForce < maxGlideSpeed)
            {
                glideForce = Mathf.Lerp(glideForce, maxGlideSpeed, glideAcceleration * Time.deltaTime);
                yield return null; //next frame
            }
            if (Mathf.Abs(rb.velocity.x) < .2f) glideForce = 500;
            yield return null;
            StartCoroutine(AccelerateGliding());//Replays itself as long as I am gliding.
            //Debug.Log("Accelerating glidingspeed");
        }
    }

    private void AddForwardObjectToCamera()
    {
        CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
        if (!cf.objectsToFollow.Contains(forwardObjectToAdd))
        {
            cf.objectsToFollow.Add(forwardObjectToAdd);
        }
    }

    private void RemoveForwardObjectFromCamera()
    {
        CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
        if (cf.objectsToFollow.Contains(forwardObjectToAdd))
        {
            cf.objectsToFollow.Remove(forwardObjectToAdd);
        }
    }


    int normalZoom = 40;
    int wideZoom = 45;
    int currentZoom;
    private IEnumerator GlidingZoomOut()
    {

        if (!isGliding && currentState != State.Gliding)//Stop gliding
        {
            StopCoroutine(GlidingZoomOut());
            //ppc.assetsPPU = normalZoom;

        }
        else if (currentState == State.Gliding && isGliding)
        {
            if (currentZoom > wideZoom)
            {
                currentZoom--;
                ppc.assetsPPU = currentZoom;
                //Debug.Log(ppc.assetsPPU);
                yield return new WaitForSeconds(0.1f);//every .2 seconds, zoom out a little

                StartCoroutine(GlidingZoomOut()); // loop self
            }
        }
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
    private bool dashAudioIsPLaying;
    private bool glideAudioIsPlaying;


    private AudioClip footsteps;
    private AudioClip dashSound;
    private AudioClip glideSound;
    private AudioClip jumpSound;
    private AudioClip landSound;

    State lastKnownState;
    private void PlayActiveAudio()
    {
        if (lastKnownState != currentState)
        {
            switch (currentState)
            {
                case State.General:
                    AudioManager.instance.PlayPlayerSounds("Silence");

                    break;
                case State.Idle:
                    AudioManager.instance.PlayPlayerSounds("Silence");

                    break;
                case State.Walking:
                    AudioManager.instance.PlayPlayerSounds("Walk");
                    break;
                case State.Jumping:
                    AudioManager.instance.PlayPlayerSounds("Silence");

                    break;
                case State.Walljumping:
                    AudioManager.instance.PlayPlayerSounds("Silence");

                    break;
                case State.Falling:
                    AudioManager.instance.PlayPlayerSounds("Silence");

                    break;
                case State.Dashing:
                    AudioManager.instance.PlayPlayerSounds("Glide");

                    break;
                case State.Grabbing:
                    AudioManager.instance.PlayPlayerSounds("Slide");

                    break;
                case State.SlidingDown:
                    AudioManager.instance.PlayPlayerSounds("Slide");

                    break;
                case State.Attacking:
                    AudioManager.instance.PlayPlayerSounds("Silence");

                    break;
                case State.Gliding:
                    AudioManager.instance.PlayPlayerSounds("Glide");

                    break;

            }
        }
                lastKnownState = currentState;
    }
    //Single shot audio:
    //Jump, Dash, Hit floor, 

    #endregion

    #region Particles
    [SerializeField] ParticleSystem dust;
    [SerializeField] ParticleSystem dashDust;
    [SerializeField] ParticleSystem glideDust;

    private void CreateDust()
    {
        dust.Play();
    }

    private void CreateDashDust()
    {
        dashDust.Play();
    }

    private void CreateGlideDust()
    {
        glideDust.Play();
    }

    #endregion

}
