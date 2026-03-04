using System;
using UnityEngine;

[Serializable] public class IntCounter 
{
    [SerializeField] private int value;
    public int Value => value;

    public void Increment(int amount = 1) => value += amount;
    public void Reset() => value = 0;
}
