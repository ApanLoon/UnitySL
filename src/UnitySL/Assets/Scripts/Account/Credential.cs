
using System.Globalization;

public class Credential
{
    public Credential(string name, string secret)
    {
        string first = "";
        string last = "resident";
        string[] parts = name.Split(' ');
        switch (parts.Length)
        {
            case 1:
                first = parts[0];
                break;

            case 2:
                first = parts[0];
                last = parts[1];
                break;

            default:
                Logger.LogWarning("Credential.Constructor", $"Invalid name \"{name}\".");
                break;
        }

        First = first;
        Last = last;
        Secret = secret;
    }

    public Credential(string first, string last, string secret)
    {
        First = first;
        Last = last;
        Secret = secret;
    }

    public string First { get; set; }
    public string Last { get; set; }
    public string Secret { get; set; }
}
