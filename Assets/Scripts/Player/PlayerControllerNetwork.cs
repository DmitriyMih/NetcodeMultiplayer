using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerControllerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    private Vector3 moveDirection;

    [SerializeField] private NetworkVariable<int> randomSpeedNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private NetworkVariable<MyCustomData> messageSystem = new NetworkVariable<MyCustomData>(new MyCustomData { }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private string message;

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
        //  Update Value
        randomSpeedNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log($"Client {OwnerClientId} | Value {randomSpeedNumber.Value}" % Colorize.Green % FontFormat.Bold);
        };

        messageSystem.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            if (CustomChatManager.Instance != null)
                CustomChatManager.Instance.OnSetMessage?.Invoke(newValue.message.ConvertToString());
            Debug.Log($"Client {OwnerClientId} | Message {newValue.message}" % Colorize.Yellow % FontFormat.Bold);
        };
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

        GetInput();
        HandleMovement();
    }

    private void GetInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = new Vector3(vertical, 0, -horizontal);

        if (Input.GetKeyDown(KeyCode.Q))
            randomSpeedNumber.Value = Random.Range(1, 4);

        if (Input.GetKeyDown(KeyCode.E))
            messageSystem.Value = new MyCustomData { message = $"Player {OwnerClientId} Send: " + message };
    }

    private void SendGlobalMessage(string newMessage)
    {
        messageSystem.Value = new MyCustomData { message = $"Player {OwnerClientId} Send: " + newMessage };
    }

    private void HandleMovement()
    {
        transform.position += moveDirection * randomSpeedNumber.Value * moveSpeed * Time.deltaTime;
    }
}