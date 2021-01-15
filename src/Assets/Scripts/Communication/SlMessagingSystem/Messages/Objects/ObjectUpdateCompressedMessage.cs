
public class ObjectUpdateCompressedMessage : ObjectUpdateMessage
{
    public ObjectUpdateCompressedMessage()
    {
        MessageId = MessageId.ObjectUpdateCompressed;
        Flags = 0;

        UpdateType = ObjectUpdateType.OUT_FULL_COMPRESSED;
    }

    //TODO: Decompress data and pass it to the base class DeSerialise
}