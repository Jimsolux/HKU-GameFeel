using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] public float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private SpriteRenderer sprite;
    Color oldColor;

    private void Awake()
    {
        health = maxHealth;
        sprite = GetComponentInChildren<SpriteRenderer>();
        oldColor = sprite.color;

    }

    private void CheckHealth()
    {
        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public void GainHealth(float value) { health += value; CheckHealth(); }

    public void TakeDamage(float value) { health -= value; CheckHealth(); StartCoroutine(FlashTextureRed()); }


    //Dmg Visualisation
    private IEnumerator FlashTextureRed()
    {
        //save sprite color
        //set sprite color red
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        //reset sprite color
        sprite.color = oldColor;
    }

}
