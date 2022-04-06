using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public delegate void DateSelected(DateTime date);

    [AddComponentMenu("CoinMode/UI/CM DatePicker")]
    public class DatePicker : CoinModeUIBehaviour
    {
        public enum Month
        {
            None = 0,
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        public enum Day
        {
            None = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
            Sunday = 7
        }

        private string[] shortDays =
        {
            "Mon",
            "Tue",
            "Wed",
            "Thu",
            "Fri",
            "Sat",
            "Sun"
        };

        private string[] shortMonth =
        {
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sep",
            "Oct",
            "Nov",
            "Dec"
        };

        [SerializeField]
        private bool allowFutureDate = true;

        [SerializeField]
        private int minYear = 1900;

        [SerializeField]
        private bool maxYearIsCurrent = true;

        [SerializeField]
        private int maxYear = 2021;

        [SerializeField]
        private Button viewYearsButton = null;

        [SerializeField]
        private CoinModeText viewYearsText = null;

        [SerializeField]
        private Button viewDatesButton = null;

        [SerializeField]
        private CoinModeText viewDatesText = null;

        [SerializeField]
        private Button previousMonthButton = null;

        [SerializeField]
        private CoinModeText currentMonthText = null;

        [SerializeField]
        private CoinModeText currentYearText = null;

        [SerializeField]
        private Button nextMonthButton = null;

        [SerializeField]
        private GameObject datePanel;

        [SerializeField]
        private GameObject yearsPanel;

        [SerializeField]
        private DatePickerYearButton yearButtonTemplate = null;

        [SerializeField]
        private RectTransform yearButtonContainer = null;

        [SerializeField]
        private ScrollRect yearButtonScrollRect = null;

        [SerializeField]
        private CoinModeButton cancelButton = null;

        [SerializeField]
        private CoinModeButton confirmButton = null;

        [SerializeField]
        private List<DatePickerDayButton> dayButtons = new List<DatePickerDayButton>();

        public DateSelected onDateSelected;

        public DateTime selectedDate { get; private set; } = new DateTime();

        private int viewYear = 2021;
        private int viewMonth = 1;

        private List<DatePickerYearButton> yearButtons = new List<DatePickerYearButton>();
        private DatePickerDayButton selectedDayButton = null;
        private DatePickerYearButton selectedYearButton = null;

        private int startDayButtonIndex = 0;
        private int endDayButtonIndex = 0;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (maxYearIsCurrent || maxYear < DateTime.UtcNow.Year || !allowFutureDate)
            {
                maxYear = DateTime.UtcNow.Year;
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            for(int i = 0; i < dayButtons.Count; i++)
            {
                dayButtons[i].onDaySelected += OnDaySelected;
            }

            if (maxYearIsCurrent || maxYear < DateTime.UtcNow.Year || !allowFutureDate)
            {
                maxYear = DateTime.UtcNow.Year;
            }

            nextMonthButton.onClick.AddListener(IncrementMonth);
            previousMonthButton.onClick.AddListener(DecrementMonth);

            viewDatesButton.onClick.AddListener(ViewDates);
            viewYearsButton.onClick.AddListener(ViewYears);

            confirmButton.onClick.AddListener(OnConfirm);
            cancelButton.onClick.AddListener(OnCancel);

            PopulateYears(minYear, maxYear);
            SetSelectedDate(DateTime.UtcNow);
            ViewDates();
        }

        public void Init(bool allowFuture, int? minYear = null, int? maxYear = null)
        {
            allowFutureDate = allowFuture;
            if(minYear != null)
            {
                this.minYear = minYear.Value;
            }
            if (maxYear != null)
            {
                this.maxYear = maxYear.Value;
            }
        }

        public void SetSelectedDate(DateTime date)
        {
            selectedDate = date;
            SetUpDaysForMonth(selectedDate.Month, selectedDate.Year);
            UpdateDisplayComponents();
        }

        private void UpdateDisplayComponents()
        {                  
            if (selectedYearButton != null)
            {
                selectedYearButton.Deselect();
            }            
            selectedYearButton = GetYearButton(selectedDate.Year);
            selectedYearButton.Select();

            viewYearsText.text = selectedDate.Year.ToString();
            viewDatesText.text = string.Format("{0}, {1} {2}", GetShortDay(ToDay(selectedDate.DayOfWeek)), selectedDate.Day, shortMonth[selectedDate.Month - 1]);
        }

        private void UpdateSelectedDay()
        {
            if (selectedDayButton != null)
            {
                selectedDayButton.Deselect();
            }

            if (viewMonth == selectedDate.Month && viewYear == selectedDate.Year)
            {
                selectedDayButton = GetDayButton(selectedDate.Day);
                selectedDayButton.Select();
            }
        }

        private void PopulateYears(int minYear, int maxYear)
        {
            int count = maxYear - minYear + 1;
            for(int i = 0; i < count; i++)
            {
                DatePickerYearButton yearButton = Instantiate(yearButtonTemplate, yearButtonContainer);
                yearButton.onYearSelected += OnYearSelected;
                yearButton.SetYear(minYear + i);
                yearButton.gameObject.SetActive(true);
                yearButtons.Add(yearButton);
            }
        }

        private void SetUpDaysForMonth(int month, int year)
        {
            viewMonth = month;
            viewYear = year;

            int prevMonth = viewMonth - 1 > 0 ? viewMonth - 1 : 12;
            int prevYear = prevMonth == 12 ? viewYear - 1 : viewYear;
            int prevMonthDaysInMonth = DateTime.DaysInMonth(prevYear, prevMonth);

            int daysInMonth = DateTime.DaysInMonth(viewYear, viewMonth);
            Day startingDay = ToDay(new DateTime(viewYear, viewMonth, 1).DayOfWeek);
            startDayButtonIndex = (int)startingDay - 1;
            endDayButtonIndex = startDayButtonIndex + daysInMonth;

            int dayCount = 1;
            for (int i = startDayButtonIndex; i < endDayButtonIndex; i++)
            {
                dayButtons[i].SetDay(dayCount);
                if(!allowFutureDate)
                {
                    if (IsDateInFuture(dayCount, viewMonth, viewYear))
                    {
                        dayButtons[i].Disable();
                    }
                    else
                    {
                        dayButtons[i].Enable();
                    }
                }
                else
                {
                    dayButtons[i].Enable();
                }                
                dayCount++;
            }

            if(startDayButtonIndex > 0)
            {
                int count = 0;
                for(int i = startDayButtonIndex - 1; i >= 0; i--)
                {
                    dayButtons[i].SetDay(prevMonthDaysInMonth - count);
                    dayButtons[i].Disable();
                    count++;
                }
            }

            if(endDayButtonIndex < dayButtons.Count)
            {
                int count = 1;
                for (int i = endDayButtonIndex; i < dayButtons.Count; i++)
                {
                    dayButtons[i].SetDay(count);
                    dayButtons[i].Disable();
                    count++;
                }
            }

            currentMonthText.text = ((Month)month).ToString();
            currentYearText.text = year.ToString();

            UpdateSelectedDay();
        }

        private void OnDaySelected(int day)
        {
            selectedDate = new DateTime(viewYear, viewMonth, day);
            UpdateSelectedDay();
            UpdateDisplayComponents();
        }

        private void OnYearSelected(int year)
        {
            int daysInMonth = DateTime.DaysInMonth(year, selectedDate.Month);
            int day = selectedDate.Day;

            if (day > daysInMonth)
            {
                day = daysInMonth;
            }

            SetSelectedDate(new DateTime(year, selectedDate.Month, day));
            ViewDates();
        }

        private void IncrementMonth()
        {
            int month = viewMonth + 1 <= 12 ? viewMonth + 1 : 1;
            int year = month == 1 ? viewYear + 1 : viewYear;
            if(year <= maxYear && (allowFutureDate || (!allowFutureDate && !IsDateInFuture(1, month, year))))
            {
                SetUpDaysForMonth(month, year);
            }            
        }

        private void DecrementMonth()
        {
            int month = viewMonth - 1 > 0 ? viewMonth - 1 : 12;
            int year = month == 12 ? viewYear - 1 : viewYear;
            if(year >= minYear)
            {
                SetUpDaysForMonth(month, year);
            }            
        }

        private void ViewDates()
        {
            datePanel.gameObject.SetActive(true);
            yearsPanel.gameObject.SetActive(false);
        }

        private void ViewYears()
        {            
            datePanel.gameObject.SetActive(false);
            yearsPanel.gameObject.SetActive(true);
            ScrollYearToCurrent();
        }

        private DatePickerDayButton GetDayButton(int day)
        {
            return dayButtons[startDayButtonIndex + day - 1];
        }

        private DatePickerYearButton GetYearButton(int year)
        {
            return yearButtons[year - minYear];
        }

        private Day ToDay(DayOfWeek dayOfWeek)
        {
            if ((int)dayOfWeek >= 1)
            {
                return (Day)dayOfWeek;
            }
            else
            {
                return Day.Sunday;
            }
        }

        private string GetShortDay(Day day)
        {
            return shortDays[(int)day - 1];
        }

        private bool IsDateInFuture(int day, int month, int year)
        {
            if(year > DateTime.UtcNow.Year)
            {
                return true;
            }
            else if(year == DateTime.UtcNow.Year)
            {
                if(month > DateTime.UtcNow.Month)
                {
                    return true;
                }
                else if(month == DateTime.UtcNow.Month)
                {
                    if (day > DateTime.UtcNow.Day)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ScrollYearToCurrent()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(yearButtonScrollRect.transform as RectTransform);
            float max = maxYear - minYear + 1;
            float current = maxYear - selectedDate.Year;
            float normalPosition = current / max;
            yearButtonScrollRect.verticalNormalizedPosition = normalPosition;
        }

        private void OnCancel()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirm()
        {
            onDateSelected?.Invoke(selectedDate);
            gameObject.SetActive(false);
        }
    }
}