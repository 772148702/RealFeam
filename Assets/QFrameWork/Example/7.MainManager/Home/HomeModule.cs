namespace QFrameWork
{
    public class HomeModule:MainManager
    {
        protected override void LaunchInDevelopingMode()
        {
           
        }

        protected override void LaunchInProductionMode()
        {
            GameModule.LoadModule();
          
        }

        protected override void LaunchInTestMode()
        {
            GameModule.LoadModule();
         
        }
    }
}