using System;
using UnityEngine;

[Serializable]
public class ExperienceModifierModel
{
    [SerializeField] private int level;
    [SerializeField] private float modifier;

    public int Level => level;
    public float Modifier => modifier;
}
