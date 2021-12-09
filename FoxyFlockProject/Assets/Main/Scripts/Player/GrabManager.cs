using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror.Experimental;
public class GrabManager : MonoBehaviour
{
    public List<Modifier> modifiers;
    public List<GrabablePhysicsHandler> grabableObjects;
    public List<Batch> batches;
    public PhysicMaterial[] basicMats = new PhysicMaterial[2];
    public Representation[] representations;
    public List<pool> mainPool;
    private bool isGrabLeft;
    private bool isGrabRight;
    private bool isFirstBacthPassed;
    private int currentPool;
    private List<GameObject> malusNumber;
    private List<GameObject> bonusNumber;
    public PlayGround playGround;
    public InputManager inputManager;
    public GameObject fireBallInstantiated;

    // public Buble[] bubles;
#if UNITY_EDITOR
    public bool modifierFoldout;
    public List<ModifierAction> actions;
    [SerializeField] public List<int> numbersPerModifer;
    public GameObject[] allflocks;
#endif
    // Start is called before the first frame update
    public virtual void Start()
    {
        inputManager = GetComponentInParent<InputManager>();
        for (int i = 0; i < grabableObjects.Count; i++)
        {
            Modifier _modifer = modifiers[UnityEngine.Random.Range(0, modifiers.Count)];
            Type type = _modifer.actions.GetType();
            var _object = GetComponent(type);
            grabableObjects[i].ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
        }
        inputManager.OnSpawn.AddListener(SpawnBacth);
        for (int i = 0; i < modifiers.Count; i++)
        {
            Type type = modifiers[i].actions.GetType();
            var _object = GetComponent(type);
            if (_object)
            {
                Destroy(_object);
            }
        }
        InitPool();
        inputManager.OnGrabbingLeft.AddListener(OnGrabLeft);
        inputManager.OnGrabbingReleaseLeft.AddListener(OnRealeseLeft);
        inputManager.OnGrabbingRight.AddListener(OnGrabRight);
        inputManager.OnGrabbingReleaseRight.AddListener(OnRealeseRight);

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
                GameObject flock = Instantiate(batches[i].pieces[x], new Vector3(300 + x * 20 + i * 5, 300 + x * 20 + i * 5, 300 + x * 20 + i * 5), Quaternion.identity);
                Modifier _modifer = modifiers[UnityEngine.Random.Range(0, modifiers.Count)];
                Type type = _modifer.actions.GetType();
                var _object = GetComponent(type);
                flock.GetComponent<GrabablePhysicsHandler>().ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
                flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
                flock.GetComponent<GrabablePhysicsHandler>().inputManager = inputManager;

                flock.GetComponent<Rigidbody>().useGravity = false;
                mainPool[i].floxes.Add(flock);
                mainPool[i].isSelected.Add(false);
                ScenesManager.instance.numberOfFlocksInScene++;
            }
        }
    }

 
    public void SpawnBacth()
    {
        int previousPool = currentPool;

        if (isFirstBacthPassed)
        {
            if (!mainPool[currentPool].isEmpty)
            {
                Debug.LogError("There are still flock on the dispenser");
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
                Debug.LogError("Your floxes are still moving");
                return;
            }
            if (mainPool[currentPool].floxes[i].GetComponent<GrabbableObject>().isGrab)
            {
                Debug.Log("grabbed");
                return;
            }
        }

        int totalWeight = 0;
        for (int i = 0; i < batches.Count-1; i++)
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
        for (int i = 0; i < batches.Count-1; i++)
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
        if(currentWeight==0)
        {
            inputManager.OnSpawn.RemoveListener(SpawnBacth);
            return;
        }
        if (previousPool < 5000)
            for (int i = 0; i < mainPool[previousPool].floxes.Count; i++)
            {
                Destroy(mainPool[previousPool].floxes[i].GetComponent<GrabbableObject>());
                Destroy(mainPool[previousPool].floxes[i].GetComponent<GrabablePhysicsHandler>());
                Destroy(mainPool[previousPool].floxes[i].GetComponent<Rigidbody>());
                NetworkRigidbody rgb;
                if (mainPool[previousPool].floxes[i].TryGetComponent<NetworkRigidbody>(out rgb))             
                Destroy(rgb);
            }
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


        if (malusNumber!= null && malusNumber.Count > 0)
        {
            AllowMalus();
            for (int i = 0; i < malusNumber.Count; i++)
            {
                if (malusNumber[i] != null)
                    NetworkManagerRace.instance.playerController.CmdDestroyBubble(malusNumber[i]);
            }
            malusNumber.Clear();
        }
        if (bonusNumber != null && bonusNumber.Count > 0)
        {
            AllowBonus();
            for (int i = 0; i < bonusNumber.Count; i++)
            {
                if (bonusNumber[i] != null)
                    NetworkManagerRace.instance.playerController.CmdDestroyBubble(bonusNumber[i]);
            }
        }
       
    }
    
    public void AddBubble(bool isMalus,GameObject bubble)
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
    public void RemoveBubble(bool isMalus, GameObject bubble)
    {
        if (isMalus)
        {
            if (malusNumber.Count == 0)
                return;
            if(malusNumber.Contains(bubble))
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
        if (mainPool[currentPool].isMalusUsed)
            return;
        representations[representations.Length - 2].gameObject.SetActive(true);
        representations[representations.Length - 2].index = representations.Length - 2;
        representations[representations.Length - 2].manager = this;
        representations[representations.Length - 2].image.texture = mainPool[currentPool].bonus.GetComponent<TextureForDispenser>().texture;
    }
    private void AllowBonus()
    {
        if (mainPool[currentPool].isBonusUsed)
            return;
        representations[representations.Length - 1].gameObject.SetActive(true);
        representations[representations.Length - 1].index = representations.Length - 1;
        representations[representations.Length - 1].manager = this;
        representations[representations.Length - 1].image.texture = fireBallInstantiated.GetComponent<TextureForDispenser>().texture;
    }

    public void GetPiece(XRBaseInteractor baseInteractor, int index)
    {
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.RightHand && !isGrabRight)
            return;
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.LeftHand && !isGrabLeft)
            return;
        representations[index].gameObject.SetActive(false); 

        if (index == representations.Length - 2)
        {
            mainPool[currentPool].isMalusUsed = true;
            return;
        }
        if (index == representations.Length - 1)
        {
            XRBaseInteractable baseInteractableBonus = mainPool[currentPool].bonus.GetComponent<GrabbableObject>();
            mainPool[currentPool].isBonusUsed = true;
            var grabableBonus = mainPool[currentPool].bonus.GetComponent<GrabablePhysicsHandler>();
            grabableBonus.enabled = true;

            StartCoroutine(WaiToSelect(baseInteractableBonus, baseInteractor, index, grabableBonus));
            grabableBonus.OnHitGround.AddListener(RespawnPiece);
            mainPool[currentPool].bonus.transform.position = baseInteractor.transform.position;
            return;
        }
        XRBaseInteractable baseInteractable = mainPool[currentPool].floxes[index].GetComponent<GrabbableObject>();

        var grabable = mainPool[currentPool].floxes[index].GetComponent<GrabablePhysicsHandler>();
        grabable.enabled = true;

        StartCoroutine(WaiToSelect(baseInteractable, baseInteractor, index, grabable));
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
    IEnumerator WaiToSelect(XRBaseInteractable baseInteractable, XRBaseInteractor baseInteractor, int index, GrabablePhysicsHandler grabable)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        InteractionManager.instance.ForceSelect(baseInteractor, baseInteractable);
        grabable.timeToSlow = playGround.timeBeforFall;
        grabable.slowForce = playGround.slowForce;
        grabable.OnEnterStasis.Invoke(grabable.gameObject, true, grabable.m_rgb);
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
        if(mainPool[currentPool].bonus == coll.gameObject && !coll.isGrab)
        {
            representations[representations.Length - 1].gameObject.SetActive(true);
            representations[representations.Length - 1].index = representations.Length - 1;
            representations[representations.Length - 1].manager = this;
            representations[representations.Length - 1].image.texture = mainPool[currentPool].bonus.GetComponent<TextureForDispenser>().texture;
            return true;
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
    }
    
}

public class pool
{
    public List<GameObject> floxes;
    public List<bool> isSelected;
    public GameObject bonus;
    public bool isEmpty;
    public bool isBonusUsed;
    public bool isMalusUsed;
}