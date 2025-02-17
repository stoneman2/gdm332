using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Collections;

public class MainPlayerScript : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    Rigidbody rb;
    private NetworkVariable<NetworkString> goggleColor = new NetworkVariable<NetworkString>(
        new NetworkString{info = "Color"}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );
    public NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>(
        new NetworkString{info = "Player"}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public GameObject goggleObject;
    public List<MonoBehaviour> scriptsDeath;
    public List<MeshRenderer> renderersDeath;

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
        
        if (colorCooldown > 0)
        {
            colorCooldown -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            // If we press F, we cycle through the colors
            switch (goggleColor.Value.ToString())
            {
                case "Blue":
                    goggleColor.Value = "Red";
                    break;
                case "Red":
                    goggleColor.Value = "Yellow";
                    break;
                case "Yellow":
                    goggleColor.Value = "Blue";
                    break;
            }
            colorCooldown = 0.5f;
        }
    }
    public GameObject nameLabel;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
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

            // Set default color
            goggleColor.Value = "Blue";
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

        // Color changing for goggles
        ProcessColorInput();

        if (goggleColor.Value.ToString() == "Blue")
        {
            glassColor = Color.blue;
        }
        else if (goggleColor.Value.ToString() == "Red")
        {
            glassColor = Color.red;
        }
        else if (goggleColor.Value.ToString() == "Yellow")
        {
            glassColor = Color.yellow;
        }

        // Set the color of the goggles
        goggleObject.GetComponent<Renderer>().material.SetColor("_BaseColor", glassColor);
    }

    private void FixedUpdate()
    {
        float translation = Input.GetAxis("Vertical") * speed;
        translation *= Time.deltaTime;
        rb.MovePosition(rb.position + this.transform.forward * translation);

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
