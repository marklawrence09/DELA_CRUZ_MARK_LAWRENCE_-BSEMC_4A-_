using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components; // Required for NetworkAnimator
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Combat Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI scoreText;

    // NetworkVariables for Health & Score
    public NetworkVariable<int> Health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Animator animator;
    private NetworkAnimator networkAnimator;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe to NetworkVariable change events
        Health.OnValueChanged += OnHealthChanged;
        Score.OnValueChanged += OnScoreChanged;

        // Initialize world-space Health Slider
        if (healthSlider != null)
        {
            healthSlider.value = Health.Value;
        }

        // Dynamically find and bind Scene Score UI for the local player instance
        if (IsOwner)
        {
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
                scoreText.text = "Score: " + Score.Value;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        Health.OnValueChanged -= OnHealthChanged;
        Score.OnValueChanged -= OnScoreChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (healthSlider != null)
        {
            healthSlider.value = newValue;
        }

        if (newValue <= 0 && !isDead)
        {
            TriggerDeath();
        }
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        // Update the local player's HUD score text on value change
        if (IsOwner && scoreText != null)
        {
            scoreText.text = "Score: " + newValue;
        }
    }

    private void Update()
    {
        if (!IsOwner || isDead) return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", moveDir.magnitude);
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FireBulletServerRpc();
        }
    }

    [ServerRpc]
    private void FireBulletServerRpc()
    {
        // Default spawn offset if FirePoint transform is not assigned
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 1.0f + Vector3.up * 0.8f;
        Quaternion spawnRot = transform.rotation;

        if (bulletPrefab != null)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, spawnPos, spawnRot);
            NetworkBullet bulletScript = bulletInstance.GetComponent<NetworkBullet>();

            if (bulletScript != null)
            {
                bulletScript.SetShooter(this);
            }

            bulletInstance.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void TakeDamage(int damage, PlayerController shooter)
    {
        // Only the server processes health updates and score attribution
        if (!IsServer || isDead) return;

        int previousHealth = Health.Value;

        // Apply health deduction
        Health.Value = Mathf.Max(0, Health.Value - damage);

        // Check for fatal hit to grant kill score
        if (shooter != null && shooter != this)
        {
            if (previousHealth > 0 && Health.Value == 0)
            {
                shooter.Score.Value += 100; // Award kill score
                Debug.Log($"Player {shooter.OwnerClientId} scored a kill on Player {OwnerClientId}!");
            }
        }
    }

    private void TriggerDeath()
    {
        isDead = true;

        // 1. Trigger Death animation across network using NetworkAnimator
        if (networkAnimator != null)
        {
            networkAnimator.SetTrigger("Death");
        }
        else if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // 2. Disable collider so corpse doesn't block bullet projectiles or active players
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 3. Freeze Rigidbody velocity so corpse stays grounded
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
}