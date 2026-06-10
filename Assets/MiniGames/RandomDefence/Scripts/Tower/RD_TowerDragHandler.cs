using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace MiniGames.RandomDefence
{
    [RequireComponent(typeof(RD_TowerBase))]
    public class RD_TowerDragHandler : MonoBehaviour
    {
        private RD_TowerBase _towerBase;
        private SortingGroup _sortingGroup;
        private Collider2D   _collider;

        private enum PointerState { None, Pressed, Dragging }
        private PointerState _state = PointerState.None;

        private RD_TowerSlot _originSlot;
        private Vector3      _dragOffset;
        private Vector3      _pressWorldPos;

        private const float DragThreshold = 0.2f;

        private static bool s_anyDragging;
        private static RD_TowerDragHandler s_selectedHandler;

        private void Awake()
        {
            _towerBase    = GetComponent<RD_TowerBase>();
            _sortingGroup = GetComponent<SortingGroup>();
            _collider     = GetComponent<Collider2D>();
#if UNITY_EDITOR
            if (_sortingGroup == null)
                Debug.LogWarning($"[RD_TowerDragHandler] {name}: SortingGroup이 없습니다.");
            if (_collider == null)
                Debug.LogError($"[RD_TowerDragHandler] {name}: Collider2D가 없습니다.");
#endif
        }

        private void Update()
        {
            switch (_state)
            {
                case PointerState.None:     HandleNone();     break;
                case PointerState.Pressed:  HandlePressed();  break;
                case PointerState.Dragging: HandleDragging(); break;
            }
        }

        private void HandleNone()
        {
            if (!IsPointerDown()) return;

            Vector3 worldPos = PointerToWorld();
            bool isOverMe = _collider != null && _collider.OverlapPoint(worldPos);

            if (IsPointerOverUI()) return;

            if (isOverMe && !s_anyDragging)
            {
                var gm = RD_GameManager.instance;
                if (gm == null || gm.state is GameState.GameOver or GameState.Clear) return;

                _pressWorldPos = worldPos;
                _state         = PointerState.Pressed;
                ShowInfo();
            }
            else if (s_selectedHandler == this)
            {
                HideInfo();
            }
        }

        private void HandlePressed()
        {
            if (IsPointerUp())
            {
                _state = PointerState.None;
                return;
            }

            if (Vector3.Distance(PointerToWorld(), _pressWorldPos) > DragThreshold)
            {
                if (BeginDrag())
                    _state = PointerState.Dragging;
                else
                    CancelPress();
            }
        }

        private void HandleDragging()
        {
            transform.position = PointerToWorld() + _dragOffset;
            RD_RangeIndicator.ShowAt(transform.position, _towerBase.attackRange);
            RD_TowerInfoPanel.Track(_towerBase);
            RD_SellZone.UpdateHover(transform.position);
            if (IsPointerUp())
                EndDrag();
        }

        private void ShowInfo()
        {
            s_selectedHandler = this;
            RD_RangeIndicator.ShowAt(transform.position, _towerBase.attackRange);
            RD_TowerInfoPanel.Show(_towerBase);
            RD_HUD.instance?.ShowSellPrice(_towerBase);
        }

        private void HideInfo()
        {
            if (s_selectedHandler == this) s_selectedHandler = null;
            RD_RangeIndicator.Hide();
            RD_TowerInfoPanel.Hide();
            RD_HUD.instance?.HideSellPrice();
        }

        public static void ClearSelection()
        {
            s_selectedHandler = null;
        }

        private void OnDestroy()
        {
            if (s_selectedHandler == this) s_selectedHandler = null;
        }

        private static bool IsPointerOverUI()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }

        private void CancelPress()
        {
            _state = PointerState.None;
            HideInfo();
        }

        private bool BeginDrag()
        {
            _originSlot = FindOriginSlot();
            if (_originSlot == null) return false;

            _originSlot.Remove();
            _dragOffset   = transform.position - PointerToWorld();
            s_anyDragging = true;

            RD_GameManager.instance?.timeMgr?.SetDragOverride(true);
            SetSortingOrder(100);

            RD_SellZone.Show(_towerBase);
            RD_HUD.instance?.SetDragMode(true);
            return true;
        }

        private void EndDrag()
        {
            _state        = PointerState.None;
            s_anyDragging = false;
            SetSortingOrder(0);

            RD_GameManager.instance?.timeMgr?.SetDragOverride(false);

            bool inSellZone = RD_SellZone.Contains(transform.position);
            RD_SellZone.Hide();
            RD_HUD.instance?.SetDragMode(false);

            if (inSellZone)
            {
                HideInfo();
                RD_GameManager.instance?.towerMgr?.SellTower(_towerBase);
                return;
            }

            Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);

            RD_TowerSlot targetSlot  = null;
            RD_TowerBase targetTower = null;

            foreach (var col in hits)
            {
                if (col.TryGetComponent<RD_TowerSlot>(out var slot))
                    targetSlot = slot;
                if (col.TryGetComponent<RD_TowerBase>(out var tower) && tower != _towerBase)
                    targetTower = tower;
            }

            if (targetTower != null)
            {
                var towerMgr = RD_GameManager.instance?.towerMgr;
                if (towerMgr != null && towerMgr.TryMerge(_towerBase, targetTower))
                {
                    RD_MergeEffect.PlayAt(targetTower.transform.position, targetTower.grade);
                    HideInfo();
                    return;
                }

                if (targetSlot != null) { if (!TrySwap(targetSlot)) ReturnToOrigin(); }
                else ReturnToOrigin();
                HideInfo();
                return;
            }

            if (targetSlot != null)
            {
                if (targetSlot.isOccupied) { if (!TrySwap(targetSlot)) ReturnToOrigin(); }
                else { if (!targetSlot.TryPlace(_towerBase)) ReturnToOrigin(); }
                HideInfo();
                return;
            }

            ReturnToOrigin();
            HideInfo();
        }

        private bool TrySwap(RD_TowerSlot targetSlot)
        {
            if (_originSlot == null) return false;
            RD_TowerBase other = targetSlot.Remove();
            if (!targetSlot.TryPlace(_towerBase))
            {
                if (other != null) targetSlot.TryPlace(other);
                return false;
            }
            if (other != null) _originSlot.TryPlace(other);
            return true;
        }

        private void ReturnToOrigin()
        {
            if (_originSlot == null) return;
            _originSlot.TryPlace(_towerBase);
        }

        private RD_TowerSlot FindOriginSlot()
        {
            var towerMgr = RD_GameManager.instance?.towerMgr;
            if (towerMgr == null) return null;
            foreach (var slot in towerMgr.allSlots)
                if (slot.placedTower == _towerBase) return slot;
            return null;
        }

        private void SetSortingOrder(int order)
        {
            if (_sortingGroup != null) _sortingGroup.sortingOrder = order;
        }

        private static bool IsPointerDown()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }

        private static bool IsPointerUp()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) return true;
            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) return true;
            return false;
        }

        private static Vector3 PointerToWorld()
        {
            var cam = RD_GameManager.instance?.cam;
            if (cam == null) return Vector3.zero;

            Vector2 screenPos;
            if (Mouse.current != null)
                screenPos = Mouse.current.position.ReadValue();
            else if (Touchscreen.current != null)
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            else
                return Vector3.zero;

            Vector3 pos   = new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z);
            Vector3 world = cam.ScreenToWorldPoint(pos);
            world.z = 0f;
            return world;
        }
    }
}
