using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TowerSlot : MonoBehaviour
    {
        public RD_TowerBase placedTower { get; private set; }
        public bool         isOccupied  => placedTower != null;

        public bool TryPlace(RD_TowerBase tower)
        {
            if (isOccupied) return false;
            placedTower = tower;
            tower.transform.SetParent(transform, worldPositionStays: false);
            tower.transform.localPosition = Vector3.zero;
            tower.transform.localRotation = Quaternion.identity;
            tower.transform.localScale    = Vector3.one;
            return true;
        }

        public RD_TowerBase Remove()
        {
            if (!isOccupied) return null;
            RD_TowerBase t = placedTower;
            t.transform.SetParent(null);
            placedTower = null;
            return t;
        }
    }
}
