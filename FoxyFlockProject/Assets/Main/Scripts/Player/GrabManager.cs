using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror.Experimental;

public class GrabManager : MonoBehaviour
{
    public Modifier baseModifier;
    public List<Modifier> positiveModifiers;
    public List<Modifier> negativeModifiers;
    public List<GrabablePhysicsHandler> grabableObjects;
    public List<Batch> batches;
    public PhysicMaterial[] basicMats = new PhysicMaterial[2];
    public Representation[] representations;
    public Representation[] representationsModifiers;
    public List<pool> mainPool;
    public MoveBubble moveBubble;
    protected Vector3 nextMilestonePos;
    protected bool isGrabLeft;
    protected bool isGrabRight;
    protected bool isFirstBacthPassed;
    protected int currentPool;
    protected List<GameObject> malusNumber;
    protected List<GameObject> fireBallNumber;
    protected List<GameObject> bonusNumber;
    public Reset reset;
    public PlayGround playGround;
    public InputManager inputManager;
    public GameObject fireBallInstantiated;
    protected SoundReader sound;
    public int currentMilestone = -1;
    [HideInInspector] public Vector3 positionOfMilestoneIntersection;
    protected int numberOfMilestones;
    public int weightOfBasicInRandom = 1;
    protected MaterialPropertyBlock propBlock;

