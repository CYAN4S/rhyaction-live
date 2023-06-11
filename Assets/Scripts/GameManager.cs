using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYAN4S
{
    public class GameManager : MonoBehaviour
    {
        public void GoToSelect()
        {
            SceneManager.LoadScene("Select");
        }

        public void GoToEditor()
        {
            SceneManager.LoadScene("Editor");
        }

        public void GoToTitle()
        {
            SceneManager.LoadScene("Title");
        }

        public void Exit()
        {
            Debug.Log("Application Quit!");
            Application.Quit();
        }
    }
}
