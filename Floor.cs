using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessFloor {
  public class Floor : MonoBehaviour
  {
     private GameObject _refObj;
     private FloorGenerator _floorGenerator;

     protected string id = "";

     void Start()
     {
        _refObj = GameObject.Find("/FloorGenerator");
        _floorGenerator = _refObj.GetComponent<FloorGenerator>();
     }

     public void OnCollisionStay(Collision other)
     {
        if (_floorGenerator != null)
        {
           _floorGenerator.OnCollisionStayFloor(other, id, gameObject);
        }
     }

  }
}