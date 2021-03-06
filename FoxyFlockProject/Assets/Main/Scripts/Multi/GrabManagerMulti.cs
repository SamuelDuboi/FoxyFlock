using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror.Experimental;

public class GrabManagerMulti : GrabManager
{
    private GameModeSolo gameModeSolo;
    public int numberOfPool;
    [SerializeField] public GameObject fireBallPrefab;
    [SerializeField] public GameObject fireBallPrefabOut;
    private PlayerMovementMulti playerMovement;
    private ResetMulti resetMulti;
    public Representation fireballRepresentation;
    public bool nextIsFireBallBatche;
    [HideInInspector] public int playerNumber;
    public MultiUIHandler multiUI;
    public Transform lever;
    public Transform pos1;
    public Transform pos2;
    private PlayerMovementMulti otherplayer;
    private GameObject authorityToSpawn;
    // Start is called before the first frame update
    public override IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        sound = GetComponent<SoundReader>();
        for (int i = 0; i < grabableObjects.Count; i++)
        {
            Modifier _modifer = positiveModifiers[UnityEngine.Random.Range(0, positiveModifiers.Count)];
            Type type = _modifer.actions.GetType();
            var _object = GetComponent(type);
            grabableObjects[i].ChangeBehavior(_modifer, _object as ModifierAction, basicMats);
        }

        inputManager = GetComponentInParent<InputManager>();
        inputManager.OnSpawn.AddListener(UpdateBatche);
        playGround = inputManager.GetComponent<PlayerMovementMulti>().tableTransform.GetComponent<PlayGround>();
        gameModeSolo = playGround.GetComponentInChildren<GameModeSolo>();
        playerMovement = inputManager.GetComponent<PlayerMovementMulti>();
        resetMulti = inputManager.GetComponent<ResetMulti>();
        inputManager.OnGrabbingLeft.AddListener(OnGrabLeft);
        inputManager.OnGrabbingReleaseLeft.AddListener(OnRealeseLeft);
        inputManager.OnGrabbingRight.AddListener(OnGrabRight);
        inputManager.OnGrabbingReleaseRight.AddListener(OnRealeseRight);

