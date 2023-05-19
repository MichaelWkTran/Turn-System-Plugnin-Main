using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Item", menuName = "Inventory and Items/Create New Item")]
public class Item : Move
{
    [SerializeField] Sprite m_icon; //The icon representing the item
    [SerializeField] byte m_capacity; //How many of that item can be held

    public Sprite Icon { get { return m_icon; } }
    public byte Capacity { get { return m_capacity; } }
}
