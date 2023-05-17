using UnityEngine;

public class PlayerParty : MonoBehaviour
{
    static PlayerParty m_singleton;

    void Start()
    {
        //Check singleton
        if (m_singleton) if (m_singleton != this) { Destroy(gameObject); return; }
        else m_singleton = this;

        //Set up PlayerParty
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }
}