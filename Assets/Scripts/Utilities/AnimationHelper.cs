using UnityEngine;

public static class AnimationHelper
{
    public static float GetClipLength(this Animator animator, string clipName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        if (ac == null)
        {
            Debug.LogWarning($"Animator on {animator.gameObject.name} has no runtime controller");
            return 0f;
        }

        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        Debug.LogWarning($"Clip '{clipName}' not found in animator on {animator.gameObject.name}");
        return 0f;
    }
}
