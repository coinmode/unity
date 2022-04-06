using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public delegate void YearSelected(int year);

    [AddComponentMenu("CoinMode/UI/CM DatePickerYearButton")]
    public class DatePickerYearButton : CoinModeUIBehaviour
    {
        [SerializeField]
        private Text yearText = null;

        [SerializeField]
        private Button button = null;

        public YearSelected onYearSelected;

        private int year = 1;

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(OnClick);
        }

        public void SetYear(int year)
        {
            this.year = year;
            yearText.text = year.ToString();
        }

        public void OnClick()
        {
            onYearSelected?.Invoke(year);   
        }

        public void Select()
        {
            yearText.fontSize = 42;
        }

        public void Deselect()
        {
            yearText.fontSize = 32;
        }
    }
}