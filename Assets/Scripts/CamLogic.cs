using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamLogic : MonoBehaviour
{
//                     ********************************
//                     |  Written by:  Savran Donmez  |
//                     |  Start Date:  04/12/2022     |
//                     |  Last Update: 08/12/2022     |
//                     ********************************
    public Transform playerTransform;

    public float fixedDistance;
    public float height;
    public float heightFactor;
    public float rotationFactor;

    float startRotationAngle; // cam's Rotation angle around Y axis.
    float endRotationAngle; // player's rotation angle around Y axis.
    float finalRotationAngle; // smoothed out rotation angle of cam around Y axis.

    float currentHeight;
    float wantedHeight;

    // Update is called once per frame
    void LateUpdate()
    {
        currentHeight = this.transform.position.y;
        wantedHeight = playerTransform.position.y + height;
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightFactor * Time.deltaTime);

        startRotationAngle = this.transform.eulerAngles.y;
        endRotationAngle = playerTransform.eulerAngles.y;
        finalRotationAngle = Mathf.LerpAngle(startRotationAngle, endRotationAngle, Time.deltaTime * rotationFactor);
        
        Quaternion finalRotation = Quaternion.Euler(0, finalRotationAngle, 0); // Convert angle value into actual rotation.
        this.transform.position = playerTransform.position;
        this.transform.position -= finalRotation * Vector3.forward * fixedDistance;

        this.transform.position = new Vector3(this.transform.position.x, currentHeight, this.transform.position.z);
        this.transform.LookAt(playerTransform);
        
    }   
}
