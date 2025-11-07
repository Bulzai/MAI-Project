using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeCursorColliderSmaller : MonoBehaviour
{

    public BoxCollider2D makeColliderSmaller;



    private void Start()
    {
        makeColliderSmaller = GetComponent<BoxCollider2D>();

    }

    private void OnEnable()
    {

        GameEvents.OnPlaceItemStateEntered += MakeColliderSmaller;
    }


    void MakeColliderSmaller()
    {
        Debug.Log("ItemState COllider smaller");
        if (makeColliderSmaller != null)
        {
            makeColliderSmaller.offset = new Vector2(-0.02170485f, 0.1969473f);
            makeColliderSmaller.size = new Vector2(1.204323f, 2.858709f);
        }

    }
}
