using Smooth;
using Unity.Netcode;
using UnityEngine;

public class PProjectile : PNetworkAbility
{
    [SerializeField] private Transform throwPosition;
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private GameObject projectilePrefab;
    private NetworkVariable<NetworkObjectReference> _pooledProjectile = new();
    private Rigidbody _pooledProjectileRb;
    private SmoothSyncNetcode _pooledProjectileSync;
    private bool _previewingThrow;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkObject obj = Instantiate(projectilePrefab, Vector3.one * 999, Quaternion.identity).GetComponent<NetworkObject>();
            obj.Spawn();
            _pooledProjectile.Value = obj;
        }
    }
    public override void EnableAbility()
    {
        base.EnableAbility();
        InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Start, StartPreviewThrowProjectile);
        InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Stop, ThrowProjectileClient);
        
    }

    public override void DisableAbility()
    {
        base.DisableAbility();
        InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Start, StartPreviewThrowProjectile);
        InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Stop, ThrowProjectileClient);
    }
    
    private void StartPreviewThrowProjectile()
    {
        _previewingThrow = true;
        Debug.Log("Previewing throw");
    }
    private void StopPreviewThrowProjectile()
    {
        _previewingThrow = false;
        Debug.Log("Stopped previewing throw");
    }
    private void ThrowProjectileClient()
    {
        if(_previewingThrow) StopPreviewThrowProjectile();
        Vector3 position = throwPosition.position;
        Vector3 direction = throwPosition.forward;
        float force = throwSpeed;
        ThrowProjectileServerRpc(position, direction, force);
        ThrowProjectile(position, direction, force);
    }

    private void ThrowProjectile(Vector3 position, Vector3 direction, float force)
    {
        if (_pooledProjectileRb == null)
        {
            _pooledProjectile.Value.TryGet(out NetworkObject obj, NetworkManager.Singleton);
            _pooledProjectileRb = obj.GetComponent<Rigidbody>();
        }
        
        _pooledProjectileRb.transform.rotation = Quaternion.LookRotation(direction);
        _pooledProjectileRb.position = position;
        _pooledProjectileRb.linearVelocity = direction * force;
    }
    [Rpc(SendTo.Server)]
    private void ThrowProjectileServerRpc(Vector3 position, Vector3 direction, float force) => ThrowProjectile(position,direction,force);
}