using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Collections;
using System;

public class MainPlayerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;
    public NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>(
        new NetworkString{info = "Player"}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public List<MonoBehaviour> scriptsDeath;
    public List<SkinnedMeshRenderer> renderersDeath;
    private float InputX;
    private float InputZ;
    private Animator animator;
    private bool running;

    public struct NetworkString : INetworkSerializable
    {
        public FixedString32Bytes info;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref info);
        }
        public override string ToString()
        {
            return info.ToString(); 
        }

        public static implicit operator NetworkString(string v) =>
            new NetworkString() { info = new FixedString32Bytes(v) };
    }
    private float colorCooldown = 0.5f;
    private Color glassColor;
    private void ProcessColorInput()
    {
        if (!IsOwner) return;
    }
    public GameObject nameLabel;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        animator = this.GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("MainCanvas");
        nameLabel = Instantiate(nameLabel, Vector3.zero, Quaternion.identity);
        nameLabel.transform.SetParent(canvas.transform);

        if (IsOwner)
        {
            string name = LoginManager.Instance.myUsername;
            Debug.Log($"Trying to set the name of {OwnerClientId} to {name}");
            playerName.Value = name;
        }
    }

    public override void OnDestroy()
    {
        Destroy(nameLabel);
    }

    private void Update()
    {
        Vector3 namePos = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0, 2f, 0));
        nameLabel.transform.position = namePos;

        int clientId = (int)OwnerClientId;

        if (clientId == 0)
        {
            nameLabel.GetComponent<TMPro.TextMeshProUGUI>().text = playerName.Value.ToString() + " (Host)";
        }
        else
        {
            nameLabel.GetComponent<TMPro.TextMeshProUGUI>().text = playerName.Value.ToString();
        }
    }

    private void MoveForward()
    {
        float verticalInput = Input.GetAxis("Vertical");
        if (Math.Abs(verticalInput) > 0.01f)
        {
            if (verticalInput > 0.01f)
            {
                float translation = verticalInput * speed;
                translation *= Time.fixedDeltaTime;
                rb.MovePosition(rb.position + this.transform.forward * translation);

                if (!running)
                {
                    running = true;
                    animator.SetBool("Running", true);
                }
            }
        }
        else if (running)
        {
            running = false;
            animator.SetBool("Running", false);
        }
    }

    private void Turn()
    {
        float rotation = Input.GetAxis("Horizontal");
        if (rotation != 0)
        {
            rotation *= rotationSpeed;
            Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * turn);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Add these variables to your class
    public bool isGrounded;

    // Add this method to check ground status
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (isGrounded)
        {
            animator.SetBool("Falling", false);
        }
        else
        {
            animator.SetBool("Falling", true);
        }

        MoveForward();
        Turn();
    }

    // private void FixedUpdate()
    // {
    //     float translation = Input.GetAxis("Vertical") * speed;
    //     translation *= Time.deltaTime;
    //     rb.MovePosition(rb.position + this.transform.forward * translation);

    //     float rotation = Input.GetAxis("Horizontal");
    //     if (rotation != 0)
    //     {
    //         rotation *= rotationSpeed;
    //         Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
    //         rb.MoveRotation(rb.rotation * turn);
    //     }
    //     else
    //     {
    //         rb.angularVelocity = Vector3.zero;
    //     }
    // }

    private void OnEnable()
    {
        if (nameLabel != null)
        {
            nameLabel.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (nameLabel != null)
        {
            nameLabel.SetActive(false);
        }
    }
}
