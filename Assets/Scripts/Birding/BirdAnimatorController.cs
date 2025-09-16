using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BirdAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _flappingAnimationSpeed;
    [SerializeField, UnityEngine.Range(0, 1f)] private float _glideFrameInNormalizedTime = 0.435f;
    [SerializeField] private Logger _logger = new();
    private BirdBrain _bird;

    public void Initialize()
    {
        _logger.Info("Bird animator initialized.");
        _bird = GetComponent<BirdBrain>();

        Assert.IsNotNull(_animator);
        Assert.IsNotNull(_bird);

        OverrideBirdAnimatorClips(_animator, _bird.SpeciesData);
    }

    public void PlayFlying()
    {
        Play(_bird.InstanceData.IsTagged.Value ? "Flying_Tagged" : "Flying", 1f);
    }

    public void PlayIdle()
    {
        Play(_bird.InstanceData.IsTagged.Value ? "Idle_Tagged" : "Idle", 1f);
    }

    public void PlaySwimming()
    {
        Play(_bird.InstanceData.IsTagged.Value ? "Swimming_Tagged" : "Swimming", 1f);
    }

    public void PlayIdleSwimming()
    {
        Play(_bird.InstanceData.IsTagged.Value ? "Idle_Swimming_Tagged" : "Idle_Swimming", 1f);
    }

    public void PlayWalking()
    {
        Play(_bird.InstanceData.IsTagged.Value ? "Walking_Tagged" : "Walking", 1f);
    }

    public void PlayTwoHop()
    {
        _logger.Info("Playing two hop animation");
        StartCoroutine(PlayTwoHop(_bird.GetComponent<Rigidbody2D>()));
    }

    public void PlayFlapping()
    {
        Play(_bird.InstanceData.IsTagged.Value ? "Flying_Tagged" : "Flying", _flappingAnimationSpeed);
    }

    public void PlayGliding()
    {
        _logger.Info("Playing gliding animation");
        string stateName = _bird.InstanceData.IsTagged.Value ? "Flying_Tagged" : "Flying";
        _animator.Play(stateName, 0, _glideFrameInNormalizedTime);
        _animator.speed = 0f;
    }

    private void Play(string animation, float speed)
    {
        _animator.speed = speed;
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
        {
            float randomStart = Random.value; // 0.0 to 1.0 normalized time
            _logger.Info($"Playing {animation} animation");
            _animator.Play(animation, 0, randomStart);
        }
    }

    private void OverrideBirdAnimatorClips(Animator animator, BirdSpeciesData speciesData)
    {
        if (animator == null || speciesData == null || speciesData.BaseAnimatorController == null)
        {
            Debug.LogError("Animator or species data is null. Cannot create animator for bird.");
            return;
        }

        var overrideController = new AnimatorOverrideController(speciesData.BaseAnimatorController);
        var currentOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(currentOverrides);

        // Create a lookup of existing clip names -> original clips (for safety)
        var clipLookup = new Dictionary<string, AnimationClip>();
        foreach (var pair in currentOverrides)
        {
            if (pair.Key != null && !clipLookup.ContainsKey(pair.Key.name))
            {
                clipLookup[pair.Key.name] = pair.Key;
            }
        }

        // Apply overrides based on species data clip names
        foreach (var clipOverride in speciesData.AnimationClips)
        {
            if (string.IsNullOrEmpty(clipOverride.OriginalName) || clipOverride.OverrideClip == null)
                continue;

            if (clipLookup.TryGetValue(clipOverride.OriginalName, out var originalClip))
            {
                overrideController[originalClip] = clipOverride.OverrideClip;
            }
            else
            {
                Debug.LogWarning($"Original animation '{clipOverride.OriginalName}' not found in base animator for {speciesData.SpeciesName}.");
            }
        }

        animator.runtimeAnimatorController = overrideController;
    }

    private IEnumerator PlayTwoHop(Rigidbody2D rb)
    {
        string baseAnimationName = "Two Hop";
        string additiveAnimationName = "Idle";
        int returnState = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        _animator.Play(baseAnimationName, 0, 0f);
        _animator.Play(additiveAnimationName, 1, 0f);

        bool completed = true;

        // Wait until the animation actually starts playing
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(baseAnimationName))
            yield return null;

        // Get the length of the current animation
        float animLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        float timer = 0f;

        while (timer < animLength)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (!stateInfo.IsName(baseAnimationName))
            {
                _logger.Info("Two hop animation interrupted");
                completed = false;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (completed)
        {
            _logger.Info("Animation completed. Returning to state");
            _animator.Play(returnState);
        }

        rb.linearVelocity = Vector2.zero; 
    }

    public void MatchAnimationToFacingDirection(CompassDirection direction)
    {
        transform.localScale = new Vector3
        (
            direction == CompassDirection.West ?
                Mathf.Abs(transform.localScale.x) :
                -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }
}
