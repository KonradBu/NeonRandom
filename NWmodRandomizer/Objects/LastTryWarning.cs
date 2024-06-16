using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Steamworks;
using MelonLoader;

namespace NeonRandom
{
    internal class LastTryWarning : MonoBehaviour
    {
        private readonly Rect _rectTotal = new Rect(10, 100, 100, 70);
        private readonly GUIStyle _style = new GUIStyle()
        {
            font = Resources.Load("fonts/nova_mono/novamono") as Font,
            fontSize = 20
        };

        public void Initialize()
        {

        }
        private void Start()
        {
            _style.normal.textColor = Color.red;
        }
        
        private void OnGUI()
        {
            if (NeonRandom.Setting_NeonRandom_WarningEnabled.Value && NeonRandom.lasttry)
            {
                String text = "Last Try!";
                GUI.Label(_rectTotal, text, _style);
            }        
        }
    }
}
