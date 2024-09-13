namespace WebsiteApi.Apis.Hubs;

public class HubBadRequest
{
    public string Text { get; set; }
    public HubBadRequest(string text)
    {
        Text = text;
    }

    public override string ToString()
    {
        return Text;

    }
}