using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

/*
Notes to prevent confusion on the turn system
 - A turn the state where actions is performed by a unit whether that would be using an item or attacking
 - 
*/

public class TurnSystem : MonoBehaviour
{
    #region Classes, Enums and Structs
    public enum BattleState { SelectMove, SelectTarget, ExecuteMoves, }

    //The position of a player unit
    [Serializable] public class PlayerPositionsClass
    {
        public Vector3[] m_positions;
    }

    //Turn system UI
    [Serializable] protected struct CommandUI
    {
        public RectTransform m_pannel;
        public Button m_backButton;
    }
    [Serializable] protected struct FightUI
    {
        public RectTransform m_pannel;
        public Button m_moveButtonPrefab;
        public VerticalLayoutGroup m_moveLayoutGroup;
    }
    [Serializable] protected struct SelectTargetUI
    {
        public RectTransform m_pannel;
    }

    #endregion
    
    StateMachine m_machine = new StateMachine();
    public BattleState CurrentBattleState
    {
        get
        {
            return (BattleState)Enum.Parse(typeof(BattleState), m_machine.CurrentStateName);
        }
    }

    //Player and enemy information
    [Header("Player and Enemy information")]
    public List<UnitStats> m_players, m_enemies, m_neutral; //The stored player & enemy units
    [SerializeField] PlayerPositionsClass[] m_playerPositions; //The position of the player units when a battle starts
    public PlayerPositionsClass[] PlayerPositions
    {
        get { return m_playerPositions; }
        set
        {
            m_playerPositions = value;
            CheckPlayerPositionsValidity();
        }
    }

    //Selecting enemies or players to target
    [Header("Unit and Move Selection information")]
    int m_playerSelectIndex = 0; //The index of the player unit who the player is currently selecting a move for

    public UnitStats[] m_unitTurnOrder { get; private set; } //Array that orders the Units from fastest to slowest
    int m_unitTurnIndex = 0; //Used to iterate through m_unitTurnOrder to perform the action of the unit in the array

    public uint m_turnNumber { get; private set; } = 0U; //Number of turns performed since the battle as begun

    //Other Variables
    PlayableDirector m_director;
    IEnumerator m_coroutine = null;

    //BattleUI
    [Header("UI")]
    [SerializeField] RectTransform m_battleUI;
    [SerializeField] CommandUI m_commandUI;
    [SerializeField] FightUI m_fightUI;
    [SerializeField] SelectTargetUI m_selectTargetUI;

#if UNITY_EDITOR
    [Header("Debug")]
    public int m_playerPositionsDebugIndex;
    public UnitStats m_selectedTarget;
#endif

    void Awake()
    {
        //Assign Components
        m_director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        //Find Units
        List<UnitStats> neutralPlayers = new List<UnitStats>();
        foreach (UnitStats unit in FindObjectsOfType<UnitStats>(true))
            switch (unit.m_unitType)
            {
                case UnitStats.UnitType.Player: m_players.Add(unit); break;
                case UnitStats.UnitType.Enemy: m_enemies.Add(unit); break;
                case UnitStats.UnitType.Neutral: m_neutral.Add(unit); break;
                case UnitStats.UnitType.NeutralPlayer: m_neutral.Add(unit); neutralPlayers.Add(unit); break;
            }
        
        //Ensure GoToPreviousTurnButton is disabled
        m_commandUI.m_backButton.gameObject.SetActive(false);

        //Prepare Players
        for (int i = 0; i < m_players.Count; i++)
        {
            UnitStats unit = m_players[i];

            //Sets the player positions
            unit.transform.position = m_playerPositions[m_players.Count-1].m_positions[i];
        }

        //Prepare Neuteral Players
        for (int i = 0; i < neutralPlayers.Count; i++)
        {
            UnitStats unit = neutralPlayers[i];

            //Sets the neuteral positions
            unit.transform.position = m_playerPositions[m_players.Count + neutralPlayers.Count - 2].m_positions[i];
        }

        //Set up state machine
        m_machine.m_states = new Dictionary<string, StateMachine.State>
        {
            { BattleState.SelectMove.ToString(),   new StateMachine.State(SelectMoveStart, null, SelectMoveExit) },
            { BattleState.SelectTarget.ToString(), new StateMachine.State(SelectTargetStart, null, SelectTargetExit) },
            { BattleState.ExecuteMoves.ToString(), new StateMachine.State(ExecuteMovesStart) }
        };

        //Start state machine
        m_machine.Start(m_machine.m_states[BattleState.SelectMove.ToString()]);
    }

