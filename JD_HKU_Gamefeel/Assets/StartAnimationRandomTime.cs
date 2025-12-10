using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StartAnimationRandomTime : MonoBehaviour
{
    Animator animator;
    [SerializeField] private float myRangeMin;
    [SerializeField] private float myRangeMax;
    SpriteRenderer mySprite;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        mySprite = GetComponent<SpriteRenderer>();
        StartCoroutine(RandomTimedStart());
    }

    private IEnumerator RandomTimedStart()
    {
        yield return new WaitForSeconds(Random.Range(myRangeMin, myRangeMax));
        if(animator != null) animator.Play("WindAnim");
        mySprite.flipY = Random.Range(0,2) == 1;
        transform.localScale = new Vector2(Random.Range(0.6f, 1.2f), Random.Range(0.6f, 1.2f));

    }
}
