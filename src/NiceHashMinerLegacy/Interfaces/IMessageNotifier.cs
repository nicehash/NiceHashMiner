namespace NiceHashMiner.Interfaces
{
    /// <summary>
    /// IMessageNotifier interface is for message setting.
    /// </summary>
    public interface IMessageNotifier
    {
        void SetMessage(string infoMsg);
        void SetMessageAndIncrementStep(string infoMsg);
    }
}
