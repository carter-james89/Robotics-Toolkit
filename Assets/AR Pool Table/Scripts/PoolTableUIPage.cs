using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ARPoolTrainer
{
    public class PoolTableUIPage : MonoBehaviour
    {
        [Header("Pool Table Reference")]
        [SerializeField] private ARPoolTable _poolTable;

        [Header("Pocket Selection Toggles")]
        [SerializeField] private Toggle _SWPocketToggle;
        [SerializeField] private Toggle _NWPocketToggle;
        [SerializeField] private Toggle _NPocketToggle;
        [SerializeField] private Toggle _NEPocketToggle;
        [SerializeField] private Toggle _SEPocketToggle;
        [SerializeField] private Toggle _SPocketToggle;

        [Header("Bank Rail Toggles")]
        [SerializeField] private Toggle _northBankToggle;
        [SerializeField] private Toggle _southBankToggle;
        [SerializeField] private Toggle _eastBankToggle;
        [SerializeField] private Toggle _westBankToggle;

        private Dictionary<Toggle, ARPoolTable.TargetPocket> _pocketToggleMap;
        private Dictionary<Toggle, ARPoolTable.BankRail> _bankToggleMap;

        void Start()
        {
            // --- Pocket toggles setup ---
            _pocketToggleMap = new Dictionary<Toggle, ARPoolTable.TargetPocket>
            {
                { _SWPocketToggle, ARPoolTable.TargetPocket.SW },
                { _NWPocketToggle, ARPoolTable.TargetPocket.NW },
                { _NPocketToggle,  ARPoolTable.TargetPocket.N  },
                { _NEPocketToggle, ARPoolTable.TargetPocket.NE },
                { _SEPocketToggle, ARPoolTable.TargetPocket.SE },
                { _SPocketToggle,  ARPoolTable.TargetPocket.S  },
            };

            // Initialize pocket toggles state
            var currentPocket = _poolTable.GetTargetPocket();
            foreach (var kv in _pocketToggleMap)
                kv.Key.isOn = (kv.Value == currentPocket);

            // Register listeners for pocket toggles
            foreach (var kv in _pocketToggleMap)
                kv.Key.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        OnPocketToggleSelected(kv.Key);
                });

            // --- Bank rail toggles setup ---
            _bankToggleMap = new Dictionary<Toggle, ARPoolTable.BankRail>
            {
                { _northBankToggle, ARPoolTable.BankRail.North },
                { _southBankToggle, ARPoolTable.BankRail.South },
                { _eastBankToggle,  ARPoolTable.BankRail.East  },
                { _westBankToggle,  ARPoolTable.BankRail.West  },
            };

            // Initialize bank toggles state
            var currentBank = _poolTable.GetBankRail();
            foreach (var kv in _bankToggleMap)
                kv.Key.isOn = (kv.Value == currentBank);

            // Register listeners for bank toggles
            foreach (var kv in _bankToggleMap)
                kv.Key.onValueChanged.AddListener(isOn =>
                {
                    OnBankToggleChanged(kv.Key, isOn);
                });
        }

        private void OnPocketToggleSelected(Toggle selected)
        {
            // Turn off all other pocket toggles
            foreach (var kv in _pocketToggleMap)
                if (kv.Key != selected)
                    kv.Key.isOn = false;

            // Update the pool table's target pocket
            _poolTable.SetTargetPocket(_pocketToggleMap[selected]);
        }

        private void OnBankToggleChanged(Toggle toggled, bool isOn)
        {
            if (isOn)
            {
                // Turn off all other bank toggles
                foreach (var kv in _bankToggleMap)
                    if (kv.Key != toggled)
                        kv.Key.isOn = false;

                // Apply selected bank rail
                _poolTable.SetBankRail(_bankToggleMap[toggled]);
            }
            else
            {
                // Clicking an already-on toggle clears it
                _poolTable.SetBankRail(ARPoolTable.BankRail.None);
            }
        }
    }
}
