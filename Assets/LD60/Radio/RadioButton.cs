using UnityEngine;

namespace LD60
{
    public class RadioButton : MonoBehaviour
    {
        [SerializeField] private ERadioButton value;

        public void Press()
        {
            RadioInput.Press(value);
        }
    }
}
