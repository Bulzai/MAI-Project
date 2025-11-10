using UnityEngine;

public enum HealthTier { Full, Half, Low }

[System.Serializable]
public struct MotionClips
{
    public AnimationClip Idle;
    public AnimationClip Run;
    public AnimationClip Jump;
    public AnimationClip Land;
    public AnimationClip Wall;
    public AnimationClip Death;
}

[CreateAssetMenu(menuName = "Characters/Character Animation Library")]
public class CharacterAnimationLibrary : ScriptableObject
{
    [Header("Normal")]
    public MotionClips NormalFull;
    public MotionClips NormalHalf;
    public MotionClips NormalLow;

    [Header("On Fire")]
    public MotionClips FireFull;
    public MotionClips FireHalf;
    public MotionClips FireLow;
}
