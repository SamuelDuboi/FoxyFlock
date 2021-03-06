using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof( SoundReader))]
public class PlayGround : MonoBehaviour
{
    public CapsuleCollider spherCollider;
    public Transform m_transform;
    public LayerMask layerMask;
    public Transform minClampTransform;
    public Transform maxClampTransform;
    [Range(1,2)]
    public float slowForce;
    public float timeBeforFall;
    [HideInInspector] public float radius;
    private RaycastHit hit;
    private float distance;
    private Vector3 point1;
    private Vector3 extend;
    private Vector3 point2;
    
    private GrabablePhysicsHandler _temp;
    public  HandsPlayground _tempHand;
    public MilestoneManager milestoneManager;

    [Header("orbes")]
    public List<GameObject> bonusOrbes;
    public List<GameObject> malusOrbes;
    public GameObject fireBallOrbe;
    public SoundReader soundReader;
    // Start is called before the first frame update
    void Start()
    {
        radius = spherCollider.radius;
        extend = new Vector3(0, spherCollider.bounds.extents.y, 0) ;
        point1 =  m_transform.position - extend;
        point2 = m_transform.position + extend ;
        spherCollider.enabled = false;
        distance = Vector3.Distance(point1, point2);
    }
    /// <summary>
    /// return the index of the current milestones activated if none is activated, return -1
    /// </summary>
    /// <returns></returns>
    public int CheckMilestones(out Vector3 point, out int numberOfMilestones, out Vector3 nextMilestonePos)
    {
        return milestoneManager.CheckMilestones(out point, out numberOfMilestones, out nextMilestonePos);
    }


    // Update is called once per frame
    void Update()
    {
       
        var collidiers = Physics.OverlapCapsule(point1, point2, radius, layerMask);

        bool hands =false;

        for (int i = 0; i < collidiers.Length; i++)
        {
            var hand = collidiers[i].GetComponentInParent<HandsPlayground>();
            if (hand != null)
            {
                hand.inPlayground = true;
                _tempHand = hand;
                hands = true;
            }

            _temp = collidiers[i].GetComponentInParent<GrabablePhysicsHandler>();
            if (_temp)
            {
                _temp.SetIsOnPlayGround(slowForce,timeBeforFall);
            }
        }
        if (hands == false && _tempHand != null)
        {
            _tempHand.inPlayground = false;
            _tempHand = null;
        }
       
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        for (float i = 0; i < 5f; i++)
        {
            Vector3 pos = Vector3.Lerp(point1, point2, i / 5);
            Handles.DrawWireDisc(pos, Vector3.up, radius);
        }
    }
#endif
}
