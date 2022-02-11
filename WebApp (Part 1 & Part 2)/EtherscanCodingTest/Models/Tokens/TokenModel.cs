namespace EtherscanCodingTest.Models.Tokens
{
    public class TokenModel
    {
        public int Id { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public int TotalSupply { get; set; }

        public string ContractAddress { get; set; }

        public int TotalHolders { get; set; }

        public decimal Price { get; set; }

        public int Rank { get; set; }

        public float TotalSupplyPercentage { get; set; }
    }
}
