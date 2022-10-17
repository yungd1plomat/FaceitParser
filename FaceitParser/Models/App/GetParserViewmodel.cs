namespace FaceitParser.Models.App
{
    public class GetParserViewmodel
    {
        public string Account { get; set; }

        public int Games { get; set; }

        public int Parsed { get; set; }

        public int Delay { get; set; }

        public int Total { get; set; }

        public int Added { get; set; }

        public IEnumerable<string> Logs { get; set; }
    }
}
