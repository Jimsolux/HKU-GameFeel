using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateSwordMoveSlash : MonoBehaviour
{
    [SerializeField] GameObject sword;
    [SerializeField] Animator animator;
    [SerializeField] GameObject hitBox1;
    [SerializeField] GameObject hitBox2;

    private int swordhash;
    //private void Awake()
    //{
    //    swordhash = Animator.StringToHash("SwordSlash");
    //}

    public void MoveSword()
    {
        //PlayAnimation(swordhash);
        StartCoroutine(HitBoxActive());
    }

    private bool hitBoxesAreActive;
    private IEnumerator HitBoxActive()
    {
        if (!hitBoxesAreActive)
        {
            hitBox1.SetActive(true);
            hitBox2.SetActive(true);
            hitBoxesAreActive = true;
        }
        yield return new WaitForSeconds(0.5f);
        if (hitBox1 != null && hitBox2 != null && hitBoxesAreActive)
        {
            hitBox1.SetActive(false);
            hitBox2.SetActive(false);
            hitBoxesAreActive = false;
        }
    }
    private void PlayAnimation(int hash)
    {
        AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
        if (hash != currentAnimation.shortNameHash) animator.Play(hash);
    }
}
