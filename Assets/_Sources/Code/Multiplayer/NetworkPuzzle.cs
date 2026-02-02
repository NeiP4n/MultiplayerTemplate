using UnityEngine;
using UnityEngine.Events;
using PurrNet;
using TriInspector;

namespace Sources.Code.Multiplayer
{
    [RequireComponent(typeof(NetworkIdentity))]
    [DeclareBoxGroup("Setup")]
    [DeclareBoxGroup("Buttons")]
    [DeclareBoxGroup("Plates")]
    [DeclareBoxGroup("Code")]
    [DeclareBoxGroup("Runtime", Title = "Runtime State")]
    public sealed class NetworkPuzzle : NetworkBehaviour
    {
        [Group("Setup")]
        [SerializeField] private NetworkDoor linkedDoor;

        [Group("Buttons")]
        [SerializeField] private int requiredButtonPresses = 0;

        [Group("Plates")]
        [SerializeField] private int requiredPlateCount = 0;

        [Group("Code")]
        [SerializeField] private string requiredCode = "";

        [Group("Setup")]
        [SerializeField] private UnityEvent onSolved;

        [Group("Runtime"), ReadOnly, ShowInInspector]
        public bool IsSolved { get; private set; }

        [Group("Runtime"), ReadOnly, ShowInInspector]
        private int buttonPressCount;

        [Group("Runtime"), ReadOnly, ShowInInspector]
        private int plateCount;

        [Group("Runtime"), ReadOnly, ShowInInspector]
        private string currentCode = "";

        // ================= CLIENT ENTRY =================

        public void RequestButtonPress()
        {
            if (NetworkManager.main.isServer)
                ButtonPressServer();
            else
                Server_RequestButtonPress();
        }

        public void RequestPlateChange(bool added)
        {
            if (NetworkManager.main.isServer)
                PlateChangeServer(added);
            else
                Server_RequestPlateChange(added);
        }

        public void RequestCodeAppend(string symbol)
        {
            if (NetworkManager.main.isServer)
                CodeAppendServer(symbol);
            else
                Server_RequestCodeAppend(symbol);
        }

        public void RequestCodeSubmit()
        {
            if (NetworkManager.main.isServer)
                CodeSubmitServer();
            else
                Server_RequestCodeSubmit();
        }

        // ================= SERVER RPC =================

        [ServerRpc] private void Server_RequestButtonPress() => ButtonPressServer();
        [ServerRpc] private void Server_RequestPlateChange(bool added) => PlateChangeServer(added);
        [ServerRpc] private void Server_RequestCodeAppend(string symbol) => CodeAppendServer(symbol);
        [ServerRpc] private void Server_RequestCodeSubmit() => CodeSubmitServer();

        // ================= SERVER LOGIC =================

        private void ButtonPressServer()
        {
            if (IsSolved)
                return;

            buttonPressCount++;
            Debug.Log($"[PUZZLE] Button pressed | Count: {buttonPressCount}");

            CheckSolve();
        }

        private void PlateChangeServer(bool added)
        {
            if (IsSolved)
                return;

            plateCount += added ? 1 : -1;
            plateCount = Mathf.Max(0, plateCount);

            Debug.Log($"[PUZZLE] Plate change | Added: {added} | Count: {plateCount}");

            CheckSolve();
        }

        private void CodeAppendServer(string symbol)
        {
            if (IsSolved)
                return;

            currentCode += symbol;

            Debug.Log($"[PUZZLE] Code append: {symbol}");
            Debug.Log($"[PUZZLE] Current code: {currentCode}");
        }

        private void CodeSubmitServer()
        {
            if (IsSolved)
                return;

            Debug.Log($"[PUZZLE] Submit pressed");
            Debug.Log($"[PUZZLE] Required code: {requiredCode}");
            Debug.Log($"[PUZZLE] Current code:  {currentCode}");

            if (!string.IsNullOrEmpty(requiredCode) &&
                currentCode == requiredCode)
            {
                Debug.Log("[PUZZLE] CODE CORRECT");
                SolveServer();   // ← СРАЗУ РЕШАЕМ
            }
            else
            {
                Debug.Log("[PUZZLE] CODE WRONG - RESET");
                currentCode = "";
            }
        }


        private void CheckSolve()
        {
            if (IsSolved)
                return;

            // Если используется код — кнопки и плиты игнорируем
            if (!string.IsNullOrEmpty(requiredCode))
                return;

            bool buttonsOk = requiredButtonPresses == 0 || buttonPressCount >= requiredButtonPresses;
            bool platesOk = requiredPlateCount == 0 || plateCount >= requiredPlateCount;

            Debug.Log($"[PUZZLE] CheckSolve (no code mode)");
            Debug.Log($"  Buttons OK: {buttonsOk}");
            Debug.Log($"  Plates OK:  {platesOk}");

            if (buttonsOk && platesOk)
                SolveServer();
        }

        private void SolveServer()
        {
            if (IsSolved)
                return;

            Debug.Log("[PUZZLE] PUZZLE SOLVED");

            IsSolved = true;

            if (linkedDoor != null)
                linkedDoor.SetStateServer(true);

            onSolved?.Invoke();
            BroadcastSolved();
        }

        // ================= SYNC =================

        [ObserversRpc]
        private void BroadcastSolved()
        {
            if (NetworkManager.main.isServer)
                return;

            Debug.Log("[PUZZLE] Client received solved state");

            IsSolved = true;
            onSolved?.Invoke();
        }
    }
}
