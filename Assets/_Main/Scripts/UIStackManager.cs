using System.Collections.Generic;
using UnityEngine;

namespace MiniGames.Main
{
    public class UIStackManager : MonoBehaviour
    {
        private readonly Stack<GameObject> _stack = new();

        public bool canPop  => _stack.Count > 1;
        public bool isEmpty => _stack.Count == 0;

        public void Push(GameObject panel)
        {
            if (panel == null) return;
            if (_stack.Count > 0)
                _stack.Peek().SetActive(false);

            panel.SetActive(true);
            _stack.Push(panel);
        }

        public bool Pop()
        {
            if (!canPop) return false;

            _stack.Pop().SetActive(false);
            _stack.Peek().SetActive(true);
            return true;
        }

        public void ClearAll()
        {
            while (_stack.Count > 0)
                _stack.Pop().SetActive(false);
        }

        public void SetRoot(GameObject panel)
        {
            ClearAll();
            Push(panel);
        }
    }
}
