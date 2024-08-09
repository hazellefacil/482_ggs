using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public class gungun_actions : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator mAnimator;
    private int currentAction; 
    private string nextAction;
    private string[] actions = {"block","reload","shoot"};
    private string idleAnimation = "idle";
    TcpClient client;
    NetworkStream stream;

    private int computerReload;
    private int userReload;

    void Start()
    {
        client = new TcpClient("localhost", 65432);
        stream = client.GetStream();
        mAnimator = GetComponent<Animator>();
        mAnimator.Play(idleAnimation); 
        Invoke(nameof(UpdateAction), 1f);
        computerReload = 0;
        userReload = 0;
    }

    void Update()
    {
        if (stream.DataAvailable)
        {
            byte[] data = new byte[256];
            int bytesRead = stream.Read(data, 0, data.Length);
            string gesture = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();
            ProcessGesture(gesture);
        }
    }

    void ProcessGesture(string gesture)
    {
        if (gesture == nextAction || gesture == "block" || nextAction == "block")
        {
            computerReload = 1;
        }
        else if (gesture == "shoot" && nextAction == "reload")
        {
            computerReload = 0;
        }
        else if (gesture == "shoot")
        {
            userReload = 0;
        }
    }

    void UpdateAction()
    {
        int newAnimationIndex; 
        do
        {
            newAnimationIndex = UnityEngine.Random.Range(0,actions.Length); 
        } while (newAnimationIndex == currentAction);
        currentAction = newAnimationIndex; 
        nextAction = actions[currentAction]; 
        mAnimator.Play(nextAction); 
        float animationLength = mAnimator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(ReturnToIdle), animationLength);
    }

    void ReturnToIdle()
    {
        mAnimator.Play(idleAnimation); 
        Invoke(nameof(UpdateAction), 1f);
    }

}
