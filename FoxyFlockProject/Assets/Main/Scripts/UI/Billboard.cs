using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera billboardCamera;
    private void Start()
    {
        billboardCamera = GetComponentInParent<PlayerMovement>().vrHeadSett.GetComponent<Camera>();  
    }
    private void Update()
    {
        if (!billboardCamera)
            billboardCamera = GetComponentInParent<PlayerMovement>().vrHeadSett.GetComponent<Camera>();
        else
            transform.forward = billboardCamera.transform.forward;
    }
}