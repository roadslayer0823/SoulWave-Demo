using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowParticles : MonoBehaviour
{
    public Transform targetCamera;
    public Vector3 offset = new Vector3(0, 10, 0);

    private void LateUpdate()
    {
        if(targetCamera != null)
        {
            transform.position = targetCamera.position + offset;
        }
    }
}
