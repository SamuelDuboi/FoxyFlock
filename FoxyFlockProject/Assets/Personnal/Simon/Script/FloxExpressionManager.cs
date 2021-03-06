using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloxExpressionManager : MonoBehaviour
{
    public Material floxMat;
    public MeshRenderer flox;
    public Rigidbody rb;
    public GrabbableObject GrabbableFlox;
    public Animator floxanimator;
    public SoundReader floxReader;

    public bool isFrozen;
    public bool baseFace;
    public bool sleepFace;
    public bool panicFace;
    public List<string> floxBase;
    public List<string> sleepBase;
    public List<string> fearBase;

    public float moveThreshold;
   [HideInInspector] public int _tempInt;

    void Start()
    {
        baseFace = true;
        moveThreshold = 0.1f;
    }

    void Update()
    {
        if (floxReader.source.isPlaying)
            return;

        if (floxMat.GetFloat("IsFrozen") == 1 && isFrozen == false)
        {
            isFrozen = true;
        }

        if (isFrozen && sleepFace == false)
        {
            sleepFace = true;
            _tempInt = Random.Range(0, 5);
            floxReader.clipName = sleepBase[_tempInt];
            floxReader.Play();

            baseFace = false;
            floxanimator.SetBool("Base", false);
            panicFace = false;
            floxanimator.SetBool("Panic", false);
            floxanimator.SetBool("Sleep", true);

        }

        else if (panicFace == false && isFrozen == false && ((GrabbableFlox != null && GrabbableFlox.isGrab == true) || (rb != null && rb.velocity.magnitude >= moveThreshold)))
        {
            
            _tempInt = Random.Range(0, 5);
            floxReader.clipName = fearBase[_tempInt];
            floxReader.Play();
            baseFace = false;
            floxanimator.SetBool("Base", false);
            floxanimator.SetBool("Sleep", false);
            panicFace = true;
            floxanimator.SetBool("Panic", true);
        }
        else if (isFrozen == false && (rb != null && rb.velocity.magnitude <= moveThreshold) && baseFace == false && GrabbableFlox != null && GrabbableFlox.isGrab == false)
        {
            _tempInt = Random.Range(0, 5);
            floxReader.clipName = floxBase[_tempInt];

            floxReader.Play();
            panicFace = false;
            floxanimator.SetBool("Panic", false);
            floxanimator.SetBool("Sleep", false);
            baseFace = true;
            floxanimator.SetBool("Base", true);

        }
    }
}
