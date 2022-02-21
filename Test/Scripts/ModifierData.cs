using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MODIFIER_TYPE { HEALTH, MANA, ATTACK}

[CreateAssetMenu(menuName = "Test/Cards/Card Modifier")]
public class ModifierData : ScriptableObject
{
    public bool IsOverride;
    public int Value;
    public MODIFIER_TYPE Type;
}
