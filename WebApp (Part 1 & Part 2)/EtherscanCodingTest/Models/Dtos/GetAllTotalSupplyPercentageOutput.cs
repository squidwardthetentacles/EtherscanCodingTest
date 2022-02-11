namespace EtherscanCodingTest.Models.Dtos
{
    public class GetAllTotalSupplyPercentageOutput
    {
        public List<TokenTotalSupplyPercentageDto> TokenTotalSupplyPercentages { get; set; }

        public string Message { get; set; }
    }
}
