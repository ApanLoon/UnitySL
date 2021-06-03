namespace Assets.Scripts.Primitives
{
    /// <summary>
    /// Action associated with clicking on an object
    /// </summary>
    public enum ClickAction : byte
    {
        /// <summary>Touch object</summary>
        Touch = 0,
        /// <summary>Sit on object</summary>
        Sit = 1,
        /// <summary>Purchase object or contents</summary>
        Buy = 2,
        /// <summary>Pay the object</summary>
        Pay = 3,
        /// <summary>Open task inventory</summary>
        OpenTask = 4,
        /// <summary>Play parcel media</summary>
        PlayMedia = 5,
        /// <summary>Open parcel media</summary>
        OpenMedia = 6
    }
}
