using EtherscanCodingTest.Models.Tokens;

namespace EtherscanCodingTest.Models.Dtos
{
    public class GetOutput
    {
        public TokenModel Token { get; set; }

        public string Message { get; set; }
    }
}
