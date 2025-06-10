using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private PlayerAimWeapon playerAimWeapon;
    [SerializeField] private CameraShaker cameraShaker; 

    // Start is called before the first frame update
    void Awake()
    {
        playerAimWeapon.OnShoot += PlayerAimWeapon_OnShoot;
        
    }

    private void PlayerAimWeapon_OnShoot( object sender, PlayerAimWeapon.OnShootEventArgs e)
    {
        cameraShaker.ShakeCamera(0.05f, .1f);

    }
}
