using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] float checkPointValue; //ranking in how important i am.
    SpriteRenderer spriteRenderer;
    Energy energy;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        energy = GameObject.Find("PlayerObject").GetComponent<Energy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckPointSystem cps = collision.GetComponent<CheckPointSystem>();
            cps.ReceiveCheckPoint(gameObject.transform.position, checkPointValue);
            spriteRenderer.color = Color.blue;
            energy.ResetEnergy();
        }
    }
}