    void Update()
    {
        //Update State Machine
        m_machine.Update();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        //Draw Unit Gizmos for player positions
        {
            PlayerPositionsClass playerPositionsClass = m_playerPositions[m_playerPositionsDebugIndex];
            for (int posIndex = 0; posIndex < playerPositionsClass.m_positions.Length; posIndex++)
            {
                GUIStyle guiStye = new GUIStyle();
                guiStye.fontSize = 30;
                guiStye.normal.textColor = Color.white;

                Gizmos.DrawIcon(playerPositionsClass.m_positions[posIndex], "Turn System/Unit Gizmo.png", false);
                Handles.Label(playerPositionsClass.m_positions[posIndex], posIndex.ToString(), guiStye);
            }
        }
    }
#endif

    #region State Methods
    #region Select Move
    void SelectMoveStart()
    {
        //Reset turn system
        if (m_playerSelectIndex >= m_players.Count)
        {
            m_playerSelectIndex = 0;
        }

        //Enables/Disables UI
        m_battleUI.gameObject.SetActive(true);
        m_commandUI.m_pannel.gameObject.SetActive(true);
        m_fightUI.m_pannel.gameObject.SetActive(false);
        m_commandUI.m_backButton.gameObject.SetActive(m_playerSelectIndex > 0);

        IEnumerator SelectMovesCoroutine()
        {
            //Skip Player Units that have no health
            for (; m_playerSelectIndex < m_players.Count; m_playerSelectIndex++)
            {
                if (m_players[m_playerSelectIndex].Health >= 0.0f) break;

                //Lose the game if all players have no health
                if (m_playerSelectIndex + 1 >= m_players.Count)
                {
                    Lost();
                    m_coroutine = null;
                    yield break;
                }
            }

            //Update fight UI
            foreach (Transform child in m_fightUI.m_moveLayoutGroup.transform) Destroy(child.gameObject);
            for (int i = 0; i < m_players[m_playerSelectIndex].m_UnitActionMoves.Count; i++)
            {
                Button moveButton = Instantiate(m_fightUI.m_moveButtonPrefab, m_fightUI.m_moveLayoutGroup.transform);
                Move move = m_players[m_playerSelectIndex].m_UnitActionMoves[i];

                int moveIndex = i;
                moveButton.onClick.AddListener(delegate
                {
                    m_players[m_playerSelectIndex].m_MoveSelected = moveIndex;
                    StartCoroutine(m_coroutine);
                });
                moveButton.GetComponentInChildren<TMP_Text>().text = move.name;
            }

            //Pause Coroutine for the player to select a move
            StopCoroutine(m_coroutine);
            yield return null;

            //Transition to select target state
            m_machine.CurrentState = m_machine.m_states[BattleState.SelectTarget.ToString()];
        }

        //Start Coroutine
        m_coroutine = SelectMovesCoroutine();
        StartCoroutine(m_coroutine);
    }

    void SelectMoveExit()
    {
        //End coroutine and set stored coroutine to null when it has finished
        m_coroutine = null;
    }

