using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bubble : MonoBehaviour
{
    public SphereCollider spherCollider;
    public Transform m_transform;
    public LayerMask layerMask;
    private float radius;
    private GrabablePhysicsHandler _temp;
    public  bool isMalus;
    [HideInInspector] public GrabManager grabManager;
    bool hasFlocks;
    private void Start()
    {
        radius = spherCollider.radius *m_transform.lossyScale.x;
        
        spherCollider.enabled = false;
    }
    private void Update()
    {
        var collidiers = Physics.OverlapSphere(m_transform.position, radius, layerMask);

       
        for (int i = 0; i < collidiers.Length; i++)
        {
           
            _temp = collidiers[i].GetComponentInParent<GrabablePhysicsHandler>();
            if (_temp && !hasFlocks)
            {
                if (grabManager == null)
                    grabManager = _temp.GetComponentInParent<GrabablePhysicsHandler>().inputManager.GetComponentInChildren<GrabManager>();
                grabManager.AddBubble(isMalus, gameObject);
                hasFlocks = true;
            }
        }
        
        if(collidiers.Length == 0 && grabManager != null && hasFlocks)
        {
            grabManager.RemoveBubble(isMalus, gameObject);
            hasFlocks = false;
        }
    }

}