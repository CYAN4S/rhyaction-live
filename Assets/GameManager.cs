using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYAN4S
{
    public class GameManager : Singleton<GameManager>
    {
        public void GoToSelect()
        {
            SceneManager.LoadScene("Select");
        }
    }
}
