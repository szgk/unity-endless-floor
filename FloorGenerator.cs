using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace EndlessFloor {
  public class FloorGenerator : MonoBehaviour
  {
      [Tooltip("First Floor Object")]
      [SerializeField] GameObject firstFloorObject;

      [Tooltip("Floor Prefab")]
      [SerializeField] GameObject floorPrefab;

      [Tooltip("Tag Name of Player")]
      [SerializeField] string playerTagName;

      // Generated Floor list.
      private List<GameObject> _floorList = new List<GameObject>();
      public List<GameObject> floorList
      {
          get { return _floorList; }
      }

      // New generated Floor list published for use in other classes.
      public List<GameObject> newFloorList = new List<GameObject>();

      // Visited Floor list.
      private List<string> _visitedFloorNameList = new List<string>(3);

      // Time list for debounce.
      private List<DateTime> currentTimeList = new List<DateTime> { DateTime.Now };

      private byte[] _floorPositionIndexArray = new byte [8]{
          0, // LeftForward
          1, // CenterForward
          2, // RightForward
          3, // LeftMiddle
          4, // RightMiddle
          5, // LeftBack
          6, // CenterBack
          7, // RightBack
      };

      private Vector3[] _floorPositionArray;

      void Awake()
      {
        // Attache Floor script if not exist.
        if(firstFloorObject.GetComponent<Floor>() == null) {
          firstFloorObject.AddComponent<Floor>();
        }
        if(floorPrefab.GetComponent<Floor>() == null) {
          floorPrefab.AddComponent<Floor>();
        }
      }

      void Start()
      {
          _floorPositionArray = new Vector3[]
          {
            // 0 LeftForward
            new Vector3(-floorPrefab.transform.localScale.x, 0, floorPrefab.transform.localScale.z), 
            // 1 CenterForward
            new Vector3(0, 0, floorPrefab.transform.localScale.z),
            // 2 RightForward
            new Vector3(floorPrefab.transform.localScale.x, 0, floorPrefab.transform.localScale.z),
            // 3 LeftMiddle
            new Vector3(-floorPrefab.transform.localScale.x, 0, 0),
            // 4 RightMiddle
            new Vector3(floorPrefab.transform.localScale.x, 0, 0),
            // 5 LeftBack
            new Vector3(-floorPrefab.transform.localScale.x, 0, -floorPrefab.transform.localScale.z),
            // 6 CenterBack
            new Vector3(0, 0, -floorPrefab.transform.localScale.z),
            // 7 RightBack
            new Vector3(floorPrefab.transform.localScale.x, 0, -floorPrefab.transform.localScale.z),
          };

          // init visitedFloorList
          _visitedFloorNameList.AddRange(new string[] { "", "", "" });

          GameObject progressObj = GameObject.Find("/Canvas/ProgressBar");

          Init();
      }

      // <summary>
      //   Generate first Floors
      // </summary>
      public List<GameObject> Init()
      {
          _floorList.Add(firstFloorObject);

          List<byte> initList = new List<byte>();
          initList.AddRange(_floorPositionIndexArray);
          // Generate first floor
          return GenerateFloor(firstFloorObject, initList);
      }

      // <summary>
      //   Set Visited Floor Name.
      // </summary>
      public void SetVisitedFloorName(string FloorName)
      {
          if (_visitedFloorNameList.Count != 0)
          {
              _visitedFloorNameList.RemoveAt(0);
          }
          _visitedFloorNameList.Add(FloorName);
      }

      // <summary>
      //   Generate Floors around the current Floor.
      // </summary>
      private List<GameObject> GenerateFloor(GameObject targetFloor, List<byte> positionIndexList)
      {
          List<GameObject> newFloors = new List<GameObject>();
          for(byte i = 0; i < positionIndexList.Count; i++)
          {
              string newName = Guid.NewGuid().ToString() + i;

              GameObject newFloor = Instantiate(
                floorPrefab,
                targetFloor.transform.position + _floorPositionArray[positionIndexList[i]],
                Quaternion.identity
              );

              newFloor.name = newName;
              newFloors.Add(newFloor);
          }
          newFloorList = newFloors;
          _floorList.AddRange(newFloors);
          return newFloors;
      }

      // <summary>
      //   Regenerate Floors (Generates adjacent Floors & Remove non-adjacent Floors).
      // </summary>
      public List<GameObject> RegenerateFloor(GameObject currentFloor)
      {
          List<GameObject> destoroiedFloors = new List<GameObject>();
          List<byte> newFloorIndexList = new List<byte>();

          // Get exist floor & Remove non-adjacent floors.
          for(byte i = 0; i < _floorList.Count; i++)
          {
              // Don't destoroied current floor.
              if (_floorList[i].name == currentFloor.name) continue;

              bool isAdjacent = AddFloorToListIfAdjacent(newFloorIndexList, _floorList[i], currentFloor.transform.position);
              if (!isAdjacent)
              {
                // Remove non-adjacent floors.
                Destroy(_floorList[i]);
                destoroiedFloors.Add(_floorList[i]);
              }
          };

          // Remove destoroiedFloors from _floorList.
          for(byte i = 0; i < destoroiedFloors.Count; i++)
          {
            _floorList.Remove(destoroiedFloors[i]);
          };

          List<byte> notExistPositionIndexList = GetNotExistPositions(newFloorIndexList);
          // Gemerate not exist floor.
          return GenerateFloor(currentFloor, notExistPositionIndexList);
      }

      // <summary>
      //   Get positoins that Floor not exist.
      // </summary>
      private List<byte> GetNotExistPositions(List<byte> newFloorIndexList) {
        List<byte> notExistPositionIndexList = new List<byte>();
        for(int i = 0; i < _floorPositionIndexArray.Length; i++) {
          if(newFloorIndexList.IndexOf(_floorPositionIndexArray[i]) < 0) {
            notExistPositionIndexList.Add(_floorPositionIndexArray[i]);
          }
        }
        return notExistPositionIndexList;
      }

      // <summary>
      //   If it is adjacent positon, add the index of floor position  to the newFloorIndexList.
      // </summary>
      private bool AddFloorToListIfAdjacent(List<byte> newFloorIndexList, GameObject floor, Vector3 currentFloorPosition) {
          if (floor.transform.position == _floorPositionArray[0] + currentFloorPosition)
          {
              newFloorIndexList.Add(0);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[1] + currentFloorPosition)
          {
              newFloorIndexList.Add(1);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[2] + currentFloorPosition)
          {
              newFloorIndexList.Add(2);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[3] + currentFloorPosition)
          {
              newFloorIndexList.Add(3);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[4] + currentFloorPosition)
          {
              newFloorIndexList.Add(4);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[5] + currentFloorPosition)
          {
              newFloorIndexList.Add(5);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[6] + currentFloorPosition)
          {
              newFloorIndexList.Add(6);
              return true;
          }
          else if (floor.transform.position == _floorPositionArray[7] + currentFloorPosition)
          {
              newFloorIndexList.Add(7);
              return true;
          }
          return false;
      }

      // <summary>
      //   Generate new Floors.
      // </summary>
      public IEnumerator GenerateNewArea(string floorId, GameObject currentFloor)
      {
          if (_visitedFloorNameList[_visitedFloorNameList.Count - 1] != currentFloor.name)
          {
              SetVisitedFloorName(currentFloor.name);
              DateTime newDateTime = DateTime.Now;
              currentTimeList.Add(newDateTime);

              yield return new WaitForSeconds(0.1f);

              if (currentTimeList[currentTimeList.Count - 1] > newDateTime) yield break;
              currentTimeList.RemoveAt(0);
              List<GameObject> newFloors = RegenerateFloor(currentFloor);
          }
      }

     public void OnCollisionStayFloor(Collision other, string floorId, GameObject currentFloor)
     {
         if (other.gameObject.tag == playerTagName)
         {
             StartCoroutine(GenerateNewArea(floorId, currentFloor));
         }
     }
  }
}