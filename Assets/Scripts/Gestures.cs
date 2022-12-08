using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gestures : MonoBehaviour
{
    [HideInInspector]
    public bool swipeLeft = false;
    [HideInInspector]
    public bool swipeRight = false;
    [HideInInspector]
    public bool swipeUp = false;
    [HideInInspector]
    public bool swipeDown = false;

    Vector2 touchStartPos;

    float minSwipePixelDistance = 100f;
    bool touchStarted = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStarted = true;
                    touchStartPos = touch.position;

                    swipeLeft = false;
                    swipeRight = false;
                    swipeUp = false;
                    swipeDown = false;
                    
                    break;
                case TouchPhase.Ended:
                    if (touchStarted)
                    {
                        Swipe(touch);
                        touchStarted = false;
                    }
                    break;
                case TouchPhase.Canceled:
                    touchStarted = false;
                    break;
            }
        }
    }

    void Swipe(Touch touch)
    {
        Vector2 touchLastPos = touch.position;
        float distance = Vector2.Distance(touchLastPos, touchStartPos);

        if(distance > minSwipePixelDistance)
        {
            float dy = touchLastPos.y - touchStartPos.y;
            float dx = touchLastPos.x - touchStartPos.x;

            float swipeAngle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            swipeAngle = (swipeAngle + 360) % 360;

            if(swipeAngle < 50 || swipeAngle > 350)
            {
                swipeRight = true;
                //Debug.Log("Right");
            }
            else if(swipeAngle < 150)
            {
                swipeUp = true;
                //Debug.Log("Up");
            }
            else if(swipeAngle < 190)
            {
                swipeLeft = true;
                //Debug.Log("Left");
            }
            else if(swipeAngle > 190)
            {
                swipeDown = true;
                //Debug.Log("Down");
            }
        }
    }
}
