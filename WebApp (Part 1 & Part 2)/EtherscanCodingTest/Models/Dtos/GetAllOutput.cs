using EtherscanCodingTest.Models.Tokens;

namespace EtherscanCodingTest.Models.Dtos
{
    public class GetAllOutput
    {
        public List<TokenModel> Tokens { get; set; }

        public int TotalCount { get; set; }

        public string Message { get; set; }
    }
}
