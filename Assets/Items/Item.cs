using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Item", menuName = "Inventory and Items/Create New Item")]
public class Item : ScriptableObject
{
    [SerializeField] string m_INVName;
    [TextArea] [SerializeField] string m_Description;
    [SerializeField] Sprite m_Icon;

    //How many of that item can be held
    [SerializeField] byte m_Capacity;

    //Item special affects (Optional)
    [SerializeField] string m_Properties;
    //How much health is added or subtracted
    [SerializeField] float m_Health;
    //How much the attack stat is added or subtracted
    [SerializeField] float m_Attack;
    //How much the defence stat is added or subtracted
    [SerializeField] float m_Defence;
    //How much the speed stat is added or subtracted
    [SerializeField] float m_Speed;
    //How much the evasion stat is added or subtracted
    [SerializeField] float m_Evasion;
    //How much the accuracy stat is added or subtracted
    [SerializeField] float m_Accuracy;
    //How much the item effect is going to last
    [SerializeField] float m_Time;

    public string INVName { get { return m_INVName; } }
    public string Description { get { return m_Description; } }
    public Sprite Icon { get { return m_Icon; } }
    public byte Capacity { get { return m_Capacity; } }
    public string Properties { get { return m_Properties; } }
    public float Health { get { return m_Health; } }
    public float Attack { get { return m_Attack; } }
    public float Defence { get { return m_Defence; } }
    public float Speed { get { return m_Speed; } }
    public float Evasion { get { return m_Evasion; } }
    public float Accuracy { get { return m_Accuracy; } }
    public float Time { get { return m_Time; } }
}
