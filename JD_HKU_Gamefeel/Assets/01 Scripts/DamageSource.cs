using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] float baseDamage;
    [SerializeField] float dmgBoost;
    [SerializeField] float myDmg;

    [Header("Attributes")]
    [SerializeField] int burstAmount;
    [SerializeField] float burstRate;
    [SerializeField] Type myType;

    [SerializeField] private enum Type
    {
        SingleHit,
        Burst,
        Constant
    }

    private void CalculateCurrentDamage()
    {
        myDmg = baseDamage * dmgBoost;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Health>())//if damageable
        {
            CalculateCurrentDamage();

            Health health = collision.GetComponent<Health>();
            health.TakeDamage(myDmg);
        }
    }
    //for longer attacks ..<>
    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if(myType == Type.Burst || myType == Type.Constant)
    //    {
    //        if (collision.GetComponent<Health>())//if damageable
    //        {
    //            CalculateCurrentDamage();

    //            Health health = collision.GetComponent<Health>();
    //            health.TakeDamage(myDmg);
    //        }
    //}


}
