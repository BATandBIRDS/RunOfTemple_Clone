using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
//                     ********************************
//                     |  Written by:  Savran Donmez  |
//                     |  Start Date:  04/12/2022     |
//                     |  Last Update: 08/12/2022     |
//                     ********************************

    enum enDirection
    {
        North,
        East,
        West
    };

    public AudioClip[] soundFXClips;
    public Text distanceScoreText;
    public Text coinScoreText;
    public Text bestDistanceText;
    public Text bestCoinScoreText;
    public GameObject deathMenu;
    public bool mobileEnabled;

    CharacterController characterController;
    Vector3 playerVector; //Player's direction.
    enDirection playerDirection = enDirection.North;
    enDirection playerNextDirection = enDirection.North;
    Animator anim;
    BridgeSpawner bridgeSpawner;
    AudioSource audioSource;
    Gestures gestures;

    int coinsCollected = 0;
    int distanceRun = 0;
    int coinsCollectedBest;
    int distanceRunBest;
    float playerStartSpeed = 10f;
    float playerSpeed;
    float gValue = 10f;
    float translationFactor = 10f;
    float jumpForce = 1.5f;
    float timer = 0;
    float distance = 0;
    float translationFactorMobile = 5f;
    bool canTurnRight = false;
    bool canTurnLeft = false;
    bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = playerStartSpeed;
        characterController = this.GetComponent<CharacterController>();
        anim = this.GetComponent<Animator>();
        bridgeSpawner = GameObject.Find("BridgeManager").GetComponent<BridgeSpawner>();
        audioSource = this.GetComponent<AudioSource>();
        playerVector = new Vector3(0, 0, 1) * playerSpeed * Time.deltaTime;
        deathMenu.SetActive(false);
        distanceRunBest = PlayerPrefs.GetInt("highscoreD");
        coinsCollectedBest = PlayerPrefs.GetInt("highscoreC");
        gestures = this.GetComponent<Gestures>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerLogic();

        //GUI
        distanceScoreText.text = distanceRun.ToString();
        coinScoreText.text = "x" + coinsCollected.ToString();

    }

    void PlayerLogic()
    {
        if(isDead)
            return;

        if(!characterController.enabled) {  characterController.enabled = true; }
        timer += Time.deltaTime;
        
        playerSpeed += 0.1f * Time.deltaTime; // Constantly increasing Speed

        // KEYBOARD
        // Turn Left: F
        // Turn Right: G
        // Left: L-arrow
        // Righr: R-arrow
        // Jump: Space
        // Slide: Down-arrow

        if ((Input.GetKeyDown(KeyCode.F) && canTurnRight) || (gestures.swipeRight && canTurnRight))
        {
            switch (playerDirection)
            {
                case enDirection.North:
                    playerNextDirection = enDirection.East;
                    this.transform.rotation = Quaternion.Euler(0,90,0);
                    break;
                case enDirection.West:
                    playerNextDirection = enDirection.North;
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
            }

            gestures.swipeRight = false;
            audioSource.PlayOneShot(soundFXClips[6], 0.4f);
        }
        else if ((Input.GetKeyDown(KeyCode.G) && canTurnLeft) || (gestures.swipeLeft && canTurnLeft))
        {
            switch (playerDirection)
            {
                case enDirection.North:
                    playerNextDirection = enDirection.West;
                    this.transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case enDirection.East:
                    playerNextDirection = enDirection.North;
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
            }

            gestures.swipeLeft = false;
            audioSource.PlayOneShot(soundFXClips[6], 0.4f);

        }
        playerDirection = playerNextDirection;

        if (playerDirection == enDirection.North) { playerVector = Vector3.forward * playerSpeed * Time.deltaTime; }
        else if (playerDirection == enDirection.East) { playerVector = Vector3.right * playerSpeed * Time.deltaTime; }
        else if (playerDirection == enDirection.West) { playerVector = Vector3.left * playerSpeed * Time.deltaTime; }

        // HORIZONTAL movement
        switch (playerDirection)
        {
            case enDirection.North:
                if (mobileEnabled)
                {
                    playerVector.x = Input.acceleration.x * translationFactorMobile * Time.deltaTime;
                }
                else { playerVector.x = Input.GetAxisRaw("Horizontal") * translationFactor * Time.deltaTime; }
                break;
            case enDirection.East:
                if (mobileEnabled)
                {
                    playerVector.z = -Input.acceleration.x * translationFactorMobile * Time.deltaTime;
                }
                else { playerVector.z = -Input.GetAxisRaw("Horizontal") * translationFactor * Time.deltaTime; }
                break ;
            case enDirection.West:
                if (mobileEnabled)
                {
                    playerVector.z = Input.acceleration.x * translationFactorMobile * Time.deltaTime;
                }
                else { playerVector.z = Input.GetAxisRaw("Horizontal") * translationFactor * Time.deltaTime; }
                break;
        }
        
        // GRAVITY
        if (characterController.isGrounded)
        {
            playerVector.y = -0.2f;
        }
        else
        {
            playerVector.y -= gValue * Time.deltaTime;
        }

        // SLIDE
        if (Input.GetKeyDown(KeyCode.DownArrow) || gestures.swipeDown)
        {
            DoSliding();
        }

        // JUMP
        if ((Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded) || (gestures.swipeUp && characterController.isGrounded))
        {
            audioSource.PlayOneShot(soundFXClips[3], 0.4f);
            anim.SetTrigger("isJumping");
            playerVector.y = Mathf.Sqrt(jumpForce * gValue);
            gestures.swipeUp = false;
        }

        // FALLING
        if (this.transform.position.y < -0.5f) 
        {
            isDead = true;
            audioSource.PlayOneShot(soundFXClips[2], 0.4f);
            anim.SetTrigger("isTripping");
        }

        characterController.Move(playerVector);
        distance = playerSpeed * timer;
        distanceRun = (int)distance;
    }


    void DoSliding() // Reshapes CharacterController to fit the holes
    {
        characterController.height = 1f;
        characterController.center = new Vector3(0, 0.5f, 0);
        characterController.radius = 0;
        StartCoroutine(ReEnableCC());
        anim.SetTrigger("isSliding");
        audioSource.PlayOneShot(soundFXClips[5], 0.4f);
        gestures.swipeDown = false;
    }

    IEnumerator ReEnableCC()  // Turns CharacterController to default size
    {
        yield return new WaitForSeconds(0.5f);

        characterController.height = 2f;
        characterController.center = new Vector3(0, 1f, 0);
        characterController.radius = 0.2f;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.tag == "LCorner")
        {
            canTurnLeft = true;
        }
        else if(hit.gameObject.tag == "RCorner")
        {
            canTurnRight = true;
        }
        else
        {
            // for Keyboard
            canTurnLeft = false;
            canTurnRight = false;

            //for Swipe
            gestures.swipeRight = false;
            gestures.swipeLeft = false;
        }

        if(hit.gameObject.tag == "Obstacle")
        {
            isDead = true;
            audioSource.PlayOneShot(soundFXClips[1], 0.4f);
            anim.SetTrigger("isTripping");
            SaveScore();
        }
    }

    // GUI
    private void OnGUI()
    {
        if (isDead)
        {
            deathMenu.SetActive(true);
            //if(GUI.Button(new Rect(0.3f * Screen.width, 0.6f * Screen.height, 0.4f * Screen.width, 0.1f * Screen.height), "RESPAWN"))
            //{
            //    DeathEvent();
            //}
            //                          OLD CODE
            //// Current game scores
            //GUI.Label(new Rect(10, 10, 100, 20), "Coins: " + coinsCollected.ToString());
            //GUI.Label(new Rect(10, 55, 100, 20), "Distance: " + distanceRun.ToString());

            //// Saved Scores
            //GUI.Label(new Rect(10, 25, 200, 20), "Highest Collected Coins: " + PlayerPrefs.GetInt("highscoreC").ToString());
            //GUI.Label(new Rect(10, 40, 150, 20), "Highest Distance: " + PlayerPrefs.GetInt("highscoreD").ToString());
        }
    }

    public void DeathEvent()
    {
        deathMenu.SetActive(false);
        bestDistanceText.text = "";
        bestCoinScoreText.text = "";
        characterController.enabled = false;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerDirection = enDirection.North;
        playerNextDirection = enDirection.North;
        playerSpeed = playerStartSpeed;
        playerVector = Vector3.forward * playerSpeed * Time.deltaTime;
        bridgeSpawner.CleanScene();
        anim.SetTrigger("isSpawn");
        coinsCollected = 0;
        timer = 0;
        isDead = false;
    }

    #region Sound Events
    void FootStepEventA()
    {
        audioSource.PlayOneShot(soundFXClips[0], 0.4f);
    }

    void FootStepEventB()
    {
        audioSource.PlayOneShot(soundFXClips[0], 0.4f);
    }

    void JumpLandEvent()
    {
        audioSource.PlayOneShot(soundFXClips[4], 0.4f);
    }
    #endregion

    // COIN COLLECTION func.
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Coin")
        {
            Destroy(col.gameObject);
            audioSource.PlayOneShot(soundFXClips[7], 1f);
            coinsCollected += 1;
        }
    }

    void SaveScore()
    {
        // for Coins
        if(coinsCollected > coinsCollectedBest)
        {
            coinsCollectedBest = coinsCollected;
            PlayerPrefs.SetInt("highscoreC", coinsCollectedBest);
            PlayerPrefs.Save();
            bestCoinScoreText.text = "NEW HIGH SCORE OF TOTAL COINS! " + coinsCollectedBest.ToString();
        }

        // for Distance
        if(distanceRun > distanceRunBest)
        {
            distanceRunBest = distanceRun;
            PlayerPrefs.SetInt("highscoreD", distanceRunBest);
            PlayerPrefs.Save();
            bestDistanceText.text = "NEW HIGH SCORE OF TOTAL DISTANCE! " + distanceRunBest.ToString() + "m";
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