    // public Buble[] bubles;
#if UNITY_EDITOR
    public bool modifierFoldout;
    public List<ModifierAction> actions;
    [SerializeField] public List<int> numbersPerModifer;
    public GameObject[] allflocks;
#endif
    // Start is called before the first frame update
    public virtual IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        propBlock = new MaterialPropertyBlock();
        sound = GetComponent<SoundReader>();
        inputManager = GetComponentInParent<InputManager>();
        for (int i = 0; i < grabableObjects.Count; i++)
        {
            Modifier _modifer = positiveModifiers[UnityEngine.Random.Range(0, positiveModifiers.Count)];
            Type type = _modifer.actions.GetType();
            var _object = GetComponent(type);
            grabableObjects[i].ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
        }
        inputManager.OnSpawn.AddListener(UpdateBatche);
        for (int i = 0; i < positiveModifiers.Count; i++)
        {
            Type type = positiveModifiers[i].actions.GetType();
            var _object = GetComponent(type);
            
        }
        for (int i = 0; i < negativeModifiers.Count; i++)
        {
            Type type = negativeModifiers[i].actions.GetType();
            var _object = GetComponent(type);
            
        }
        InitPool();
        moveBubble.MoveBubbles(playGround.radius,0.0f,positionOfMilestoneIntersection, playGround.bonusOrbes, playGround.malusOrbes);
        inputManager.OnGrabbingLeft.AddListener(OnGrabLeft);
        inputManager.OnGrabbingReleaseLeft.AddListener(OnRealeseLeft);
        inputManager.OnGrabbingRight.AddListener(OnGrabRight);
        inputManager.OnGrabbingReleaseRight.AddListener(OnRealeseRight);
        yield return new WaitForSeconds(2.0f);
        Vector3 headSettPos = inputManager.GetComponent<XRRig>().cameraFloorOffsetObject.transform.localPosition;
        transform.localPosition += headSettPos;
       
    }
    public virtual void InitPool()
    {
        mainPool = new List<pool>();
        ScenesManager.instance.numberOfFlocksInScene = 0;
        for (int i = 0; i < batches.Count; i++)
        {
            mainPool.Add(new pool());
            mainPool[i].floxes = new List<GameObject>();
            mainPool[i].isSelected = new List<bool>();
           
            for (int x = 0; x < batches[i].pieces.Count; x++)
            {
                GameObject flock = Instantiate(batches[i].pieces[x], new Vector3(300 + (x * 5 + 1) * 20 * (i * 5 + 1), 300 + x * 20, 300 + x), Quaternion.identity);
                int random = UnityEngine.Random.Range(0, negativeModifiers.Count + positiveModifiers.Count + weightOfBasicInRandom);
                Modifier _modifier = baseModifier;
                if (random > 15 && random <= negativeModifiers.Count + weightOfBasicInRandom)
                    _modifier = negativeModifiers[random - weightOfBasicInRandom-1];
                else if (random > negativeModifiers.Count + weightOfBasicInRandom)
                    _modifier = positiveModifiers[random -( weightOfBasicInRandom+1) - negativeModifiers.Count];
                Type type = _modifier.actions.GetType();
                var _object = GetComponent(type);
                flock.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifier, _object as ModifierAction, basicMats);
                flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
                flock.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

                flock.GetComponent<Rigidbody>().useGravity = false;
                mainPool[i].floxes.Add(flock);
                mainPool[i].isSelected.Add(false);
                ScenesManager.instance.numberOfFlocksInScene++;
            }
            for (int x = 0; x < batches[i].batchModifier.negativeModifier.Count; x++)
            {
                GameObject flock2 = Instantiate(batches[i].batchModifier.negativeModifier[x], new Vector3(-300 + (x + 6) * 20 * +i * 5, 300 + (x + 6) * 20 + i * 5, 300 + (x + 6) * 20 + i * 5), Quaternion.identity);
                Modifier _modifierPiece = negativeModifiers[UnityEngine.Random.Range(0, negativeModifiers.Count)];
                Type typePiece = _modifierPiece.actions.GetType();
                var _objectPiece = GetComponent(typePiece);
                flock2.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifierPiece, _objectPiece as ModifierAction, basicMats);
                flock2.GetComponent<GrabablePhysicsHandler>().enabled = false;
                flock2.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

                flock2.GetComponent<Rigidbody>().useGravity = false;
                if (mainPool[i].malus == null)
                    mainPool[i].malus = new List<GameObject>();
                mainPool[i].malus.Add(flock2);
                
            }
            for (int x = 0; x < batches[i].batchModifier.positiveModifiers.Count; x++)
            {
                GameObject flock2 = Instantiate(batches[i].batchModifier.positiveModifiers[x], new Vector3(-300 + (x + 8) * 20 * +i * 5, 300 + (x +8) * 20 + i * 5, 300 + (x + 8) * 20 + i * 5), Quaternion.identity);
                Modifier _modifierPiece = positiveModifiers[UnityEngine.Random.Range(0, positiveModifiers.Count)];
                Type typePiece = _modifierPiece.actions.GetType();
                var _objectPiece = GetComponent(typePiece);
                flock2.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifierPiece, _objectPiece as ModifierAction, basicMats);
                flock2.GetComponent<GrabablePhysicsHandler>().enabled = false;
                flock2.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

                flock2.GetComponent<Rigidbody>().useGravity = false;
                if (mainPool[i].bonus == null)
                    mainPool[i].bonus = new List<GameObject>();
                mainPool[i].bonus.Add(flock2);
               

            }
            mainPool[i].isEmptyModifier = true;


        }
    }


    #region Update
    public void UpdateBatche()
    {
        int previousPool = currentPool;
        #region security test
        if (isFirstBacthPassed)
        {
            if (!mainPool[currentPool].isEmpty)
            {
                int randomSound = UnityEngine.Random.Range(1, 3);
                sound.clipName = "FloxMachineBad" + randomSound.ToString();
                sound.Play();
                Debug.LogError("There are still flock on the dispenser");
                return;
            }
            if (!mainPool[currentPool].isEmptyModifier)
            {
                int randomSound = UnityEngine.Random.Range(1, 3);
                sound.clipName = "FloxMachineBad" + randomSound.ToString();
                sound.Play();
                Debug.LogError("There are still bonus or malus on the dispenser");
                return;
            }

        }
        else
        {
            previousPool = 5000;
            isFirstBacthPassed = true;
        }
        for (int i = 0; i < mainPool[currentPool].floxes.Count; i++)
        {
            if (mainPool[currentPool].floxes[i].GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
            {
                int randomSound = UnityEngine.Random.Range(1, 3);
                sound.clipName = "FloxMachineBad" + randomSound.ToString();
                sound.Play();
                Debug.LogError("Your floxes are still moving");
                return;
            }
            if (mainPool[currentPool].floxes[i].GetComponent<GrabbableObject>().isGrab)
            {
                int randomSound = UnityEngine.Random.Range(1, 3);
                sound.clipName = "FloxMachineBad" + randomSound.ToString();
                sound.Play();
                Debug.Log("grabbed");
                return;
            }
            if (mainPool[currentPool].floxes[i].GetComponent<GrabablePhysicsHandler>().enabled && !mainPool[currentPool].floxes[i].GetComponent<GrabablePhysicsHandler>().isOnPlayground)
            {
                int randomSound = UnityEngine.Random.Range(1, 3);
                sound.clipName = "FloxMachineBad" + randomSound.ToString();
                sound.Play();
                Debug.Log("Flock in stasis");
                return;
            }
        }
        #endregion

        int totalWeight = 0;
        for (int i = 0; i < batches.Count - 1; i++)
        {
            if (batches[i].isEmpty)
                continue;
            totalWeight += batches[i].weight;

        }
    //  int numberOfRound = 0;
    //no one need to read that
    StartLoop:
        int random = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;
        /* numberOfRound++;
         if (numberOfRound > batches.Count)
             return;*/
        for (int i = 0; i < batches.Count - 1; i++)
        {
            if (batches[i].isEmpty)
                continue;
            currentWeight += batches[i].weight;
            if (currentWeight > random)
            {
                if (batches[i].isEmpty)
                {
                    goto StartLoop;
                }
                currentPool = i;
                break;
            }
        }
        if (currentWeight == 0)
        {
            sound.PlaySeconde();
            //inputManager.OnSpawn.RemoveListener(SpawnBacth);
            return;
        }
        //in testing
        if (previousPool < 5000)
            Freez(previousPool);

        //In testing
        UpdateMilestone();

        //need to be done
        UpdateBoard();

        //done
        UpdateInventory();

        //In testing
        UpdateSpecial();

        //In testing
        UpdateBubble();


        int randomSound3 = UnityEngine.Random.Range(1, 2);
        sound.ThirdClipName = "FloxMachineGood" + randomSound3.ToString();
        sound.PlayThird();
    }
    private void Freez(int previousPool)
    {
        FreezOfList(mainPool[previousPool].floxes, previousPool);
        FreezOfList(mainPool[previousPool].bonus, previousPool);
        FreezOfList(mainPool[previousPool].malus,previousPool);
    }

    protected virtual void FreezOfList(List<GameObject> flocksToFreez,int indexOfPool)
    {
        if (flocksToFreez == null)
            return;
        for (int i = 0; i < flocksToFreez.Count; i++)
        {
            reset.AddFreezFlock(flocksToFreez[i],indexOfPool,i);
        }
    }
    protected virtual void UpdateMilestone()
    {
        currentMilestone = playGround.CheckMilestones(out positionOfMilestoneIntersection, out numberOfMilestones, out nextMilestonePos);
        Debug.Log("You have reache " + currentMilestone.ToString() + " / " + (numberOfMilestones + 1).ToString() + "Milestones");
        if (currentMilestone == numberOfMilestones)
            UIGlobalManager.instance.Win(1);
    }
    protected virtual void UpdateBoard()
    {

    }
    protected virtual void UpdateInventory()
    {
        for (int i = 0; i < representations.Length; i++)
        {
            representations[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < mainPool[currentPool].floxes.Count; i++)
        {
            if (mainPool[currentPool].isSelected[i])
                return;
            representations[i].gameObject.SetActive(true);
            representations[i].index = i;
            representations[i].manager = this;
            representations[i].image.texture = mainPool[currentPool].floxes[i].GetComponent<TextureForDispenser>().texture;
        }
    }
    protected virtual void UpdateSpecial()
    {
        if (bonusNumber != null && bonusNumber.Count > 0)
        {

            for (int i = 0; i < bonusNumber.Count; i++)
            {

                if (bonusNumber[i] != null)
                {
                    AllowBonus();
                    bonusNumber[i].GetComponent<SoundReader>().Play();
                }
            }
           
        }
        if (malusNumber != null && malusNumber.Count > 0)
        {
           
            for (int i = 0; i < malusNumber.Count; i++)
            {
                if (malusNumber[i] != null)
                {
                    AllowMalus();
                    malusNumber[i].GetComponent<SoundReader>().Play();
                }
            }

        }
        StartCoroutine(ResetModifierCount());
    }
    protected bool doOnce;
    protected virtual void UpdateBubble()
    {
        if (!doOnce)
        {
            doOnce = true;
            return;
        }    

        foreach (GameObject orb in playGround.bonusOrbes)
        {
            orb.transform.position += Vector3.up * playGround.milestoneManager.distance;
        }
        foreach (GameObject orb in playGround.malusOrbes)
        {
            orb.transform.position += Vector3.up * playGround.milestoneManager.distance;
        }
        if (playGround.fireBallOrbe)
        playGround.fireBallOrbe.transform.position += Vector3.up * playGround.milestoneManager.distance;

    }
    #endregion
    IEnumerator ResetModifierCount()
    {
        yield return new WaitForSeconds(1.6f);
        if (malusNumber != null && malusNumber.Count != 0)
            malusNumber.Clear();
        if (bonusNumber != null && bonusNumber.Count != 0)
            bonusNumber.Clear();
    }

    public void AddBubble(bool isMalus, GameObject bubble)
    {
        if (isMalus)
        {
            if (malusNumber == null)
                malusNumber = new List<GameObject>();
            if (malusNumber.Contains(bubble))
                return;
            malusNumber.Add(bubble);

        }
        else
        {
            if (bonusNumber == null)
                bonusNumber = new List<GameObject>();
            if (bonusNumber.Contains(bubble))
                return;
            bonusNumber.Add(bubble);
        }
    }
    public void AddFireBall(GameObject bubble)
    {
        if (fireBallNumber == null)
            fireBallNumber = new List<GameObject>();
        if (fireBallNumber.Contains(bubble))
            return;
        fireBallNumber.Add(bubble);
    }
    public void RemoveBubble(bool isMalus, GameObject bubble)
    {
        if (isMalus)
        {
            if (malusNumber.Count == 0)
                return;
            if (malusNumber.Contains(bubble))
                malusNumber.Remove(bubble);
        }
        else
        {
            if (bonusNumber.Count == 0)
                return;
            if (bonusNumber.Contains(bubble))
                bonusNumber.Remove(bubble);
        }
    }
    private void AllowMalus()
    {

        if (mainPool[currentPool].numberOfModifiersActivated >= representationsModifiers.Length)
        {
            //if there is no bonus, return, else override the bonus
            if (mainPool[currentPool].bonusIndex == null || mainPool[currentPool].bonusIndex.Count == 0)
                return;
            else
            {
                representationsModifiers[mainPool[currentPool].bonusIndex[0]].gameObject.SetActive(true);
                representationsModifiers[mainPool[currentPool].bonusIndex[0]].index = mainPool[currentPool].bonusIndex[0];
                representationsModifiers[mainPool[currentPool].bonusIndex[0]].manager = this;
                if (mainPool[currentPool].malusIndex == null)
                    mainPool[currentPool].malusIndex = new List<int>();
                mainPool[currentPool].malusIndex.Add(mainPool[currentPool].bonusIndex[0]);
                if (mainPool[currentPool].malusSeletcted == null)
                    mainPool[currentPool].malusSeletcted = new List<GameObject>();
                if (mainPool[currentPool].bonusIndex != null)
                {
                    representationsModifiers[mainPool[currentPool].bonusIndex[0]].image.texture = mainPool[currentPool].malus[1].GetComponent<TextureForDispenser>().texture;

                    mainPool[currentPool].malusSeletcted.Add(mainPool[currentPool].malus[1]);
                }
                
                representationsModifiers[mainPool[currentPool].bonusIndex[0]].indexInList = mainPool[currentPool].malusIndex[mainPool[currentPool].malusIndex.Count - 1];
                representationsModifiers[mainPool[currentPool].bonusIndex[0]].isMalus = true;

                mainPool[currentPool].bonusIndex.RemoveAt(0);
                mainPool[currentPool].bonusSelected.RemoveAt(0);
                return;
            }
        }

        if (mainPool[currentPool].malusIndex == null)
            mainPool[currentPool].malusIndex = new List<int>();
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].image.texture = mainPool[currentPool].malus[mainPool[currentPool].malusIndex.Count].GetComponent<TextureForDispenser>().texture;

        mainPool[currentPool].malusIndex.Add(mainPool[currentPool].malusIndex.Count);
        if (mainPool[currentPool].malusSeletcted == null)
            mainPool[currentPool].malusSeletcted = new List<GameObject>();


        mainPool[currentPool].malusSeletcted.Add(mainPool[currentPool].malus[mainPool[currentPool].malusIndex.Count-1]);
        

        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].gameObject.SetActive(true);
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].index = mainPool[currentPool].numberOfModifiersActivated;
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].manager = this;
       
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].indexInList = mainPool[currentPool].malusIndex[mainPool[currentPool].malusIndex.Count-1];
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].isMalus = true;

        mainPool[currentPool].numberOfModifiersActivated++;
        mainPool[currentPool].isEmptyModifier = false;
        if (mainPool[currentPool].isSelectedModifier == null)
            mainPool[currentPool].isSelectedModifier = new List<bool>();
        mainPool[currentPool].isSelectedModifier.Add(false);
    }

    private void AllowBonus()
    {
        if (mainPool[currentPool].numberOfModifiersActivated > representationsModifiers.Length)
            return;
        
        if (mainPool[currentPool].bonusSelected == null)
            mainPool[currentPool].bonusSelected = new List<GameObject>();
        if (mainPool[currentPool].bonusIndex == null)
            mainPool[currentPool].bonusIndex = new List<int>();
        mainPool[currentPool].bonusSelected.Add(mainPool[currentPool].bonus[mainPool[currentPool].bonusIndex.Count]);

        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].image.texture = mainPool[currentPool].bonus[mainPool[currentPool].bonusIndex.Count].GetComponent<TextureForDispenser>().texture;
      
        mainPool[currentPool].bonusIndex.Add(mainPool[currentPool].bonusIndex.Count);

        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].gameObject.SetActive(true);
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].index = mainPool[currentPool].numberOfModifiersActivated;
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].manager = this;


        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].isMalus = false;
        mainPool[currentPool].numberOfModifiersActivated++;
        representationsModifiers[mainPool[currentPool].numberOfModifiersActivated].indexInList = mainPool[currentPool].bonusIndex[mainPool[currentPool].bonusIndex.Count-1];
        if (mainPool[currentPool].isSelectedModifier == null)
            mainPool[currentPool].isSelectedModifier = new List<bool>();
        mainPool[currentPool].isSelectedModifier.Add(false);

        mainPool[currentPool].isEmptyModifier = false;

    }
    public bool isOnCollision;
    public void GetPiece(XRBaseInteractor baseInteractor, int index)
    {
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.RightHand && !isGrabRight)
            return;
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.LeftHand && !isGrabLeft)
            return;
        representations[index].gameObject.SetActive(false);

        //how to fireball
        /* if (index == representations.Length - 2)
         {
             mainPool[currentPool].isFireballUsed = true;
             XRBaseInteractable baseInteractableBonus = fireBallInstantiated.GetComponent<GrabbableObject>();

             var grabableBonus = fireBallInstantiated.GetComponent<GrabablePhysicsHandler>();
             grabableBonus.enabled = true;

             StartCoroutine(WaiToSelect(baseInteractableBonus, baseInteractor, grabableBonus, true));
             fireBallInstantiated.transform.position = baseInteractor.transform.position;

             grabableBonus.OnHitGround.AddListener(RespawnFireball);
             return;
         }*/

        XRBaseInteractable baseInteractable = mainPool[currentPool].floxes[index].GetComponent<GrabbableObject>();

        var grabable = mainPool[currentPool].floxes[index].GetComponent<GrabablePhysicsHandler>();
        grabable.enabled = true;

        StartCoroutine(WaiToSelect(baseInteractable, baseInteractor, grabable, false));
        grabable.OnHitGround.AddListener(RespawnPiece);


        mainPool[currentPool].floxes[index].transform.position = baseInteractor.transform.position;
        mainPool[currentPool].isSelected[index] = true;
        for (int i = 0; i < mainPool[currentPool].isSelected.Count; i++)
        {
            if (!mainPool[currentPool].isSelected[i])
                return;
        }
        batches[currentPool].isEmpty = true;
        mainPool[currentPool].isEmpty = true;
    }

    public void GetPieceModifier(XRBaseInteractor baseInteractor, int index, bool isMalus, int indexInList)
    {
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.RightHand && !isGrabRight)
            return;
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.LeftHand && !isGrabLeft)
            return;
        representationsModifiers[index].gameObject.SetActive(false);
        if (isMalus)
        {
            XRBaseInteractable baseInteractableBonus = mainPool[currentPool].malus[indexInList].GetComponent<GrabbableObject>();

            var grabableBonus = mainPool[currentPool].malus[indexInList].GetComponent<GrabablePhysicsHandler>();
            grabableBonus.enabled = true;

            StartCoroutine(WaiToSelect(baseInteractableBonus, baseInteractor, grabableBonus, true));
            mainPool[currentPool].malus[indexInList].transform.position = baseInteractor.transform.position;

            grabableBonus.OnHitGround.AddListener(RespawnPiece);
        }
        else
        {
            XRBaseInteractable baseInteractableBonus = mainPool[currentPool].bonus[indexInList].GetComponent<GrabbableObject>();
            var grabableBonus = mainPool[currentPool].bonus[indexInList].GetComponent<GrabablePhysicsHandler>();
            grabableBonus.enabled = true;

            StartCoroutine(WaiToSelect(baseInteractableBonus, baseInteractor, grabableBonus, false));
            grabableBonus.OnHitGround.AddListener(RespawnPiece);
            mainPool[currentPool].bonus[indexInList].transform.position = baseInteractor.transform.position;
        }

        mainPool[currentPool].isSelectedModifier[index]= true;
        for (int i = 0; i < mainPool[currentPool].isSelectedModifier.Count; i++)
        {
            if (!mainPool[currentPool].isSelectedModifier[i])
                return;
        }
        mainPool[currentPool].isEmptyModifier = true;

    }

    protected IEnumerator WaiToSelect(XRBaseInteractable baseInteractable, XRBaseInteractor baseInteractor, GrabablePhysicsHandler grabable, bool isFireBall)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        InteractionManager.instance.ForceSelect(baseInteractor, baseInteractable);
        grabable.timeToSlow = playGround.timeBeforFall;
        grabable.slowForce = playGround.slowForce;
        grabable.OnEnterStasis.Invoke(grabable.gameObject, true, grabable.m_rgb);
        yield return new WaitForSeconds(0.5f);
        if (isFireBall)
        {
            //playGround.GetComponentInChildren<FireballManager>().canAct = true;
        }

    }
    private void OnCollisionStay(Collision collision)
    {
        var coll = collision.gameObject.GetComponentInParent<GrabbableObject>();
        if (coll)
        {
            IsInCurrentPool(coll);
        }
    }
    private bool IsInCurrentPool(GrabbableObject coll)
    {
        for (int i = 0; i < mainPool[currentPool].floxes.Count; i++)
        {
            if (mainPool[currentPool].floxes[i] == coll.gameObject && !coll.isGrab)
            {
                mainPool[currentPool].isSelected[i] = false;
                mainPool[currentPool].isEmpty = false;
                representations[i].gameObject.SetActive(true);
                coll.GetComponent<Rigidbody>().useGravity = false;
                coll.GetComponent<Rigidbody>().velocity = Vector3.zero;
                coll.transform.rotation = Quaternion.identity;
                coll.transform.position = new Vector3(300 + i * 5, 300 + i * 5, 300);
                return true;
            }
        }
        if(mainPool[currentPool].bonusSelected != null)
        for (int i = 0; i < mainPool[currentPool].bonusSelected.Count; i++)
        {
            if (mainPool[currentPool].bonusSelected[i] == coll.gameObject && !coll.isGrab)
            {
                mainPool[currentPool].isSelectedModifier[mainPool[currentPool].bonusIndex[i]] = false;
                mainPool[currentPool].isEmptyModifier = false;
                representationsModifiers[mainPool[currentPool].bonusIndex[i]].gameObject.SetActive(true);
                coll.GetComponent<Rigidbody>().useGravity = false;
                coll.GetComponent<Rigidbody>().velocity = Vector3.zero;
                coll.transform.rotation = Quaternion.identity;
                coll.transform.position = new Vector3(300 + i * 5, 300 + i * 5, 300);
                return true;
            }
        }
        if (mainPool[currentPool].malusSeletcted != null)
            for (int i = 0; i < mainPool[currentPool].malusSeletcted.Count; i++)
        {
            if (mainPool[currentPool].malusSeletcted[i] == coll.gameObject && !coll.isGrab)
            {
                mainPool[currentPool].isSelectedModifier[mainPool[currentPool].malusIndex[i]] = false;
                mainPool[currentPool].isEmptyModifier = false;
                representationsModifiers[mainPool[currentPool].malusIndex[i]].gameObject.SetActive(true);
                coll.GetComponent<Rigidbody>().useGravity = false;
                coll.GetComponent<Rigidbody>().velocity = Vector3.zero;
                coll.transform.rotation = Quaternion.identity;
                coll.transform.position = new Vector3(300 + i * 5, 300 + i * 5, 300);
                return true;
            }
        }
        if (fireBallInstantiated == coll.gameObject && !coll.isGrab)
        {
            representations[representations.Length - 2].gameObject.SetActive(true);
            representations[representations.Length - 2].index = representations.Length - 1;
            representations[representations.Length - 2].manager = this;
            representations[representations.Length - 2].image.texture = fireBallInstantiated.GetComponent<TextureForDispenser>().texture;
            return true;
        }
        return false;
    }
    public virtual void OnGrabLeft()
    {
        isGrabLeft = true;
    }
    public virtual void OnRealeseLeft()
    {
        isGrabLeft = false;
    }
    public virtual void OnGrabRight()
    {
        isGrabRight = true;
    }
    public virtual void OnRealeseRight()
    {
        isGrabRight = false;
    }

    private void RespawnPiece(GameObject _object, Vector3 initPos, bool isGrab)
    {
        if (!isGrab)
        {
            _object.GetComponent<Rigidbody>().useGravity = false;
            _object.GetComponent<Rigidbody>().velocity = Vector3.zero;
            _object.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            StartCoroutine( _object.GetComponent<DissolveFlox>().StartDissolve(_object, initPos,false,this,isGrab));
        }
    }
   
    public void ResetInInventory(GameObject _object, Vector3 initPos, bool isGrab)
    {
        if (!IsInCurrentPool(_object.GetComponent<GrabbableObject>()))
        {
            for (int i = 0; i < mainPool.Count; i++)
            {
                if (i != currentPool)
                {
                    for (int x = 0; x < mainPool[i].floxes.Count; x++)
                    {
                        if (mainPool[i].floxes[x] == _object)
                        {
                            mainPool[i].isSelected[x] = false;
                            batches[i].isEmpty = false;
                            mainPool[i].isEmpty = true;
                            _object.GetComponent<Rigidbody>().useGravity = false;
                            _object.GetComponent<Rigidbody>().velocity = Vector3.zero;
                            _object.transform.rotation = Quaternion.identity;
                            _object.transform.position = new Vector3(300 + i * 5, 300 + i * 5, 300);
                        }

                        //find a way to add recreat a bacth with only one piece and nee to add this piece with others
                        // if(mainPool)
                    }
                }
            }
        }
    }
    public void ResetInInventory(GameObject _object, Vector3 initPos, bool isGrab, bool isMalus)
    {
        if (!IsInCurrentPool(_object.GetComponent<GrabbableObject>()))
        {
            for (int i = 0; i < mainPool.Count; i++)
            {
                if (i != currentPool)
                {
                    for (int x = 0; x < mainPool[i].floxes.Count; x++)
                    {
                        if (mainPool[i].floxes[x] == _object)
                        {
                            mainPool[i].isSelected[x] = false;
                            batches[i].isEmpty = false;
                            mainPool[i].isEmpty = true;
                            _object.GetComponent<Rigidbody>().useGravity = false;
                            _object.GetComponent<Rigidbody>().velocity = Vector3.zero;
                            _object.transform.rotation = Quaternion.identity;
                          
                            return;
                        }

                        //find a way to add recreat a bacth with only one piece and nee to add this piece with others
                        // if(mainPool)
                    }
                    if (mainPool[i].bonusSelected != null)
                    {
                        for (int x = 0; x < mainPool[i].bonusSelected.Count; x++)
                        {
                            if (mainPool[i].bonusSelected[x] == _object)
                            {
                                mainPool[i].isSelectedModifier[mainPool[i].bonusIndex[i]] = false;
                                mainPool[i].isEmptyModifier = false;
                                _object.GetComponent<Rigidbody>().useGravity = false;
                                _object.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                _object.transform.rotation = Quaternion.identity;
                                
                                return;

                            }
                        }
                    }
                    if (mainPool[i].malusSeletcted != null)
                    {
                        for (int x = 0; x < mainPool[i].malusSeletcted.Count; x++)
                        {
                            if (mainPool[i].malusSeletcted[x] == _object)
                            {
                                mainPool[i].isSelectedModifier[mainPool[i].malusIndex[i]] = false;
                                mainPool[i].isEmptyModifier = false;
                                _object.GetComponent<Rigidbody>().useGravity = false;
                                _object.GetComponent<Rigidbody>().velocity = Vector3.zero;
                                _object.transform.rotation = Quaternion.identity;
                             
                                return;

                            }
                        }
                    }
                }
            }
        }
    }


    public virtual void DestroyFlock(GameObject flock, int indexOfPool)
    {
        
        if(mainPool[indexOfPool].floxes.Contains( flock))
        {
            int indexOfFlock = mainPool[indexOfPool].floxes.IndexOf(flock);
            StartCoroutine(flock.GetComponent<DissolveFlox>().StartDissolve(default, Vector3.zero, true)); 
           

            GameObject _flock = Instantiate(batches[indexOfPool].pieces[indexOfFlock], new Vector3(300 + (indexOfFlock * 5 + 1) * 20 * (indexOfPool * 5 + 1), 300 + indexOfFlock * 20, 300 + indexOfFlock), Quaternion.identity);
            Modifier _modifer = baseModifier;
            Type type = _modifer.actions.GetType();
            var _object = GetComponent(type);
            _flock.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
            _flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
            _flock.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

            _flock.GetComponent<Rigidbody>().useGravity = false;
            mainPool[indexOfPool].floxes[indexOfFlock] = _flock;
            mainPool[indexOfPool].isSelected[indexOfFlock] = false;
            mainPool[indexOfPool].isEmpty = false;
        }
        else if (mainPool[indexOfPool].floxes.Contains(flock))
        {
            StartCoroutine(flock.GetComponent<DissolveFlox>().StartDissolve(default, Vector3.zero, true));
            int indexOfFlock = mainPool[indexOfPool].floxes.IndexOf(flock);


            GameObject _flock = Instantiate(batches[indexOfPool].batchModifier.positiveModifiers[indexOfFlock], new Vector3(-300 + (indexOfFlock + 6) * 20 * +indexOfPool * 5, 300 + (indexOfFlock + 6) * 20 + indexOfPool * 5, 300 + (indexOfFlock + 6) * 20 + indexOfFlock * 5), Quaternion.identity);
            Modifier _modifer = baseModifier;
            Type type = _modifer.actions.GetType();
            var _object = GetComponent(type);
            _flock.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
            _flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
            _flock.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

            _flock.GetComponent<Rigidbody>().useGravity = false;
            mainPool[indexOfPool].bonus[indexOfFlock] = _flock;
            mainPool[indexOfPool].isEmptyModifier = false;
        }
        else if (mainPool[indexOfPool].floxes.Contains(flock))
        {
            StartCoroutine(flock.GetComponent<DissolveFlox>().StartDissolve(default, Vector3.zero, true));
            int indexOfFlock = mainPool[indexOfPool].floxes.IndexOf(flock);

            GameObject _flock = Instantiate(batches[indexOfPool].batchModifier.negativeModifier[indexOfFlock], new Vector3(-300 + (indexOfFlock + 8) * 20 * +indexOfPool * 5, 300 + (indexOfFlock + 8) * 20 + indexOfPool * 5, 300 + (indexOfFlock + 8) * 20 + indexOfFlock * 5), Quaternion.identity);
            Modifier _modifer = baseModifier;
            Type type = _modifer.actions.GetType();
            var _object = GetComponent(type);
            _flock.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
            _flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
            _flock.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

            _flock.GetComponent<Rigidbody>().useGravity = false;
            mainPool[indexOfPool].malus[indexOfFlock] = _flock;
            mainPool[indexOfPool].isEmptyModifier = false;
        }
    }


}

public class pool
{
    public List<GameObject> floxes;
    public List<bool> isSelected;
    public List<bool> isSelectedModifier;
    public List<GameObject> bonus;
    public List<GameObject> bonusSelected;
    public List<GameObject> malus;
    public List<GameObject> malusSeletcted;
    public List<int> bonusIndex;
    public List<int> malusIndex;
    public int numberOfModifiersActivated;
    public bool isEmpty;
    public bool isEmptyModifier;
    public bool isFireballUsed;
}
