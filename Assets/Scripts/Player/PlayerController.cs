using Unity.Netcode;
using UnityEngine;

namespace WastelandShooter.Player
{
    /// <summary>
    /// Basic player controller for multiplayer movement and input handling
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float mouseSensitivity = 2f;
        
        [Header("Components")]
        private CharacterController characterController;
        private Camera playerCamera;
        
        // Network variables for synchronized movement
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<float> networkRotationY = new NetworkVariable<float>();
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();
        }
        
        public override void OnNetworkSpawn()
        {
            // Only enable camera and input for the local player
            if (IsOwner)
            {
                playerCamera.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                playerCamera.enabled = false;
            }
        }
        
        private void Update()
        {
            if (!IsOwner) return;
            
            HandleMovement();
            HandleMouseLook();
        }
        
        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 direction = transform.right * horizontal + transform.forward * vertical;
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            
            // Apply gravity
            movement.y = -9.81f * Time.deltaTime;
            
            characterController.Move(movement);
            
            // Update network position
            UpdatePositionServerRpc(transform.position, transform.eulerAngles.y);
        }
        
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
        }
        
        [ServerRpc]
        private void UpdatePositionServerRpc(Vector3 position, float rotationY)
        {
            networkPosition.Value = position;
            networkRotationY.Value = rotationY;
        }
        
        private void OnEnable()
        {
            networkPosition.OnValueChanged += OnPositionChanged;
            networkRotationY.OnValueChanged += OnRotationChanged;
        }
        
        private void OnDisable()
        {
            networkPosition.OnValueChanged -= OnPositionChanged;
            networkRotationY.OnValueChanged -= OnRotationChanged;
        }
        
        private void OnPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            if (!IsOwner)
            {
                transform.position = newValue;
            }
        }
        
        private void OnRotationChanged(float previousValue, float newValue)
        {
            if (!IsOwner)
            {
                transform.rotation = Quaternion.Euler(0, newValue, 0);
            }
        }
    }
}