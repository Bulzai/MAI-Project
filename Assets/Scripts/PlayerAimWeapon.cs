using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimWeapon : MonoBehaviour
{
    [SerializeField] private Transform aimTransform; // Reference to Aim object
    [SerializeField] private BulletData bulletData;
    [SerializeField] private Transform firePoint; // This is your gun barrel

    private float _lastShotTime;
    private Vector2 _aimInput;
    private Vector2 _lastValidAimDirection = Vector2.right; // default aim direction


    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs
    {
        public Vector3 gunEndPointPosition;
    }

    private void Start()
    {
        _lastValidAimDirection = aimTransform.right;
    }


    private void Shoot(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        GameObject bullet = Instantiate(bulletData.bulletPrefab, firePoint.position, Quaternion.identity);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript == null)
        {
            Debug.LogError("Bullet prefab is missing the Bullet script!");
            return;
        }
        bullet.GetComponent<Bullet>().Initialize(direction.normalized, bulletData);
    }


    // Start is called before the first frame update
    private void Update()
    {
        HandleShooting(HandleAiming());
    }
    // also for aiming at mouse cursor
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = 10f; // Distance to camera
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private Vector2 HandleAiming()
    {
        float horizontal = Input.GetAxis("RightStickHorizontal");
        float vertical = Input.GetAxis("RightStickVertical");
        Vector2 currentInput = new Vector2(horizontal, vertical);

        if (currentInput.sqrMagnitude > 0.1f)  // 0.01 is ~0.1 magnitude (good threshold)
        {
            _lastValidAimDirection = currentInput.normalized;
        }
        // Calculate angle based on last valid aim direction
        float angle = Mathf.Atan2(_lastValidAimDirection.y, _lastValidAimDirection.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0, 0, angle);

        Debug.Log($"Input: ({horizontal}, {vertical}) | Angle: {Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg}");

        return _lastValidAimDirection;
    }

    private void HandleShooting(Vector2 _currentAimDirection)
    {

        // Only shoot if stick is moved and cooldown passed or R1 is pressed
        bool wantsToShoot = Input.GetButton("Fire1"); // R1 should be mapped to "Fire1" in Input Manager

        if (wantsToShoot && Time.time > _lastShotTime + bulletData.cooldown)
        {
            Shoot(_currentAimDirection);
            _lastShotTime = Time.time;
            OnShoot?.Invoke(this, new OnShootEventArgs
            {
                gunEndPointPosition = firePoint.transform.position
            });
        }
    }
}
