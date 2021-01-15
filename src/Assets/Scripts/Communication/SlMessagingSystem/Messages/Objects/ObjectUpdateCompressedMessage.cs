
public class ObjectUpdateCompressedMessage : ObjectUpdateMessage
{
    public ObjectUpdateCompressedMessage()
    {
        Id = MessageId.ObjectUpdateCompressed;
        Flags = 0;
        Frequency = MessageFrequency.High;

        UpdateType = ObjectUpdateType.OUT_FULL_COMPRESSED;
    }

    //TODO: Decompress data and pass it to the base class DeSerialise
}