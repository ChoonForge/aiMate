namespace AiMate.Web.Store.Settings
{
    public class UpdateRememberContextAction
    {
        public bool RememberContext { get; }

        public UpdateRememberContextAction(bool rememberContext)
        {
            RememberContext = rememberContext;
        }
    }
}