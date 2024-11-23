using System.Collections;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] PlayerMovement pm;
    Rigidbody2D rb;
    [SerializeField] float chaseDistance;
    [SerializeField] float speed;
    [SerializeField] float attackDistance;
    [SerializeField] GameObject enemyBody;
    private bool facingLeft;
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    }
    public State currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        idleHash = Animator.StringToHash("EN_Idle1");
        chasingHash = Animator.StringToHash("EN_Walk");
        attackHash = Animator.StringToHash("EN_Attack1");
    }
    private void FixedUpdate()
    {
        StateDecider();
        StateBehaviour();
        AnimateStates();
        SpriteFlipping();
    }

    private void StateDecider()
    {
        if (Vector2.Distance(this.transform.position, player.transform.position) > chaseDistance) currentState = State.Idle;
        if (Vector2.Distance(this.transform.position, player.transform.position) <= chaseDistance) currentState = State.Chasing;
        if (Vector2.Distance(this.transform.position, player.transform.position) <= attackDistance) currentState = State.Attacking;
    }

    private void SpriteFlipping()
    {
        Vector2 movementDirection = transform.position - player.transform.position;
        movementDirection.Normalize();
        if (movementDirection.x < 0)//Should face left
        {
            if (facingLeft)
            {

            }
            else if (!facingLeft)
            {
                Vector2 lookVector = enemyBody.transform.localScale;
                lookVector.x *= -1;
                enemyBody.transform.localScale = lookVector;
                facingLeft = true;
            }
        }
        else if (movementDirection.x > 0) //should face right
        {
            if (facingLeft)
            {
                Vector2 lookVector = enemyBody.transform.localScale;
                lookVector.x *= -1;
                enemyBody.transform.localScale = lookVector;
                facingLeft = false;
            }
            else if (!facingLeft)
            {

            }
        }
    }

    private void StateBehaviour()
    {
        switch (currentState)
        {
            case State.Idle:
                IdleBehaviour();
                break;
            case State.Chasing:
                ChasingBehaviour();
                break;
            case State.Attacking:
                AttackingBehaviour();
                break;
        }
    }

    private void IdleBehaviour()
    {

    }

    private void ChasingBehaviour()
    {
        Vector2 movementDirection = transform.position - player.transform.position;
        movementDirection.Normalize();
        rb.AddForce(movementDirection * -1 * speed, ForceMode2D.Impulse);

    }

    private void AttackingBehaviour()
    {
        StartCoroutine(HitBoxActive());

    }

    #region animation
    [Header("Animation and attacks")]
    Animator animator;
    private int idleHash;
    private int chasingHash;
    private int attackHash;
    private State lastState;
    [SerializeField] GameObject dmgCircle1;
    [SerializeField] private bool hitBoxesAreActive;
    private bool isAttacking = false;

    private void AnimateStates()
    {
        if (currentState != lastState)
        {
            switch (currentState)
            {
                case State.Idle:
                    PlayAnimation(idleHash);
                    break;
                case State.Chasing:
                    PlayAnimation(chasingHash);
                    break;
                case State.Attacking:
                    PlayAnimation(attackHash);

                    break;
            }
        }
        lastState = currentState;

    }

    private void PlayAnimation(int hash)
    {
        AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
        if (hash != currentAnimation.shortNameHash) animator.Play(hash);
    }

    private IEnumerator HitBoxActive()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            yield return new WaitForSeconds(0.35f);
            if (!hitBoxesAreActive)
            {
                dmgCircle1.SetActive(true);
                hitBoxesAreActive = true;
            }
            yield return new WaitForSeconds(0.7f);
            if (dmgCircle1 != null && hitBoxesAreActive)
            {
                dmgCircle1.SetActive(false);
                hitBoxesAreActive = false;
                isAttacking = false;

            }
        }
    }

    #endregion



}