// Copyright CodeGamified 2025-2026
// MIT License — Aufstand
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aufstand.Scripting
{
    /// <summary>
    /// Input provider — keyboard input for manual play / debugging.
    /// </summary>
    public class AufstandInputProvider : MonoBehaviour
    {
        public static AufstandInputProvider Instance { get; private set; }

        public float CurrentInput { get; private set; }

        private InputAction _moveAction;

        private void Awake()
        {
            Instance = this;

            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.Enable();
        }

        private void Update()
        {
            Vector2 move = _moveAction.ReadValue<Vector2>();
            if (move.sqrMagnitude > 0.01f)
                CurrentInput = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
            else
                CurrentInput = -1f;
        }

        private void OnDestroy()
        {
            _moveAction?.Disable();
            _moveAction?.Dispose();
            if (Instance == this)
                Instance = null;
        }
    }
}
