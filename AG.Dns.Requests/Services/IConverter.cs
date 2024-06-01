namespace AG.Dns.Requests.Services
{
    public interface IConverter
    {
        List<string> Query(string query);
    }
}
