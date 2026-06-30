//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class QuadZoneTrigger : MonoBehaviour
//{
//    private QuadZoneTracker _tracker;

//    public void Initialize(QuadZoneTracker tracker)
//    {
//        _tracker = tracker;
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player")) _tracker.UpdatePlayerZone(collision.gameObject, gameObject.name);
//    }

//    private void OnTriggerExit2D(Collider2D collision)
//    {
//        if (collision.CompareTag("Player")) _tracker.RemovePlayer(collision.gameObject);
//    }
//}
