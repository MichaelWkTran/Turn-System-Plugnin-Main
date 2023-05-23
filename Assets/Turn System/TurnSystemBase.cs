using System.Collections.Generic;
using UnityEngine;

public abstract class TurnSystemBase : MonoBehaviour
{
    public static TurnSystemBase m_singleton { get; private set; }
    protected StateMachine m_machine = new StateMachine();
    public uint m_turnNumber { get; private set; } = 0U; //Number of turns performed since the battle as begun
    
    //Player and enemy information
    [Header("Player and Enemy information")]
    public List<UnitBase> m_players, m_enemies, m_neutral; //The stored player, enemy, neutral units
    
    //Selecting enemies or players to target
    [Header("Unit and Move Selection information")]
    protected int m_unitTurnIndex = 0; //Used to iterate through m_unitTurnOrder to perform the action of the unit in the array
    protected UnitBase[] m_unitTurnOrder; //Array that orders the Units from fastest to slowest
    public UnitBase[] UnitTurnOrder { get { return m_unitTurnOrder; } }
    
    //BattleUI
    [Header("UI")]
    [SerializeField] protected RectTransform m_battleUI;

    virtual protected void Awake()
    {
        //Ensure that this component is a singleton
        if (m_singleton == null) m_singleton = this;
        else
        {
            Debug.LogWarning("There must only be one TurnSystem object in a scene. To maintain this, the TurnSystem component in " + gameObject.name + " is destroyed");
            Destroy(this);
        }
    }

    virtual protected void Start()
    {
        //Setup State Machine
        m_machine.m_states = new Dictionary<string, StateMachine.State>();

        //Find Units
        foreach (UnitBase unit in FindObjectsOfType<UnitBase>(true)) AddUnit(unit);
    }

    virtual protected void Update()
    {
        //Update State Machine
        m_machine.Update();
    }

    public void Won()
    {
        m_machine.End();
        Debug.Log("Won");
    }

    public void Lost()
    {
        m_machine.End();
        Debug.Log("Lost");
    }

    public void AddUnit(UnitBase _unit)
    {
        static void AddToList(ref List<UnitBase> _list, UnitBase _unit)
        {
            if (_list.Contains(_unit)) return;
            _list.Add(_unit);
        }

        switch (_unit.m_unitType)
        {
            case UnitBase.UnitType.Player: AddToList(ref m_players, _unit); break;
            case UnitBase.UnitType.Enemy:  AddToList(ref m_enemies, _unit); break;
            default:                   AddToList(ref m_neutral, _unit); break;
        }
    }
}