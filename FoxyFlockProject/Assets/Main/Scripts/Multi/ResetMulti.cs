using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Experimental;
public class ResetMulti : NetworkBehaviour
{
    private List<GameObject> freezedFlocks = new List<GameObject>();
    private List<int> freezdFlockPoolIndex = new List<int>();
    private List<int> freezdFlockIndex = new List<int>();
    public GrabManager grabManager;
    public InputManager inputManager;
    private SoundReader soundReader;
    public virtual void AddFreezFlock(GameObject flock, int poolIndex, int flockIndex, bool isHotFlox = false)
    {
        if (flock == null)
            return;
        freezedFlocks.Add(flock);

        freezdFlockPoolIndex.Add(poolIndex);
        freezdFlockIndex.Add(flockIndex);
        CmdFreezFlock(flock, isHotFlox);
    }
    public virtual void RemoveFreezedFlock(GameObject flock, int indexOfPool, int indexOfFLock)
    {
        if (freezedFlocks.Contains(flock))
        {
            var index = freezedFlocks.IndexOf(flock);
            CmdDestroyFlock(flock,indexOfPool);
            freezedFlocks.RemoveAt(index);
            freezdFlockPoolIndex.RemoveAt(index);
            freezdFlockIndex.RemoveAt(index);

        }
    }
   
    public virtual void ResetEvent()
    {
        StartCoroutine(WaitToSeeTable());
    }
    IEnumerator WaitToSeeTable()
    {
       yield return new WaitUntil(() => grabManager.GetComponentInParent<PlayerMovementMulti>().SeeTable());
        for (int i = 0; i < freezedFlocks.Count; i++)
        {
            CmdDestroyFlock(freezedFlocks[i], freezdFlockPoolIndex[i]);
        }
        if (soundReader == null)
            soundReader = grabManager.sound;
        soundReader.secondClipName = "StartReset";
        if (freezedFlocks != null && freezedFlocks.Count > 0)
            StartCoroutine(LastFlockIsDestroy(freezedFlocks[freezedFlocks.Count - 1].GetComponent<DissolveFlox>()));
        freezedFlocks.Clear();
        freezdFlockPoolIndex.Clear();
        freezdFlockIndex.Clear();
    }
    [Command]
    public void CmdDestroyFlock(GameObject flock, int indexOfPool)
    {
    //    grabManager.DestroyFlock(flock, indexOfPool);
        RpcDestroyFlock(flock, indexOfPool);


    }
    [ClientRpc]
    public void RpcDestroyFlock(GameObject flock, int indexOfPool)
    {
        grabManager.DestroyFlock(flock, indexOfPool);

    }

    [Command(requiresAuthority = false)]
    public void CmdFreezFlock(GameObject flock, bool isHotFlox)
    {
        RpcFreezFlock(flock, isHotFlox);
    }
    [ClientRpc]
    public void RpcFreezFlock(GameObject flock, bool isHotFlox)
    {
        if(!isHotFlox)
       StartCoroutine( flock.GetComponent<GrabablePhysicsHandler>().Freez());

        flock.GetComponent<GrabbableObject>().enabled = false;
        flock.GetComponent<GrabablePhysicsHandler>().enabled= false;
        flock.GetComponent<Rigidbody>().isKinematic =true;
        flock.GetComponentInChildren<FloxExpressionManager>().isFrozen = true;

        NetworkRigidbody rgb;
        if (flock.TryGetComponent<NetworkRigidbody>(out rgb))
            Destroy(rgb);
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroy(GameObject flock)
    {
        if (flock)
        flock.transform.position+=Vector3.up*250*(Random.Range(1,250));
    }
    IEnumerator LastFlockIsDestroy(DissolveFlox dissolveFlox)
    {
        grabManager.playGround.soundReader.Play("Dissolve");
        yield return new WaitForSeconds(dissolveFlox.dissolveTime);
        soundReader.ThirdClipName = "EndReset";
        soundReader.PlayThird();
        grabManager.UpdateIntersectionPos();
    }
}
