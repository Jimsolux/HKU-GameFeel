using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    PlayerMovement playerMovement;
    Animator animator;
    PlayerMovement.State newState;
    PlayerMovement.State lastState;

    private int jumpHash;
    private int runningHash;
    private int idleHash;
    private int fallingHash;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();

        jumpHash = Animator.StringToHash("Anim_Jump");
        runningHash = Animator.StringToHash("Anim_Run");
        idleHash = Animator.StringToHash("Anim_Idle");
        fallingHash = Animator.StringToHash("Anim_Fall");
    }
    private void FixedUpdate()
    {
        CompareStates();
    }
    private void CompareStates()
    {
        //if the state has changed, and only then
        if (playerMovement.currentState != lastState)
        {
            switch (playerMovement.currentState)
            {
                case PlayerMovement.State.General:
                    PlayAnimation(idleHash);
                    break;
                case PlayerMovement.State.Idle:
                    PlayAnimation(idleHash);
                    break;
                case PlayerMovement.State.Walking:
                    PlayAnimation(runningHash);
                    break;
                case PlayerMovement.State.Jumping:
                    PlayAnimation(jumpHash);
                    break;
                case PlayerMovement.State.Walljumping:
                    PlayAnimation(jumpHash);
                    break;
                case PlayerMovement.State.Falling:
                    PlayAnimation(fallingHash);
                    break;
                case PlayerMovement.State.Dashing:
                    PlayAnimation(jumpHash);
                    break;
                case PlayerMovement.State.Grabbing:
                    PlayAnimation(fallingHash);
                    break;
                case PlayerMovement.State.SlidingDown:
                    PlayAnimation(fallingHash);
                    break;
            }
            //Check the current state and set the right bool true.
        }
        //

        lastState = playerMovement.currentState;
    }


    private void PlayAnimation(int hash)
    {
        AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
        if (hash != currentAnimation.shortNameHash) animator.Play(hash);
    }
}