using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "NewCharacterStats",order =0)]
public class CharacterStats : ScriptableObject
{
    
    public float xPower = 5;
    public float RotateXOffset = 0.01f;
    public float RotateYOffset = 0.02f;
    public float yPower = -2;
    public float upOffset = 0.01f;
    public float zPower = 2;
    public float forwardZOffset = 0.01f;
    public float forwardYOffset = 0.02f;
    public float snapTurnAngle = 15f;
    public InputHelpers.Button fingerButton = InputHelpers.Button.Trigger;
    public InputHelpers.Button moveTrigger = InputHelpers.Button.Trigger;
    public float minTriggerTreshold = 0.5f;
    public InputHelpers.Button gripBtton= InputHelpers.Button.Grip;
    public InputHelpers.Button spawnButton= InputHelpers.Button.SecondaryButton;
}
