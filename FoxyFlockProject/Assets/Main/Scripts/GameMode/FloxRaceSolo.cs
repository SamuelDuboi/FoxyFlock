using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloxRaceSolo : GameModeSolo
{
    public float limitHeight;
    private float limitDiameter;
    public float timeToWin;
    private float timeAboveHeight;
    public Limits winLimit;
    [HideInInspector] public Vector3 p;
    void Start()
    {
        p = winLimit.transform.position;
        p.y = tableTransform.position.y + limitHeight;
        winLimit.transform.position = p;
        UIGlobalManager.instance.SetGameMode("Flock Race","0");
        //winLimit.gameObject.diameter = limitDiameter;
        //winLimit.gameObject.diameter = limitDiameter;

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (winLimit.triggered && hands.inPlayground == false)
        {
            tip = "can win";
            winLimit.GetComponent<MeshRenderer>().material = winLimit.winMat;
            timeAboveHeight += Time.deltaTime;
        } else if (winLimit.triggered && hands.inPlayground == true)
        {
            tip = "hands out";
            UIGlobalManager.instance.SetRulesMode(tip);
            timeAboveHeight = 0;
            winLimit.GetComponent<MeshRenderer>().material = winLimit.defeatMat;
        }
        else
        {
            tip = "try too reach height";
            UIGlobalManager.instance.SetRulesMode(tip);
            timeAboveHeight = 0;
            winLimit.GetComponent<MeshRenderer>().material = winLimit.baseMat;
        }
        if (timeAboveHeight >= timeToWin)
        {
            if (number == 0)
                playerMovement.CmdWin1();
            else
                playerMovement.CmdWin2();
            Destroy(this);
        }
    }
}
