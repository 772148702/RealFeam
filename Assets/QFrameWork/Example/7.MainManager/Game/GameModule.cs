using UnityEngine.SceneManagement;

namespace QFrameWork
{
    public class GameModule:MainManager
    {
        public static void LoadModule()
        {
            SceneManager.LoadScene("Game");
        }
        protected override void LaunchInDevelopingMode()
        {
  
        }

        protected override void LaunchInProductionMode()
        {
   
        }

        protected override void LaunchInTestMode()
        {
          
        }
    }
}