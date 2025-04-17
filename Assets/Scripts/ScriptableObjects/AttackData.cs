using UnityEngine;

[CreateAssetMenu(menuName = "Combat/AttackData")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public AnimationClip clip;
    public int damage;
    public float comboWindow;  // time to chain next attack
    public AttackData nextCombo;  // next attack in chain
}
