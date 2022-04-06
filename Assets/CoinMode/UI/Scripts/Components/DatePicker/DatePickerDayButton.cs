using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public delegate void DaySelected(int day);

    [AddComponentMenu("CoinMode/UI/CM DatePickerDayButton")]
    public class DatePickerDayButton : CoinModeUIBehaviour
    {
        [SerializeField]
        private Text dateText = null;

        [SerializeField]
        private Button button = null;

        [SerializeField]
        private Image selectedImage = null;

        public DaySelected onDaySelected;

        private int day = 1;

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(OnClick);
        }

        public void SetDay(int day)
        {
            this.day = day;
            dateText.text = day.ToString();
        }        

        public void OnClick()
        {
            onDaySelected?.Invoke(day);
        }

        public void Select()
        {
            selectedImage.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            selectedImage.gameObject.SetActive(false);
        }

        public void Disable()
        {
            button.interactable = false;
        }

        public void Enable()
        {
            button.interactable = true;
        }
    }
}