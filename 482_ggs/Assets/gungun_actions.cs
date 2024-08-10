using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public class tester_actions : MonoBehaviour
{

    private Animator mAnimator;
    private int currentAction; 
    private string nextAction;
    private string[] actions = { "block", "reload", "shoot" };
    private string idleAnimation = "idle";
    TcpClient client;
    NetworkStream stream;
    private bool computerReload = false;
    private bool userReload = false;

    private bool gameContinues;

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
            mAnimator.SetBool("gameContinues", true);
            gameContinues = true;
            // Wait for the gesture from Python
            StartCoroutine(WaitForGesture());
        }
    }
    
    IEnumerator WaitForGesture()
    {
        while (gameContinues)
        {
            if (gameContinues == false){
                mAnimator.Play(idleAnimation); 
            }
            else if (stream.DataAvailable)
            {
                byte[] data = new byte[256];
                int bytesRead = stream.Read(data, 0, data.Length);
                string gesture = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();

                // Send "ready" signal back to Python
                byte[] readyResponse = Encoding.ASCII.GetBytes("ready");
                stream.Write(readyResponse, 0, readyResponse.Length);

                // Start the 5-second countdown
                yield return StartCoroutine(StartCountdown(10));

                // Update the computer's action
                UpdateAction();
                ProcessGesture(gesture);

                // If the game has ended, break out of the loop
                if (!gameContinues)
                {
                    mAnimator.SetBool("gameContinues", false);
                    break;
                }
                // Send the computer's move back to Python
                byte[] moveResponse = Encoding.ASCII.GetBytes(nextAction);
                stream.Write(moveResponse, 0, moveResponse.Length);
            }


            yield return null;
        }
        mAnimator.SetBool("gameContinues", false);
        mAnimator.Play(idleAnimation);
        Debug.Log("Game over.");
    }

    IEnumerator StartCountdown(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            Debug.Log($"Revealing actions in {i} seconds...");
            yield return new WaitForSeconds(1);
        }
    }

    void ProcessGesture(string gesture)
    {
        if(gesture == "reload"){
            userReload = true;
            if(nextAction == "reload"){
                computerReload = true;
            }
            else if(nextAction == "shoot"){
                if(computerReload == true){
                    Debug.Log("Computer wins!");
                    gameContinues = false;
                    mAnimator.SetBool("gameContinues", false);
                }
                else{
                    Debug.Log("Computer miss!");
                }
            }

            else if(nextAction == "block"){
                Debug.Log("Draw!");
            }
        }

        else if(gesture == "shoot"){
            if(nextAction == "reload"){
                if(userReload == true){
                    Debug.Log("User wins");
                    gameContinues = false;
                    mAnimator.SetBool("gameContinues", false);
                }
                else{
                    Debug.Log("Player miss!");
                }
            }
            else if(nextAction == "shoot"){   
                if(userReload == true){
                    if(computerReload == true){
                        Debug.Log("Draw!");
                        gameContinues = false;
                        mAnimator.SetBool("gameContinues", false);
                    }
                    else{
                        Debug.Log("User wins - computer has no ammo!");
                        gameContinues = false;
                        mAnimator.SetBool("gameContinues", false);
                    }
                }

                else if(userReload == false){
                    if(computerReload == true){
                        Debug.Log("Computer wins - user has no ammo!");
                        gameContinues = false;
                        mAnimator.SetBool("gameContinues", false);
                    }
                    else{
                        Debug.Log("Draw!");
                        gameContinues = false;
                        mAnimator.SetBool("gameContinues", false);
                    }
                }
            }
            else if(nextAction == "block"){
                Debug.Log("User was blocked by the computer! Reload Again!");
                userReload = false;
            }
        }
        // autodefaulted none = block!
        else if(gesture == "block" || gesture == "none"){
            if(nextAction == "reload"){
                computerReload = true;
            }
            else if(nextAction == "shoot"){
                Debug.Log("Computer was blocked by the user!");
                computerReload = false;
            }
            else if(nextAction == "block"){
                
            }
            
        }
        /*
        // check reloads regardless
        if (nextAction == "reload")
        {
            computerReload = true;
        }
        if (gesture == "reload")
        {
            userReload = true;
        }


        if (gesture == nextAction || gesture == "block" || nextAction == "block")
        {
            Debug.Log("draw");
        }
        else if (gesture == "shoot" && nextAction == "reload")
        {
            if (userReload)
            {
                Debug.Log("user wins");
                gameContinues = false;
                mAnimator.SetBool("gameContinues", false);
            }
            else
            {
                Debug.Log("miss!");
            }
        }
        else if (nextAction == "shoot")
        {
            if (computerReload)
            {
                Debug.Log("computer wins");
                gameContinues = false;
                mAnimator.SetBool("gameContinues", false);
            }
            else
            {
                Debug.Log("miss!");
            }
        }
        else if (gesture == "shoot")
        {
            Debug.Log("user wins");
            gameContinues = false;
            mAnimator.SetBool("gameContinues", false);
        }*/
    }
    void UpdateAction()
    {
        int newAnimationIndex; 
        do
        {
            newAnimationIndex = UnityEngine.Random.Range(0, actions.Length); 
        } while (newAnimationIndex == currentAction);
        currentAction = newAnimationIndex; 
        nextAction = (string)actions[currentAction]; 
        mAnimator.Play(nextAction); 
        float animationLength = mAnimator.GetCurrentAnimatorStateInfo(0).length;
        Invoke(nameof(ReturnToIdle), animationLength);
    }
    void ReturnToIdle()
    {
        mAnimator.Play(idleAnimation); 
    }

    void OnDestroy()
    {
        stream.Close();
        client.Close();
    }
}