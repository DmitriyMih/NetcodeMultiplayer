using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerNetwork : NetworkBehaviour
{
    [SerializeField] private GameObject spawnedObjectPrefab;
    [SerializeField] private List<GameObject> spawnedObjectsList = new List<GameObject>();

    [Header("Move Settings")]
    private CharacterController characterController;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    private bool isWalking;
    public bool IsWalking
    {
        get => isWalking;
        set
        {
            isWalking = value;
            IsWalkingChanged?.Invoke(isWalking);
        }
    }

    [Header("Message"), Space()]
    [SerializeField] private NetworkVariable<MyCustomData> messageSystem = new NetworkVariable<MyCustomData>(new MyCustomData { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private string message;

    public Action<bool> IsWalkingChanged;

    public struct MyCustomData : INetworkSerializable
    {
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        messageSystem.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            if (CustomChatManager.Instance != null)
                CustomChatManager.Instance.OnSetMessage?.Invoke(newValue.message.ConvertToString());
            Debug.Log($"Client {OwnerClientId} | Message {newValue.message}" % Colorize.Yellow % FontFormat.Bold);
        };
    }
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (!IsOwner)
            return;

        if (CustomChatManager.Instance != null)
            CustomChatManager.Instance.OnSendMeaage += SendGlobalMessage;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleMovement();
        HandleRotation();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            InstatiateObjectInServerRpc();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            DestroyObjectInServerRpc();
    }

    private Vector2 GetInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector2(vertical, -horizontal);
    }

    private void SendGlobalMessage(string newMessage)
    {
        messageSystem.Value = new MyCustomData { message = $"Player {OwnerClientId} Send: " + newMessage };
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GetInput();
        IsWalking = inputVector != Vector2.zero;

        float handleGravity = HandleGravity();
        Vector3 moveDirection = new Vector3(inputVector.x, handleGravity, inputVector.y);

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector2 inputVector = GetInput();
        Vector3 positionToLookAt = new Vector3(inputVector.x, 0f, inputVector.y);
        Quaternion currentRotation = transform.rotation;

        if (isWalking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private float HandleGravity()
    {
        return characterController.isGrounded ? -0.05f : -9.8f;
    }

    [ServerRpc]
    private void InstatiateObjectInServerRpc()
    {
        if (spawnedObjectPrefab == null)
            return;

        GameObject spawnedObject = Instantiate(spawnedObjectPrefab, transform);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);

        spawnedObjectsList.Add(spawnedObject);
    }

    [ServerRpc]
    private void DestroyObjectInServerRpc()
    {
        if (spawnedObjectsList.Count == 0)
            return;

        GameObject destroyObject = spawnedObjectsList[spawnedObjectsList.Count - 1];
        spawnedObjectsList.Remove(destroyObject);

        destroyObject.GetComponent<NetworkObject>().Despawn();
        Destroy(destroyObject);
    }
}