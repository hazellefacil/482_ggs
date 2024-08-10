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

    private int computerReload; //ammo count for computer
    private int userReload; //ammo count for user

    private float actionTimer;
    private float interval = 10f; // 10-second interval

    private float animationLength = 10f;

    private string pendingGesture;

    void Start()
    {
        client = new TcpClient("localhost", 65432);
        stream = client.GetStream();

        byte[] data = new byte[256];
        int bytesRead = stream.Read(data, 0, data.Length);
        string capOpenedFlag = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();

        if (capOpenedFlag == "1")
        {
            mAnimator = GetComponent<Animator>();
            mAnimator.Play(idleAnimation); 
            computerReload = 0;
            userReload = 0;
            actionTimer = 0f;
            pendingGesture = null; // Store the gesture until the timer elapses
        }
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
                Debug.Log("pendingGesture: " + pendingGesture);
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
            Debug.Log("Player wins");
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
            Debug.Log("Draw - Keep Going!");
            userReload = 0;
            computerReload = 0;
        }
        if (gesture == "reload")
        {
            Debug.Log("Player Reload");
            if(userReload == 1){
                Debug.Log("Player Already Has Ammo!");
            }
            userReload = 1;
        }
        if (nextAction == "reload")
        {
            Debug.Log("Computer Reload");
            if(computerReload == 1){
                Debug.Log("Computer Already Has Ammo!");
            }
            computerReload = 1;
        }
        if (nextAction == "shoot")
        {
            Debug.Log("Computer Shoots");
            if(computerReload == 0){
                Debug.Log("Computer Shoots Nothing - Must Reload Again!");
            }
            computerReload = 0;
        }
        if (gesture == "shoot")
        {
            Debug.Log("Player Shoots");
            if(computerReload == 0){
                Debug.Log("Player Shoots Nothing - Must Reload Again!");
            }
            userReload = 0;
        }
    }

    void UpdateAction()
    {
        Debug.Log("Updating Action");
        int newAnimationIndex; 
        do
        {
            newAnimationIndex = UnityEngine.Random.Range(0,actions.Length); 
            Debug.Log("Choosing animation index");
        } while (newAnimationIndex == currentAction);
        currentAction = newAnimationIndex; 
        nextAction = actions[currentAction]; 
        Debug.Log("Current action is: " + actions[currentAction]);
        mAnimator.Play(nextAction); 
        float animationLength = mAnimator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(ReturnToIdle), animationLength);
    }

    void ReturnToIdle()
    {
        mAnimator.Play(idleAnimation); 
    }
}
