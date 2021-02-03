public class Grid
{
    public string Id { get; protected set; }
    public string Key { get; protected set; }
    public string Label { get; protected set; }
    public string LoginUri { get; protected set; }
    public string HelperUri { get; protected set; }
    public string LoginPageUri { get; protected set; }
    public string UpdateUri { get; protected set; }
    public string WebProfileUri { get; protected set; }
    public string SlurlBase { get; protected set; }
    public bool IsSystemGrid { get; protected set; }
    public Grid(string id, string key, string label, string loginUri, string helperUri, string loginPageUri, string updateUri, string webProfileUri, string slurlBase, bool isSystemGrid = false)
    {
        Id = id;
        Key = key;
        Label = label;
        LoginUri = loginUri;
        HelperUri = helperUri;
        LoginPageUri = loginPageUri;
        UpdateUri = updateUri;
        WebProfileUri = webProfileUri;
        SlurlBase = slurlBase;
        IsSystemGrid = isSystemGrid;
    }
}
