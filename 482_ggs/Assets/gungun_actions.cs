using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gungun_actions : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator mAnimator;
    private int currentAction; 
    private string[] actions = {"block","reload","shoot"};
    private string idleAnimation = "idle";

    void Start()
    {
        mAnimator = GetComponent<Animator>();
        mAnimator.Play(idleAnimation); 
        Invoke(nameof(UpdateAction), 3f);
    }

    void UpdateAction()
    {
        int newAnimationIndex; 
        do
        {
            newAnimationIndex = Random.Range(0,actions.Length); 
        } while (newAnimationIndex == currentAction);
        currentAction = newAnimationIndex; 
        string chosenAction = actions[currentAction]; 
        mAnimator.Play(chosenAction); 
        float animationLength = mAnimator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(ReturnToIdle), animationLength);
    }

    void ReturnToIdle()
    {
        mAnimator.Play(idleAnimation); 
        Invoke(nameof(UpdateAction), 3f);
    }

}