    public void CommandBackButtonClick()
    {
        //Go to Select Move from a previous player unit while ignoring players that have no health
        for (m_playerSelectIndex--; m_playerSelectIndex > 0; m_playerSelectIndex--)
            if (m_players[m_playerSelectIndex].Health > 0.0f)
                { m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()]; return; }
    }
    #endregion

    #region Select Target
    void SelectTargetStart()
    {
        //Enables/Disables UI
        m_commandUI.m_pannel.gameObject.SetActive(false);
        m_fightUI.m_pannel.gameObject.SetActive(false);
        m_selectTargetUI.m_pannel.gameObject.SetActive(true);
    }

    void SelectTargetExit()
    {
        m_selectTargetUI.m_pannel.gameObject.SetActive(false);
    }

    public void SelectTarget(UnitStats _selectedTarget)
    {
        //Dont select target if not in the appropriate state
        if (m_machine.CurrentStateName != BattleState.SelectTarget.ToString()) return;
        
        //Set the target unit for the player
        m_players[m_playerSelectIndex].m_TargetUnit = _selectedTarget;
        m_playerSelectIndex++;

        //If all players have selected a moves and a target, then move to the execute moves state
        if (m_playerSelectIndex >= m_players.Count) m_machine.CurrentState = m_machine.m_states[BattleState.ExecuteMoves.ToString()];
        //Move to the select move state for the next player unit
        else m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()];
    }

    public void SelectTargetBackButtonClick()
    {
        //Dont execute if not in the appropriate state
        if (m_machine.CurrentStateName == BattleState.SelectTarget.ToString()) return;

        //Reselect move of the player unit
        m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()];
    }
    #endregion

    #region Execute Moves
    void ExecuteMovesStart()
    {
        //For Every enemy select a move
        foreach (UnitStats enemy in m_enemies)
        {
            enemy.m_TargetUnit = m_players[UnityEngine.Random.Range(0, m_players.Count-1)];
            enemy.m_MoveSelected = UnityEngine.Random.Range(0, enemy.m_UnitActionMoves.Count-1);
        }

        //Order Units in order of speed
        m_unitTurnOrder = new UnitStats[m_players.Count + m_enemies.Count + m_neutral.Count];
        m_players.CopyTo(m_unitTurnOrder, 0);
        m_enemies.CopyTo(m_unitTurnOrder, m_players.Count);
        m_neutral.CopyTo(m_unitTurnOrder, m_players.Count + m_enemies.Count);

        Array.Sort
        (
            m_unitTurnOrder,
            delegate (UnitStats _left, UnitStats _right)
            {
                float GetUnitSpeed(ref UnitStats _unit) { return _unit.Speed + _unit.MoveSelected.Speed; }
                return (int)((GetUnitSpeed(ref _right) - GetUnitSpeed(ref _left)) * 100);
            }
        );

        //Disables BattleUI
        m_battleUI.gameObject.SetActive(false);

        //Trigger moves for all units
        IEnumerator ExecuteMovesCoroutine()
        {
            //Loop though all units
            for (m_unitTurnIndex = 0; m_unitTurnIndex < m_unitTurnOrder.Length; m_unitTurnIndex++)
            {
                UnitStats executorUnit = m_unitTurnOrder[m_unitTurnIndex];
                UnitStats targetUnit = executorUnit.m_TargetUnit;

                //Skip Player Units that have no health
                if (executorUnit.Health <= 0.0f) continue;

                //Play move animations of units
                m_director.playableAsset = executorUnit.MoveSelected.Timeline;
                executorUnit.MoveSelected.SetUpPlayableDirector(m_director, executorUnit, targetUnit);
                m_director.Play();

                //Wait for the animation to finish
                yield return new WaitForSeconds((float)executorUnit.MoveSelected.Timeline.duration);

                //Ensure the animation has finished
                m_director.Stop();
            }

            //Transition to select target state
            m_machine.CurrentState = m_machine.m_states[BattleState.SelectMove.ToString()];
        }

        StartCoroutine(ExecuteMovesCoroutine());
    }
    #endregion

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
    #endregion

    public void CheckPlayerPositionsValidity()
    {
        int playerPositionIndex = 0;
        foreach (PlayerPositionsClass playerPosition in m_playerPositions)
        {
            if (playerPosition.m_positions.Length != playerPositionIndex + 1)
                Array.Resize(ref playerPosition.m_positions, playerPositionIndex + 1);

            playerPositionIndex++;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TurnSystem))]
public class TurnSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TurnSystem turnSystem = (TurnSystem)target;

        //Check player position validity
        turnSystem.CheckPlayerPositionsValidity();

        //Set player positions debug index
        turnSystem.m_playerPositionsDebugIndex = Mathf.Clamp(turnSystem.m_playerPositionsDebugIndex, 0, turnSystem.PlayerPositions.Length-1);

        //Select a target unit
        if (GUILayout.Button("Set Target Unit"))
        {
            turnSystem.SelectTarget(turnSystem.m_selectedTarget);
        }
    }

    protected virtual void OnSceneGUI()
    {
        TurnSystem turnSystem = (TurnSystem)target;

        //Only show below when turn system is selected in editor
        if (Selection.Contains(turnSystem)) return;

        //Set player position via handles
        {
            TurnSystem.PlayerPositionsClass[] playerPositions = turnSystem.PlayerPositions;

            EditorGUI.BeginChangeCheck();
            TurnSystem.PlayerPositionsClass playerPositionsClass = playerPositions[turnSystem.m_playerPositionsDebugIndex];
            for (int posIndex = 0; posIndex < playerPositionsClass.m_positions.Length; posIndex++)
            {
                playerPositionsClass.m_positions[posIndex] = Handles.PositionHandle
                    (playerPositionsClass.m_positions[posIndex], Quaternion.identity);
            }

            if (EditorGUI.EndChangeCheck()) turnSystem.PlayerPositions = playerPositions;
        }   
    }
}
#endif