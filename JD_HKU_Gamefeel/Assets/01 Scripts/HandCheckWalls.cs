using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCheckWalls : MonoBehaviour
{

    public bool canGrab;
    [SerializeField] LayerMask grabLayers;
    [SerializeField] PlayerMovement movement;


    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject.layer);
        //Debug.Log("my Layer to grab is " + 6);

        if (other.gameObject.layer == 6)
        {
            canGrab = true;
            //Debug.Log("Can grab is true");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == 6)
        {
            canGrab = false;
            //Debug.Log("Can grab is false");
        }
    }
}
