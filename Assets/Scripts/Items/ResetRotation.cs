using UnityEngine;

public class ResetRotation : MonoBehaviour
{
    private Vector3 _originalPos;
    private Quaternion _originalRot;

    private void StorePos()
    {
        _originalPos = transform.position;
        _originalRot = transform.rotation;
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += StorePos;

        GameEvents.OnMainGameStateExited += ResetTransform;

    }
    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= StorePos;

        GameEvents.OnMainGameStateExited -= ResetTransform;

    }

    private void ResetTransform()
    {
        transform.SetPositionAndRotation(_originalPos, _originalRot);
    }
}
