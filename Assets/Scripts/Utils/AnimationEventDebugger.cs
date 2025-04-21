using UnityEngine;

public class AnimationEventDebugger : MonoBehaviour
{
    public Animator animator;

    public void OnAnimationStarted()
    {
        if (animator == null)
        {
            Debug.LogError("Animator reference not assigned!");
            return;
        }

        // Get the current AnimatorStateInfo from layer 0
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length == 0)
        {
            Debug.LogWarning("No animation clips found on the current animator state.");
            return;
        }

        AnimationClip currentClip = clipInfos[0].clip;

        if (currentClip == null)
        {
            Debug.LogWarning("No AnimationClip found.");
            return;
        }

        Debug.Log($"Current Clip: {currentClip.name}");

        AnimationEvent[] events = currentClip.events;

        if (events.Length == 0)
        {
            Debug.Log("No events found on this clip.");
        }
        else
        {
            foreach (AnimationEvent evt in events)
            {
                Debug.Log($"Event: {evt.functionName} at time {evt.time} seconds (approx frame {Mathf.RoundToInt(evt.time * currentClip.frameRate)})");
            }
        }
    }
}
