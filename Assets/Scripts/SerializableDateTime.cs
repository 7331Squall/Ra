using UnityEditor;
using UnityEngine;
using System;

[Serializable]
public class SerializableDateTime {
    public int year = 2026;
    public int month = 3;
    public int day = 10;

    public int hour = 14;
    public int minute = 30;

    public SerializableDateTime(DateTime dt) {
        year = dt.Year;
        month = dt.Month;
        day = dt.Day;
        hour = dt.Hour;
        minute = dt.Minute;
    }
    
    public DateTime Value {
        get { return new DateTime(year, month, day, hour, minute, 0); }
    }
}

[CustomPropertyDrawer(typeof(SerializableDateTime))]
public class SerializableDateTimeDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, label);

        float fieldWidth = 40;
        float spacing = 4;

        Rect r = new Rect(position.x, position.y, fieldWidth, position.height);

        var year = property.FindPropertyRelative("year");
        var month = property.FindPropertyRelative("month");
        var day = property.FindPropertyRelative("day");
        var hour = property.FindPropertyRelative("hour");
        var minute = property.FindPropertyRelative("minute");

        year.intValue = EditorGUI.IntField(r, year.intValue);
        r.x += fieldWidth + spacing;

        month.intValue = EditorGUI.IntField(r, month.intValue);
        r.x += fieldWidth + spacing;

        day.intValue = EditorGUI.IntField(r, day.intValue);
        r.x += fieldWidth + spacing * 3;

        hour.intValue = EditorGUI.IntField(r, hour.intValue);
        r.x += fieldWidth + spacing;

        EditorGUI.LabelField(new Rect(r.x, r.y, 10, r.height), ":");
        r.x += 10;

        minute.intValue = EditorGUI.IntField(r, minute.intValue);

        EditorGUI.EndProperty();
    }
}