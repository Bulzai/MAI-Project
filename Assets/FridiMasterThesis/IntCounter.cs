using System;
using UnityEngine;

public class IntCounter 
{
    private int value;
    public int Value { get => value; }  // Key fix here

    public void Increment(int amount = 1) => value += amount;
    public void Reset() => value = 0;
}