using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_PathManager : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new();

        public int      waypointCount   => waypoints.Count;
        public Vector3  spawnPosition   => waypoints.Count > 0 ? waypoints[0].position : Vector3.zero;

        public Transform GetWaypoint(int index) =>
            index >= 0 && index < waypoints.Count ? waypoints[index] : null;

        public Vector3 GetWaypointPosition(int index) =>
            GetWaypoint(index)?.position ?? Vector3.zero;
    }
}
