using UnityEngine;
using UnityEngine.SceneManagement;
public class menu_buttons : MonoBehaviour
{
    public void game_start()
    {
        SceneManager.LoadScene("game");
    }

    public void exit_game()
    {
        Application.Quit();
    }
}
