using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NewDateTimeField : MonoBehaviour
{
    static readonly List<string> Months =
        new() {
            "Jan",
            "Fev",
            "Mar",
            "Abr",
            "Mai",
            "Jun",
            "Jul",
            "Ago",
            "Set",
            "Out",
            "Nov",
            "Dez"
        };
    public TMP_Dropdown dayDropdown, monthDropdown, hourDropdown, minuteDropdown;
    DateTime _actualValue;
    bool _isUpdating, _interactable = true;
    public UnityEvent<DateTime> OnValueChanged { get; set; } = new();
    public bool Interactable {
        get => _interactable;
        set {
            _interactable = value;
            dayDropdown.interactable = value;
            monthDropdown.interactable = value;
            hourDropdown.interactable = value;
            minuteDropdown.interactable = value;
        }
    }
    public DateTime Value {
        get => _actualValue;
        set {
            if (_isUpdating || _actualValue == value) return;
            _isUpdating = true;
            _actualValue = value;
            UpdateDropdowns();
            OnValueChanged.Invoke(value);
            _isUpdating = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        monthDropdown.SetOptions(Months);
        monthDropdown.onValueChanged.AddListener(MonthChanged);
        MonthChanged(monthDropdown.value);
        dayDropdown.onValueChanged.AddListener(_ => ParseDropdowns());
        hourDropdown.SetOptions(Utilities.PopulateList(24));
        minuteDropdown.SetOptions(Utilities.PopulateList(60));
        hourDropdown.onValueChanged.AddListener(_ => ParseDropdowns());
        minuteDropdown.onValueChanged.AddListener(_ => ParseDropdowns());
    }

    void UpdateDropdowns() {
        monthDropdown.value = _actualValue.Month - 1;
        dayDropdown.value = _actualValue.Day - 1;
        hourDropdown.value = _actualValue.Hour;
        minuteDropdown.value = _actualValue.Minute;
    }

    void MonthChanged(int index) {
        int[] thirtyOneDays = { 0, 2, 4, 6, 7, 9, 11 };
        int[] thirtyDays = { 3, 5, 8, 10 };
        if (thirtyOneDays.Contains(index)) {
            PopulateDayDropdown(31);
            return;
        }
        if (thirtyDays.Contains(index)) {
            PopulateDayDropdown(30);
            return;
        }
        PopulateDayDropdown(28);
        ParseDropdowns();
    }

    void ParseDropdowns() {
        Value =
            new DateTime(
                2000, monthDropdown.value + 1, dayDropdown.value + 1, hourDropdown.value, minuteDropdown.value, 0
            );
    }

    void PopulateDayDropdown(int daysAmount) { dayDropdown.SetOptions(Utilities.PopulateList(daysAmount, 1)); }
}