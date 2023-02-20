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
    [SerializeField] private NetworkVariable<MyCustomData> randomStruct = new NetworkVariable<MyCustomData>(new MyCustomData { _int = 1, _bool = false }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
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

        randomStruct.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log($"Client {OwnerClientId} | Value {newValue._int} | Bool {newValue._bool} | Message {newValue.message}" % Colorize.Yellow % FontFormat.Bold);
        };
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
            randomStruct.Value = new MyCustomData { _int = Random.Range(1, 5), _bool = ConvertToBool(Random.Range(0, 2)), message = "Player " + OwnerClientId };
    }

    private bool ConvertToBool(int value)
    {
        return value != 0;
    }

    private void HandleMovement()
    {
        transform.position += moveDirection * randomSpeedNumber.Value * moveSpeed * Time.deltaTime;
    }
}