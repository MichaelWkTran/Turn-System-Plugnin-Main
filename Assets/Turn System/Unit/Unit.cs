using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    public enum UnitType
    {
        Player, //Player unit types will appear on the player side of the battle
        Enemy, //Enemy unit types will appear on the enemy side of the battle and are required to be defeated to end the battle
        Neutral, //Neutral unit types will appear on the enemy side of the battle but are not required to be defeated to end the battle
        NeutralPlayer, //Same as neutral but spawns on the player side
    }

    public string m_unitName; //The name of the unit
    public UnitType m_unitType; //What side is the unit on
    public UnityEvent OnUpdate; //How would the unit repond when their stats are modified

    //Unit stats
    [SerializeField] float m_maxHealth;   [ReadOnly, SerializeField] float m_health;
    [SerializeField] float m_minSpeed;    [SerializeField]           float m_maxSpeed;    [ReadOnly, SerializeField] float m_speed;
    
    //Moves List
    public List<Move> m_unitMoves;

    //Battle varribles
    public Move m_moveSelected;
    public Unit m_targetUnit;

    //Animations
    [System.Serializable] public struct UnitAnimation
    {
        public string m_name;
        public AnimationClip m_clip;
    }
    public UnitAnimation[] m_unitAnimations;

    //Methods
    void Awake()
    {
        m_health   = m_maxHealth;
        m_speed    = m_maxSpeed;
    }

    void Start()
    {
        TurnSystem turnSystem = FindObjectOfType<TurnSystem>();
        turnSystem.AddUnit(this);
    }

    void OnDestroy()
    {
        TurnSystem turnSystem = FindObjectOfType<TurnSystem>();
        if (turnSystem != null)
        {
            //Remove the unit from turnsystem list if destroyed
            switch(m_unitType)
            {
                case UnitType.Player: turnSystem.m_players.Remove(this); break;
                case UnitType.Enemy: turnSystem.m_enemies.Remove(this); break;
                default: turnSystem.m_neutral.Remove(this); break;
            }
        }
    }

    void SetMaxFloatStat(float _value, ref float _currentVariable, ref float _maxVariable)
    {
        _maxVariable = _value;
        _currentVariable = Mathf.Clamp(_currentVariable, 0.0f, _maxVariable);
    }

    void SetFloatStat(float _value, ref float _currentVariable, float _maxVariable, float _minVariable = 0.0f)
    {
        _currentVariable = Mathf.Clamp(_value, _minVariable, _maxVariable);
    }

    //Get Set Methods
    public float MaxHealth
    {
        get { return m_maxHealth; }
        set { SetMaxFloatStat(value, ref m_health, ref m_maxHealth); }
    }

    public float Health
    {
        get { return m_health; }
        set { SetFloatStat(value, ref m_health, m_maxHealth); }
    }

    public float MaxSpeed
    {
        get { return m_maxSpeed; }
        set { SetMaxFloatStat(value, ref m_speed, ref m_maxSpeed); }
    }

    public float Speed
    {
        get { return m_speed; }
        set { SetFloatStat(value, ref m_speed, m_maxSpeed); }
    }

    public void SetMoveSelected(int _moveIndex)
    {
        m_moveSelected = m_unitMoves[_moveIndex];
    }
}