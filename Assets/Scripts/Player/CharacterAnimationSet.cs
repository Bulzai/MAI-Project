using UnityEngine;

[CreateAssetMenu(menuName = "Characters/CharacterAnimationSet")]
public class CharacterAnimationSet : ScriptableObject
{
    public RuntimeAnimatorController fullHealth;
    public RuntimeAnimatorController halfHealth;
    public RuntimeAnimatorController lowHealth;
    public RuntimeAnimatorController fullHealthFire;
    public RuntimeAnimatorController halfHealthFire;
    public RuntimeAnimatorController lowHealthFire;
}
