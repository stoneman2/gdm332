using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSystem : NetworkBehaviour
{
    public GameObject cameraObject;
    private void Update()
    {
        // If we're not the owner of this camera, we should hide ourselves so our camera isn't used.
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
