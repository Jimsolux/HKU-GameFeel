using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsctoClose : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }
}
