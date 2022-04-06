using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM DateInputField")]
    public class CoinModeDateInputField : CoinModeUIBehaviour
    {
        public enum InputMode
        {
            FieldControl,
            Picker,
        }

        public enum DateDisplayMode
        {
            DayMonthYear,
            MonthDayYear,
        }

        [SerializeField]
        private InputMode inputMode = InputMode.Picker;

        [SerializeField]
        private CoinModeInputField dayInputField = null;

        [SerializeField]
        private CoinModeInputFieldControl dayInputControl = null;

        [SerializeField]
        private CoinModeInputField monthInputField = null;

        [SerializeField]
        private CoinModeInputFieldControl monthInputControl = null;

        [SerializeField]
        private CoinModeInputField yearInputField = null;

        [SerializeField]
        private CoinModeInputFieldControl yearInputControl = null;

        [SerializeField]
        private Button openPickerButton = null;

        [SerializeField]
        private CoinModeText dateLabel = null;

        [SerializeField]
        private DateDisplayMode dateFormat = DateDisplayMode.DayMonthYear;

        [SerializeField]
        private bool allowFuture = false;

        private DateTime date = new DateTime();
        private bool fixing = false;

        private int dayInt = 1;
        private int monthInt = 1;
        private int yearInt = 1;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            OrderDateInputs();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            if(dayInputField != null)
            {
                dayInputField.onEndEdit.AddListener(OnEndEdit);
                dayInputField.onValueChanged.AddListener(OnValueUpdated);
            }
            if (monthInputField != null)
            {
                monthInputField.onEndEdit.AddListener(OnEndEdit);
                monthInputField.onValueChanged.AddListener(OnValueUpdated);
            }
            if (yearInputField != null)
            {
                yearInputField.onEndEdit.AddListener(OnEndEdit);
                yearInputField.onValueChanged.AddListener(OnValueUpdated);
            }

            switch (inputMode)
            {
                case InputMode.FieldControl:
                    dayInputControl.gameObject.SetActive(true);
                    monthInputControl.gameObject.SetActive(true);
                    yearInputControl.gameObject.SetActive(true);
                    openPickerButton.gameObject.SetActive(false);
                    break;
                case InputMode.Picker:
                    dayInputControl.gameObject.SetActive(false);
                    monthInputControl.gameObject.SetActive(false);
                    yearInputControl.gameObject.SetActive(false);
                    openPickerButton.gameObject.SetActive(true);
                    break;
            }

            openPickerButton.onClick.AddListener(OpenPicker);

            OrderDateInputs();
            SetDate(DateTime.Now);
        }

        public void SetPlaceholderDate(DateTime date)
        {
            SetPlaceholderDate(date.Day.ToString(), date.Month.ToString(), date.Year.ToString());
        }

        private void SetPlaceholderDate(string day, string month, string year)
        {
            dayInputField.SetPlaceholderText(day);
            monthInputField.SetPlaceholderText(month);
            yearInputField.SetPlaceholderText(year);
        }

        public void SetDate(DateTime date)
        {
            this.date = date;
            SetInputDate(date.Day.ToString(), date.Month.ToString(), date.Year.ToString());
            dateLabel.text = date.ToLongDateString();
        }

        private void SetInputDate(string day, string month, string year)
        {
            dayInputField.text = day;
            monthInputField.text = month;
            yearInputField.text = year;
        }                

        public DateTime GetDate()
        {
            return date;
        }

        public void SetInputTextColor(Color color)
        {
            dayInputField.SetInputTextColor(color);
            monthInputField.SetInputTextColor(color);
            yearInputField.SetInputTextColor(color);
        }

        public void SetPlaceholderColor(Color color)
        {
            dayInputField.SetPlaceholderColor(color);
            monthInputField.SetPlaceholderColor(color);
            yearInputField.SetPlaceholderColor(color);
        }

        public void SetDateFormat(DateDisplayMode dateFormat)
        {
            this.dateFormat = dateFormat;
            OrderDateInputs();
        }

        private void OrderDateInputs()
        {
            switch (dateFormat)
            {
                case DateDisplayMode.DayMonthYear:
                    dayInputField.transform.SetSiblingIndex(0);
                    dayInputControl.transform.SetSiblingIndex(1);
                    monthInputField.transform.SetSiblingIndex(2);
                    monthInputControl.transform.SetSiblingIndex(3);
                    break;
                case DateDisplayMode.MonthDayYear:
                    monthInputField.transform.SetSiblingIndex(0);
                    monthInputControl.transform.SetSiblingIndex(1);
                    dayInputField.transform.SetSiblingIndex(2);
                    dayInputControl.transform.SetSiblingIndex(3);
                    break;
            }
        }

        private void OnEndEdit(string value)
        {
            if (!fixing)
            {
                CheckAndFixDate();
                ConstructDateFromInput(dayInputField.text, monthInputField.text, yearInputField.text);
                dateLabel.text = date.ToLongDateString();
            }
        }

        private void OnValueUpdated(string value)
        {

        }

        private void CheckAndFixDate()
        {
            // Prevents recursion on edit delegates
            fixing = true;

            bool needsFix = false;            

            int.TryParse(monthInputField.text, out monthInt);            
            int.TryParse(yearInputField.text, out yearInt);            
            int.TryParse(dayInputField.text, out dayInt);

            DateTime now = DateTime.UtcNow;

            bool future = false;
            if (!allowFuture)
            {                
                if (yearInt >= now.Year)
                {
                    if(yearInt > now.Year)
                    {
                        yearInt = now.Year;
                        monthInt = now.Month;
                        dayInt = now.Day;
                        future = true;
                    }
                    else
                    {                        
                        if (monthInt >= now.Month)
                        {
                            if (monthInt > now.Month)
                            {
                                monthInt = now.Month;
                                dayInt = now.Day;
                                future = true;
                            }
                            else
                            {
                                if (dayInt > now.Day)
                                {
                                    dayInt = now.Day;
                                    future = true;
                                }
                            }                            
                        }
                    }                    
                }
            }            

            if((!future && !allowFuture) || allowFuture)
            {
                if (monthInt < 1 || monthInt > 12)
                {
                    needsFix = true;
                    monthInt = monthInt < 1 ? 1 : 12;
                }

                if (yearInt < 1900 || yearInt > now.Year + 100)
                {
                    needsFix = true;
                    yearInt = yearInt < 1900 ? 1900 : now.Year + 100;
                }

                int daysInMonth = DateTime.DaysInMonth(yearInt, monthInt);
                if (dayInt < 1 || dayInt > daysInMonth)
                {
                    needsFix = true;
                    dayInt = dayInt < 1 ? 1 : daysInMonth;
                }
            }            

            if (needsFix || (future && !allowFuture))
            {
                dayInputField.SetInputText(dayInt.ToString());
                monthInputField.SetInputText(monthInt.ToString());
                yearInputField.SetInputText(yearInt.ToString());
            }

            fixing = false;
        }

        private void OpenPicker()
        {
            CoinModeMenu.OpenDatePicker(GetDate(), SetDate, allowFuture);
        }

        private void ConstructDateFromInput(string day, string month, string year)
        {
            date = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
        }
    }
}