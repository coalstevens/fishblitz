using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdingWinFrame : MonoBehaviour
{
    [SerializeField] private Animator _nice;
    [SerializeField] private SpriteRenderer _birdIcon;

    public void PlayWin(Bird _bird) {
        gameObject.SetActive(true);
        _birdIcon.sprite = _bird.Icon;
        StartCoroutine(WaitForAnimationToEnd("NICE!_Clip"));
    }

    private IEnumerator WaitForAnimationToEnd(string animationName)
    {
        _nice.Play(animationName);

        AnimatorStateInfo stateInfo = _nice.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1f)
        {
            yield return null;
            stateInfo = _nice.GetCurrentAnimatorStateInfo(0);
        }

        gameObject.SetActive(false);
    }
}
