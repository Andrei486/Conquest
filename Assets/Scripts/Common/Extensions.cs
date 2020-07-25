using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using InBattle;
using Objects;

public static class Extensions
{
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length==j) ? Arr[0] : Arr[j];            
    }

    public static T[][] ToJagged<T>(this T[,] value) {
        if (System.Object.ReferenceEquals(null, value))
        return null;

        // Jagged array creation
        T[][] result = new T[value.GetLength(0)][];

        for (int i = 0; i < value.GetLength(0); ++i) 
        result[i] = new T[value.GetLength(1)];

        // Jagged array filling
        for (int i = 0; i < value.GetLength(0); ++i)
        for (int j = 0; j < value.GetLength(1); ++j)
            result[i][j] = value[i, j];

        return result;
    }

    public static Vector2 Rotate(this Vector2 v2, float angle)
    {
        /**Rotates the vector by angle degrees COUNTERCLOCKWISE.!--*/
        angle *= Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * v2.x - Mathf.Sin(angle) * v2.y;
        float y = Mathf.Sin(angle) * v2.x + Mathf.Cos(angle) * v2.y;
        return new Vector2(x, y);
    }

    public static Color BorderColor(this UnitAffiliation affiliation){
        Color color;
        switch (affiliation){
            case UnitAffiliation.PLAYER:
                color = new Color(0f, 0.2f, 0.9f); //blue
                break;
            case UnitAffiliation.ENEMY:
                color = new Color(0.8f, 0.1f, 0.1f); //red
                break;
            case UnitAffiliation.ALLY:
                color = new Color(0.3f, 0.7f, 0.1f); //forest green
                break;
            case UnitAffiliation.OTHER:
                color = new Color(0.8f, 0.7f, 0f); //golden yellow
                break;
            default:
                color = Color.black;
                break;
        }
        return color;
    }

    public static Color BgColor(this UnitAffiliation affiliation){
        Color color;
        switch (affiliation){
            case UnitAffiliation.PLAYER:
                color = new Color(0.85f, 0.9f, 1f); //light blue
                break;
            case UnitAffiliation.ENEMY:
                color = new Color(1f, 0.85f, 0.85f); //light red
                break;
            case UnitAffiliation.ALLY:
                color = new Color(0.85f, 1f, 0.85f); //light green
                break;
            case UnitAffiliation.OTHER:
                color = new Color(1f, 0.9f, 0.8f); //golden yellow
                break;
            default:
                color = Color.white;
                break;
        }
        return color;
    }

    public static T NextOf<T>(this IList<T> list, T item){
        var indexOf = list.IndexOf(item);
        return list[indexOf == list.Count - 1 ? 0 : indexOf + 1];
    }
    public static T PreviousOf<T>(this IList<T> list, T item){
        var indexOf = list.IndexOf(item);
        return list[indexOf == 0 ? list.Count - 1 : indexOf - 1];
    }

    public static bool ApproximatelyEqual(this Quaternion quatA, Quaternion quatB, float acceptableRange = (float)(1e-5)){
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, quatB)) < acceptableRange;
    } 

    public static T GetByDescription<T>(this string description){
        var type = typeof(T);
        if(!type.IsEnum) throw new InvalidOperationException();
        foreach(var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) as DescriptionAttribute;
            if(attribute != null)
            {
                if(attribute.Description == description)
                    return (T)field.GetValue(null);
            }
            else
            {
                if(field.Name == description)
                    return (T)field.GetValue(null);
            }
        }
        throw new ArgumentException("Not found.", nameof(description));
    }

    public static string GetDescription<T>(this T element){
        /**Returns the description of the given enum element.!--*/
        if(!typeof(T).IsEnum) throw new InvalidOperationException();
        FieldInfo fi = element.GetType().GetField(element.ToString());

        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        if (attributes.Any())
        {
            return attributes.First().Description;
        }

        return element.ToString();
    }
}