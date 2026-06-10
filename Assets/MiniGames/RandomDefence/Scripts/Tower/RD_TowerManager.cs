using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TowerManager : MonoBehaviour
    {
        [SerializeField] private List<RD_TowerSlot> slots = new();

        public IReadOnlyList<RD_TowerSlot> allSlots => slots;

        public void ClearAll()
        {
            foreach (var slot in slots)
            {
                if (!slot.isOccupied) continue;
                var tower = slot.Remove();
                if (tower != null) Destroy(tower.gameObject);
            }
        }

        public IEnumerable<RD_TowerBase> GetActiveTowers()
        {
            foreach (var slot in slots)
                if (slot.isOccupied) yield return slot.placedTower;
        }

        public RD_TowerSlot GetFirstEmptySlot()
        {
            foreach (var slot in slots)
                if (!slot.isOccupied) return slot;
            return null;
        }

        public bool HasEmptySlot() => GetFirstEmptySlot() != null;

        public bool PlaceTower(RD_TowerBase tower)
        {
            RD_TowerSlot slot = GetFirstEmptySlot();
            return slot != null && slot.TryPlace(tower);
        }

        public void SellTower(RD_TowerBase tower)
        {
            if (tower == null) return;

            foreach (var slot in slots)
            {
                if (slot.placedTower != tower) continue;
                slot.Remove();
                break;
            }

            int price = tower.sellPrice;
            Destroy(tower.gameObject);
            RD_GameManager.instance?.economyMgr?.AddCurrency(price);
        }

        public bool TryMerge(RD_TowerBase towerA, RD_TowerBase towerB)
        {
            if (towerA == null || towerB == null || towerA == towerB) return false;
            if (towerA.type != towerB.type || towerA.grade != towerB.grade) return false;

            foreach (var slot in slots)
            {
                if (slot.placedTower != towerA) continue;
                slot.Remove();
                break;
            }
            Destroy(towerA.gameObject);
            towerB.TryGradeUp();
            return true;
        }

        public void SwapSlots(RD_TowerSlot slotA, RD_TowerSlot slotB)
        {
            if (slotA == null || slotB == null || slotA == slotB) return;
            RD_TowerBase towerA = slotA.Remove();
            RD_TowerBase towerB = slotB.Remove();
            if (towerB != null) slotA.TryPlace(towerB);
            if (towerA != null) slotB.TryPlace(towerA);
        }
    }
}
