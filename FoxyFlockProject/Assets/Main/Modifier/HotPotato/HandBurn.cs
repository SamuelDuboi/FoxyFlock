using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum HeatState
{
    cool,
    burning,
    burned
}

public class HandBurn : MonoBehaviour
{
    [SerializeField] private float heatMaxValue = 100f;
    [SerializeField] private float burningSpeed = 10f;
    [SerializeField] private float coolingSpeed = 10f;
    [SerializeField] private float wiggleCoolingScale = 2f;

    private float heatCurrentValue = 0f;
    [HideInInspector] public float heatPourcentage = 0f;
    private float lastFrameHeatPourcentage = 0f;
    private Vector3 lastFrameTransform;

    private XRDirectInteractor interactor;
    private SkinnedMeshRenderer handRenderer;
    private MaterialPropertyBlock propBlock;
    [HideInInspector] public bool doOnce;
    private bool playOnceCool;
    private SoundReader soundReader;
    private HandController handController;
    public float wigglePower;

    public HeatState heatState =  HeatState.cool;

    private IEnumerator Start()
    {
        lastFrameTransform = this.transform.localPosition;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(5.0f);

        propBlock = new MaterialPropertyBlock();
        handController = GetComponent<HandController>();
        interactor = this.GetComponent<XRDirectInteractor>();

        handRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
        soundReader = GetComponent<SoundReader>();
      

    }

    private void FixedUpdate()
    {
        heatPourcentage = heatCurrentValue / heatMaxValue;

        if (heatState != HeatState.cool)
        {
            UpdateMatBurnValue();
            CoolEvent();
            lastFrameHeatPourcentage = heatPourcentage;
        }

        wigglePower = wiggleStrengh();
    }

    private void UpdateMatBurnValue()
    {
        if(handRenderer == null)
        handRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();

        if (handRenderer != null)
        {
            //Recup Data
            handRenderer.GetPropertyBlock(propBlock);
            //EditZone
            propBlock.SetFloat("Burn", heatPourcentage);
            //Push Data
            handRenderer.SetPropertyBlock(propBlock);
        }
    }

    public void BurnEvent(GrabbableObject flockInteractable)
    {
        heatState = HeatState.burning;
        if (!doOnce)
        {
            doOnce = true;
            soundReader.secondClipName = "HotFloxHolding";
            soundReader.source.loop = true;
            soundReader.PlaySeconde();
        }
        heatCurrentValue += Time.deltaTime* burningSpeed;
        lastFrameTransform = this.transform.localPosition;
        if (heatCurrentValue >= heatMaxValue)
        {
            heatCurrentValue = heatMaxValue;
            heatState = HeatState.burned;
            soundReader.source.loop = false;
            soundReader.StopSound();
            soundReader.ThirdClipName = "HotForceRelease";
            soundReader.PlayThird();
            doOnce = false;
            InteractionManager.instance.SelectExit(interactor, flockInteractable);
            //flockInteractable.transform.position = interactor.transform.position;
            flockInteractable.GetComponent<Rigidbody>().velocity = Vector3.zero;
            flockInteractable.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            interactor.allowSelect = false;
            interactor.allowHover = false;
            handController.enableInputActions = false;
            StartCoroutine(WaitToCoolSound());
        }
    }
    IEnumerator WaitToCoolSound()
    {
        yield return new WaitForSeconds(0.4f);
        playOnceCool = false;
    }
    private void CoolEvent()
    {
        if (heatState != HeatState.burning)
        {
            if (heatCurrentValue > 0)
            {
                if (!playOnceCool)
                {
                    playOnceCool = true;
                    soundReader.ThirdClipName = "HotCool";
                    soundReader.source.loop = true;
                    soundReader.PlayThird();
                }
                heatCurrentValue -= Time.deltaTime*( coolingSpeed + (coolingSpeed * wiggleStrengh()));
                
                if (heatCurrentValue <= 0)
                {
                    if (heatState == HeatState.burned)
                    {
                        interactor.allowHover = true;
                        soundReader.source.Stop();
                    }

                    heatState = HeatState.cool;
                    handController.enableInputActions = true;
                    interactor.allowSelect = true;
                    interactor.allowHover = true;
                }
            }  
        }
    }
    public void DropEvent()
    {
        heatState = HeatState.burned;
        soundReader.source.loop = false;
        soundReader.StopSound();
        doOnce = false;
        playOnceCool = false;
    }

    private float wiggleStrengh()
    {
        Vector3 lastFramePosition = lastFrameTransform;
      //  Quaternion lastFrameRotation = lastFrameTransform;
         float distanceCheck =Vector3.Distance( transform.localPosition, lastFramePosition);
     //   float rotationCheck = Mathf.Abs(Quaternion.Angle(transform.localRotation, lastFrameRotation)); //I don't know if quaternion.angle can be negative so i take the absolute as a security

        float wiggle = (distanceCheck ) * wiggleCoolingScale;

            lastFrameTransform = transform.localPosition;

        return wiggle;
    }
}


