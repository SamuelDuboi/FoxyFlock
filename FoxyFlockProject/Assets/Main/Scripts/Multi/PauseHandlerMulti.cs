using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class PauseHandlerMulti : NetworkBehaviour
{

    public GameObject UI;
    public InputManager inputManager;
    public GameObject[] UIToReset;
    // Start is called before the first frame update
    void Start()
    {
        inputManager.OnMenuPressed.AddListener(MenuPressed);
    }
    private void MenuPressed()
    {
        if (UI.activeSelf)
        {
            UI.SetActive(false);
            foreach (GameObject ui in UIToReset)
            {
                ui.SetActive(false);
            }
            UIToReset[UIToReset.Length - 1].SetActive(true);
        }
        else
            UI.SetActive(true);
    }
    public void Resume()
    {
        inputManager.OnMenuPressed.Invoke();
    }
    
 [Command]
 public void CmdReturToLobbye()
    {
        NetworkManagerRace.instance.OnReset();
    }
}
