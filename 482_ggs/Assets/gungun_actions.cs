using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public class gungun_actions : MonoBehaviour
{
    private Animator mAnimator;
    private int currentAction; 
    private string nextAction;
    private string[] actions = {"block","reload","shoot"};
    private string idleAnimation = "idle";
    TcpClient client;
    NetworkStream stream;

    private int computerReload;
    private int userReload;

    private float actionTimer;
    private float interval = 10f; // 10-second interval

    private string pendingGesture;

    void Start()
    {
        client = new TcpClient("localhost", 65432);
        stream = client.GetStream();
        mAnimator = GetComponent<Animator>();
        mAnimator.Play(idleAnimation); 
        computerReload = 0;
        userReload = 0;
        actionTimer = 0f;
        pendingGesture = null; // Store the gesture until the timer elapses
    }

    void Update()
    {
        actionTimer += Time.deltaTime;

        if (stream.DataAvailable)
        {
            byte[] data = new byte[256];
            int bytesRead = stream.Read(data, 0, data.Length);
            string gesture = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();
            pendingGesture = gesture; // Save the gesture but don't process it immediately
        }

        if (actionTimer >= interval)
        {
            // Process the gesture and update the action after the 10-second interval
            if (pendingGesture != null)
            {
                ProcessGesture(pendingGesture);
                pendingGesture = null; // Reset after processing
            }

            UpdateAction(); // Update the computer's action
            actionTimer = 0f; // Reset the timer
        }
    }

    void ProcessGesture(string gesture)
    {
        if ((userReload == 1) && (gesture == "shoot"))
        {
            Debug.Log("User wins");
            userReload = 0;
            computerReload = 0;
        }
        else if ((computerReload == 1) && (nextAction == "shoot") && gesture == "reload")
        {
            Debug.Log("Computer wins");
            userReload = 0;
            computerReload = 0;
        }
        else if ((userReload == 1) && (gesture == "shoot") && (nextAction == "shoot") && (computerReload == 1))
        {
            Debug.Log("Draw");
            userReload = 0;
            computerReload = 0;
        }
        else if (gesture == "reload")
        {
            Debug.Log("Player Reload");
            userReload = 1;
        }
        else if (nextAction == "reload")
        {
            Debug.Log("Computer Reload");
            computerReload = 1;
        }
        else if (nextAction == "shoot")
        {
            Debug.Log("Computer Shoot");
            computerReload = 0;
        }
        else if (gesture == "shoot")
        {
            Debug.Log("Player Shoot");
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
    }
}
