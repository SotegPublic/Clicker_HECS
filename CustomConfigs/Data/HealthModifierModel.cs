using System;
using UnityEngine;

[Serializable]
public class HealthModifierModel
{
    [SerializeField] private int level;
    [SerializeField] private float modifier;

    public int Level => level;
    public float Modifier => modifier;
}
