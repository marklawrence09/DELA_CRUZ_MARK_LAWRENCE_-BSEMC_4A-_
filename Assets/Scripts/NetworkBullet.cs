using Unity.Netcode;
using UnityEngine;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damageAmount = 25;

    private Rigidbody rb;
    private PlayerController shooterPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetShooter(PlayerController shooter)
    {
        shooterPlayer = shooter;
    }

    private void Start()
    {
        if (IsServer)
        {
            Destroy(gameObject, lifetime);
        }
    }

    private void FixedUpdate()
    {
        
        if (rb != null)
        {
            rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

       
        PlayerController targetPlayer = other.GetComponentInParent<PlayerController>();

        if (targetPlayer != null && targetPlayer != shooterPlayer)
        {
            targetPlayer.TakeDamage(damageAmount, shooterPlayer);

            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true);
            }
        }
    }
}