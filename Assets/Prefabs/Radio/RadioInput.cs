using System;
using UnityEngine;

namespace LD60
{
    public class RadioInput : MonoBehaviour
    {
        public static event Action<ERadioButton> ButtonPressed;
        
        public static void Press(ERadioButton value)
        {
            ButtonPressed?.Invoke(value);
        }
    }
}
