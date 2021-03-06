using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MilestoneManager : MonoBehaviour
{
    public GameObject milestonePrefab;
    public Transform _transform;
    public Transform _tableTransform;
    public float distance;
    public List<GameObject> milestonesInstantiated = new List<GameObject>();
    public List<Milestone> milestones= new List<Milestone>();
    public int numberOfMilestones;
    public int currenMilestonIndex;


    /// <summary>
    /// return the index of the current milestones activated if none is activated, return 0
    /// </summary>
    /// <returns></returns>
    public int CheckMilestones(out Vector3 point, out int numberOfMilestones, out Vector3 nextMilestonePos)
    {
        numberOfMilestones = milestones.Count;

            for (int i = 1; i < milestones.Count; i++)
            {
                if (milestones[i].CheckCollision(out point))
                {
                    currenMilestonIndex = i;
                    nextMilestonePos = milestones[i - 1].transform.localPosition - milestones[milestones.Count-1].transform.localPosition+Vector3.up* distance / (float)numberOfMilestones;
                    return currenMilestonIndex;
                }
            }

        point = Vector3.zero;
        //nextMilestonePos = transform.localPosition + Vector3.up * distance;
        nextMilestonePos = Vector3.zero;
        return 0;
    }
}