        currentMilestone = playGround.milestoneManager.numberOfMilestones;
        playerMovement.CmdMoveBubble(playGround.radius, nextMilestonePos.y,positionOfMilestoneIntersection, playGround.bonusOrbes, playGround.malusOrbes,playGround.fireBallOrbe, directionForBubble);
        Vector3 headSettPos = inputManager.GetComponent<XRRig>().cameraFloorOffsetObject.transform.localPosition;
        pos2.localPosition += headSettPos;
        pos1.localPosition += headSettPos;
        transform.localPosition += pos1.localPosition + Vector3.up;
        transform.localPosition -= Vector3.up;
        transform.localRotation = pos1.transform.localRotation;
        yield return new WaitForSeconds(5f);
        multiUI.grabManager = this;
        
    }
    public virtual void InitPool(GameObject authority, PlayerMovementMulti player, int v)
    {
        otherplayer = player;
        authorityToSpawn = authority;
        playerNumber = v;
        if (ScenesManagement.instance.IsLobbyScene() || ScenesManagement.instance.IsMenuScene())
            return;
        mainPool = new List<pool>();
        ScenesManagement.instance.numberOfFlocksInScene = 0;
        for (int i = 0; i < batches.Count; i++)
        {
            mainPool.Add(new pool());
            mainPool[i].floxes = new List<GameObject>();
            mainPool[i].isSelected = new List<bool>();

            for (int x = 0; x < batches[i].pieces.Count; x++)
            {
                // temp solution for attribution
                Modifier _modifier = baseModifier;
                Type type = _modifier.actions.GetType();
                var _object = GetComponent(type);
                player.InitBacth(authority,v, i, x, batches, _modifier, _object, basicMats, mainPool, out mainPool);
                floxNumber++;
            }

            for (int x = 0; x < batches[i].batchModifier.negativeModifier.Count; x++)
            {
                Modifier _modifierPiece = negativeModifiers[UnityEngine.Random.Range(0, negativeModifiers.Count)];
                Type typePiece = _modifierPiece.actions.GetType();
                var _objectPiece = GetComponent(typePiece);
                player.InitModifier(authority,v, i,x, _modifierPiece, batches[i].batchModifier.negativeModifier[x], _objectPiece, basicMats, false, mainPool, out mainPool);
            }
            for (int x = 0; x < batches[i].batchModifier.positiveModifiers.Count; x++)
            {
                Modifier _modifierPiece = positiveModifiers[UnityEngine.Random.Range(0, positiveModifiers.Count)];
                Type typePiece = _modifierPiece.actions.GetType();
                var _objectPiece = GetComponent(typePiece);
                player.InitModifier(authority,v, i,x+2, _modifierPiece, batches[i].batchModifier.positiveModifiers[x], _objectPiece, basicMats, true, mainPool, out mainPool);
            }
            mainPool[i].isEmptyModifier = true;
           
        }

        player.InitFireBall(authority, fireBallPrefab, fireBallPrefabOut);
        numberOfPool = 1;
        if (v == 1)
        {
            transform.localPosition = pos2.transform.localPosition;
            transform.localRotation = pos2.transform.localRotation;
            lever.transform.localRotation = Quaternion.Euler(0, 0, 0);
            lever.localPosition = new Vector3(-0.424f, -0.051f, 0.04f);
        }
         playerMovement.CmdInitGrabManager(playerMovement.gameObject, playerNumber-1);

       //multiUI.CmdInitManagers(NetworkManagerRace.instance.players);
    }

    protected override void UpdateMilestone()
    {
        nextMilestonePos = Vector3.zero;
        currentMilestone = playGround.CheckMilestones(out positionOfMilestoneIntersection, out numberOfMilestones, out nextMilestonePos);
        playerMovement.CmdChangeMilestoneValue(gameModeSolo.number, currentMilestone);
    }
    protected override void UpdateSpecial()
    {
        directionForBubble = new List<Vector3>();
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
        nextIsFireBallBatche = !nextIsFireBallBatche;
        if (fireBallNumber != null && fireBallNumber.Count > 0)
        {
            if(nextIsFireBallBatche)
                AllowFireBall();
            for (int i = 0; i < fireBallNumber.Count; i++)
            {
                if (fireBallNumber[i] != null)
                {
                    fireBallNumber[i].GetComponent<SoundReader>().Play();
                }
            }

        }
        StartCoroutine(ResetModifierCount());
     
    }
    protected override IEnumerator ResetModifierCount()
    {
        yield return new WaitForSeconds(1.6f);
        if (malusNumber != null && malusNumber.Count != 0)
            malusNumber.Clear();
        if (bonusNumber != null && bonusNumber.Count != 0)
            bonusNumber.Clear();
        if (fireBallNumber != null && fireBallNumber.Count != 0)
            fireBallNumber.Clear();
    }
    public void GetPieceFireball(XRBaseInteractor baseInteractor)
    {
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.RightHand && !isGrabRight)
            return;
        if (baseInteractor.GetComponent<HandController>().controllerNode == UnityEngine.XR.XRNode.LeftHand && !isGrabLeft)
            return;
        fireballRepresentation.gameObject.SetActive(false);

        mainPool[currentPool].isFireballUsed = true;
        XRBaseInteractable baseInteractableBonus = fireBallInstantiated.GetComponent<GrabbableObject>();

        var grabableBonus = fireBallInstantiated.GetComponent<GrabablePhysicsHandler>();
        grabableBonus.enabled = true;

        StartCoroutine(WaiToSelect(baseInteractableBonus, baseInteractor, grabableBonus, true));
        fireBallInstantiated.transform.position = baseInteractor.transform.position;

        grabableBonus.OnHitGround.AddListener(RespawnFireball);
        return;

    }
    private void AllowFireBall()
    {
        if (!mainPool[currentPool].isFireballUsed)
        {
            fireballRepresentation.gameObject.SetActive(true);
            fireballRepresentation.ApplyVisual(this, fireBallInstantiated.GetComponent<MeshForDispenser>().mesh, fireBallInstantiated.GetComponent<MeshRenderer>().material);
            directionForBubble.Add(fireballRepresentation.transform.position);
            multiUI.CmdSelectFireBall();
        }

    }
    protected override void FreezOfList(List<GameObject> flocksToFreez, int indeOfPool)
    {
        if (flocksToFreez == null)
            return;
        for (int i = 0; i < flocksToFreez.Count; i++)
        {
            resetMulti.AddFreezFlock(flocksToFreez[i],indeOfPool,i);
        }
        playGround.soundReader.Play("Freez");
    }
    protected override void UpdateBubble()
    {
        if (!doOnce)
        {
            doOnce = true;
            return;
        }
        if(directionForBubble ==null || directionForBubble.Count== 0)
            directionForBubble = new List<Vector3>();
        playerMovement.CmdMoveBubble( playGround.radius, nextMilestonePos.y,positionOfMilestoneIntersection, playGround.bonusOrbes, playGround.malusOrbes, playGround.fireBallOrbe, directionForBubble); 
    }
    private void RespawnFireball(GameObject _object, Vector3 initPos, bool isGrab)
    {
        if (!isGrab)
        {

            _object.GetComponent<Rigidbody>().useGravity = false;
            _object.GetComponent<Rigidbody>().velocity = Vector3.zero;
            _object.transform.rotation = Quaternion.identity;
            _object.transform.position = new Vector3(300 + 5 * 5, 300 + 5 * 5, 300);
            var grabableBonus = fireBallInstantiated.GetComponent<GrabablePhysicsHandler>();
            grabableBonus.OnHitGround.RemoveListener(RespawnFireball);
            grabableBonus.enabled = false;
            mainPool[currentPool].isFireballUsed = false;
            StartCoroutine(playGround.GetComponentInChildren<FireballManager>().TryClosePortal());
            AllowFireBall();

        }
    }

    public override void DestroyFlock(GameObject flock, int indexOfPool)
    {
        if (mainPool[indexOfPool].floxes.Contains(flock))
        {
            int indexOfFlock = mainPool[indexOfPool].floxes.IndexOf(flock);
            StartCoroutine(  flock.GetComponent<DissolveFlox>().StartDissolve(default, Vector3.zero, true, null, false, this));
            
            flock.GetComponent<GrabbableObject>().enabled = true;
            flock.GetComponentInChildren<FloxExpressionManager>().isFrozen = false;
            flock.GetComponent<GrabablePhysicsHandler>().m_rgb.useGravity = false;
            flock.GetComponent<GrabablePhysicsHandler>().m_rgb.velocity = Vector3.zero;
            flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
            currentFloxNumber++;
            floxCount.text = currentFloxNumber.ToString() + " /" + floxNumber.ToString();
            flock.GetComponent<Rigidbody>().useGravity = false;
            mainPool[indexOfPool].isSelected[indexOfFlock] = false;
            mainPool[indexOfPool].isEmpty = false;
            batches[indexOfPool].isEmpty = false;

        }
        else if (mainPool[indexOfPool].malus.Contains(flock))
        {
            int indexOfFlock = mainPool[indexOfPool].malus.IndexOf(flock);
            StartCoroutine(flock.GetComponent<DissolveFlox>().StartDissolve(default, Vector3.zero, true,null,false,this));
            flock.GetComponent<GrabbableObject>().enabled = true;
            flock.GetComponentInChildren<FloxExpressionManager>().isFrozen = false;
            flock.GetComponent<GrabablePhysicsHandler>().m_rgb.useGravity = false;
            flock.GetComponent<GrabablePhysicsHandler>().m_rgb.velocity = Vector3.zero;
            flock.GetComponent<GrabablePhysicsHandler>().enabled = false;
            if (mainPool[indexOfPool].isSelectedModifier != null)
                mainPool[indexOfPool].isSelectedModifier[indexOfFlock] = false;
            mainPool[indexOfPool].isEmptyModifier = true;
            batches[indexOfPool].isEmpty = false;

        }

        else if (mainPool[indexOfPool].bonus.Contains(flock))
        {
            int indexOfFlock = mainPool[indexOfPool].bonus.IndexOf(flock);
            StartCoroutine(flock.GetComponent<DissolveFlox>().StartDissolve(default, Vector3.zero, true, null, false, this));
            flock.GetComponent<GrabbableObject>().enabled = true;
            flock.GetComponentInChildren<FloxExpressionManager>().isFrozen = false;
            flock.GetComponent<GrabablePhysicsHandler>().m_rgb.useGravity = false;
            flock.GetComponent<GrabablePhysicsHandler>().m_rgb.velocity = Vector3.zero;
            flock.GetComponent<GrabablePhysicsHandler>().enabled = false;

            if (mainPool[indexOfPool].isSelectedModifier!= null)
            mainPool[indexOfPool].isSelectedModifier[indexOfFlock] = false;
            mainPool[indexOfPool].isEmptyModifier = true;
            batches[indexOfPool].isEmpty = false;

        }
    }
    public void Destroy(GameObject flock)
    {
        resetMulti.GetComponent<ResetMulti>().StopAllCoroutines();
        if (!flock)
            return;
        if (mainPool[currentPool].floxes.Contains(flock))
        {
            mainPool[currentPool].floxes.Remove(flock);
        }
        else if (mainPool[currentPool].bonus.Contains(flock))
        {
            mainPool[currentPool].malus.Remove(flock);
        }
        else if (mainPool[currentPool].malus.Contains(flock))
        {
            mainPool[currentPool].bonus.Remove(flock);
        }
        flock.transform.position = flock.GetComponent<GrabablePhysicsHandler>().initPos;
        var rgb = flock.GetComponent<Rigidbody>();
            flock.GetComponent<Rigidbody>().isKinematic = true;
            flock.GetComponent<Rigidbody>().velocity= Vector3.zero;
            flock.GetComponent<Rigidbody>().angularVelocity= Vector3.zero;
            flock.GetComponent<Rigidbody>().useGravity= false;
            flock.GetComponent<Rigidbody>().isKinematic = false;
        flock.GetComponent<GrabablePhysicsHandler>().OnUnFreez();
    }

    public void AddFlox()
    {
        if (currentFloxNumber == floxNumber)
        {
            floxNumber++;
            currentFloxNumber = (int)floxNumber;
        }
        else
            currentFloxNumber++;

        floxCount.text = currentFloxNumber.ToString() + " /" + floxNumber.ToString();
    }
    public override void FreezHotPotato(GameObject flox)
    {
        int value = 100;
        value = mainPool[currentPool].malus.IndexOf(flox);
        if (value != 100)
            resetMulti.AddFreezFlock(flox, currentPool, value, true);
        else
            Debug.Log("its a bug");
    }

}
