using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    Collectables collectables;
    private void Awake()
    {
        collectables = GameObject.Find("Collectables").GetComponent<Collectables>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collectables.GainCoin();
            StartCoroutine(DestroyAfterTime());
            this.gameObject.SetActive(false);
        }
    }


    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(3f);
        Destroy(this.gameObject);

    }
}
