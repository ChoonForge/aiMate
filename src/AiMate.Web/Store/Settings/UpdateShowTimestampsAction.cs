namespace AiMate.Web.Store.Settings
{
    public class UpdateShowTimestampsAction
    {
        public bool ShowTimestamps { get; }

        public UpdateShowTimestampsAction(bool showTimestamps)
        {
            ShowTimestamps = showTimestamps;
        }
    }
}