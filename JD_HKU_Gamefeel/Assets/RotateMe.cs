using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMe : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private GameObject rotateTarget;
    

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.RotateAround(rotateTarget.transform.position, Vector3.forward, rotateSpeed);
    }
}
