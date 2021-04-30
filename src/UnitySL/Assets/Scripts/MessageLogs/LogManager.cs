namespace Assets.Scripts.MessageLogs
{
    public class LogManager
    {
        public static LogManager Instance = new LogManager();

        public MessageLog DebugLog = new MessageLog(log => Logger.OnLog                                     += (level, senderName, message) => log.AddMessage(new DebugMessage(level, senderName, message)));
        public MessageLog ChatLog  = new MessageLog(log => EventManager.Instance.OnChatFromSimulatorMessage += (message)                    => log.AddMessage(new ChatMessage(message)));

    }
}
