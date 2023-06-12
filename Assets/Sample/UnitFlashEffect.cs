using UnityEngine;

public class UnitFlashEffect : MonoBehaviour
{
    SpriteRenderer m_spriteRenderer;
    Material m_defaultMaterial;
    [SerializeField] Material m_flashMaterial;

    public void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_defaultMaterial = m_spriteRenderer.sharedMaterial;
    }

    public void PointerEnter()
    {
        if (FindObjectOfType<TurnSystem>().CurrentBattleState != TurnSystem.BattleState.SelectTarget) return;
        m_spriteRenderer.material = m_flashMaterial;
    }

    public void PointerExit()
    {
        m_spriteRenderer.material = m_defaultMaterial;
    }
}
