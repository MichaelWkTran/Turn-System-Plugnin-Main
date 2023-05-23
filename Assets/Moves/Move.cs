using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public abstract class MoveBase : ScriptableObject
{
    [SerializeField, TextArea] string m_description; //The description of the move
    public string Description { get { return m_description; } }
}
