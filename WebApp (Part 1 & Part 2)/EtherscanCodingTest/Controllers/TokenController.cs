using EtherscanCodingTest.Models.Dtos;
using EtherscanCodingTest.Models.Tokens;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace EtherscanCodingTest.Controllers
{
    public class TokenController : Controller
    {
        private readonly IConfiguration _configuration;

        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Query

        [HttpGet]
        public IActionResult GetAll(int skipCount, int maxResultCount)
        {
            var output = new GetAllOutput();
            var tokens = new List<TokenModel>();
            try
            {
                using (MySqlConnection conn =
                    new MySqlConnection(_configuration["ConnectionString"]))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(@$"
                        -- Get Token Count
                        SELECT COUNT(*) as token_count 
                        FROM token;

                        -- Get Tokens
                        SELECT 
                            id, 
                            symbol, 
                            name, 
                            total_supply, 
                            contract_address,
                            total_holders,
                            price,
	                        ROUND(total_supply * 100 / 
                                    (SELECT SUM(total_supply) 
                                     FROM etherscancodingtestdb.token), 5) 
                            AS total_supply_percentage
                        FROM token
                        ORDER BY total_supply_percentage DESC
                        LIMIT {skipCount}, {maxResultCount};
                    ", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        output.TotalCount = reader.GetInt32("token_count");

                        reader.NextResult();

                        int rank = 1;
                        while (reader.Read()) 
                        {
                            tokens.Add(new TokenModel
                            {
                                Id = reader.GetInt32("id"),
                                Symbol = reader.GetString("symbol"),
                                Name = reader.GetString("name"),
                                TotalSupply = reader.GetInt32("total_supply"),
                                ContractAddress = reader.GetString("contract_address"),
                                TotalHolders = reader.GetInt32("total_holders"),
                                Price = reader.GetDecimal("price"),
                                TotalSupplyPercentage = reader.GetFloat("total_supply_percentage"),
                                Rank = rank++
                            });
                        }

                        output.Tokens = tokens;
                    }

                    return Json(output);
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }

            return Json(output);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var output = new GetOutput();
            var token = new TokenModel();
            try
            {
                using (MySqlConnection conn =
                    new MySqlConnection(_configuration["ConnectionString"]))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(@$"
                        -- Get Token
                        SELECT 
                            id, 
                            symbol, 
                            name, 
                            total_supply, 
                            contract_address,
                            total_holders,
                            price
                        FROM token
                        WHERE id = {id};
                    ", conn);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {
                            token = new TokenModel
                            {
                                Id = reader.GetInt32(0),
                                Symbol = reader.GetString(1),
                                Name = reader.GetString(2),
                                TotalSupply = reader.GetInt32(3),
                                ContractAddress = reader.GetString(4),
                                TotalHolders = reader.GetInt32(5),
                                Price = reader.GetDecimal(6)
                            };
                        }
                    }

                    output.Token = token;

                    return Json(output);
                }
            }
            catch(Exception ex)
            {
                output.Message = ex.Message;
            }

            return Json(output);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTotalSupplyPercentage()
        {
            var output = new GetAllTotalSupplyPercentageOutput();
            var tokenTotalSupplyPercentages = new List<TokenTotalSupplyPercentageDto>();
            try
            {
                using (MySqlConnection conn =
                    new MySqlConnection(_configuration["ConnectionString"]))
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(@$"
                        -- Get Tokens
                        SELECT 
                            name,
	                        ROUND(total_supply * 100 / 
                                    (SELECT SUM(total_supply) 
                                     FROM etherscancodingtestdb.token), 5) 
                            AS total_supply_percentage
                        FROM token
                        ORDER BY total_supply_percentage DESC;
                    ", conn);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tokenTotalSupplyPercentages.Add(new TokenTotalSupplyPercentageDto
                            {
                                Name = reader.GetString(0),
                                TotalSupplyPercentage = reader.GetFloat(1)
                            });
                        }

                        output.TokenTotalSupplyPercentages = tokenTotalSupplyPercentages;
                    }

                    return Json(output);
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
            }

            return Json(output);
        }

        private async Task<int> FindIdBySymbol(string symbol)
        {
            symbol = symbol.ToUpper();

            using (MySqlConnection conn =
                new MySqlConnection(_configuration["ConnectionString"]))
            {
                var id = 0;

                conn.Open();

                MySqlCommand cmd = new MySqlCommand(@$"
                    -- Get Token Id
                    SELECT id
                    FROM token
                    WHERE symbol = ?symbol;
                ", conn);

                cmd.Parameters.Add(new MySqlParameter("symbol", symbol));

                using (var reader = cmd.ExecuteReader())
                {
                    if (await reader.ReadAsync())
                    {
                        id = reader.GetInt32("id");
                    }
                }

                return id;
            }
        }

        #endregion

        #region Command

        private async Task<int> Create(TokenModel token)
        {
            using (MySqlConnection conn =
                new MySqlConnection(_configuration["ConnectionString"]))
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(@$"
                    -- Create Token
                    INSERT INTO token (`symbol`, `name`, `total_supply`, `contract_address`, `total_holders`) 
                    VALUES (?symbol, ?name, ?total_supply, ?contract_address, ?total_holders);
                ", conn);

                cmd.Parameters.Add(new MySqlParameter("symbol", token.Symbol));
                cmd.Parameters.Add(new MySqlParameter("name", token.Name));
                cmd.Parameters.Add(new MySqlParameter("total_supply", token.TotalSupply));
                cmd.Parameters.Add(new MySqlParameter("contract_address", token.ContractAddress));
                cmd.Parameters.Add(new MySqlParameter("total_holders", token.TotalHolders));

                var result = await cmd.ExecuteNonQueryAsync();
                
                return result;
            }
        }

        private async Task<int> Update(TokenModel token)
        {
            using (MySqlConnection conn =
                new MySqlConnection(_configuration["ConnectionString"]))
            {
                List<dynamic> dtos = new List<dynamic>();

                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@$"
                    -- Update Token
                    UPDATE token
                    SET symbol = ?symbol, name = ?name, total_supply = ?total_supply, contract_address = ?contract_address, total_holders = ?total_holders
                    WHERE id = {token.Id};
                ", conn);

                cmd.Parameters.Add(new MySqlParameter("symbol", token.Symbol));
                cmd.Parameters.Add(new MySqlParameter("name", token.Name));
                cmd.Parameters.Add(new MySqlParameter("total_supply", token.TotalSupply));
                cmd.Parameters.Add(new MySqlParameter("contract_address", token.ContractAddress));
                cmd.Parameters.Add(new MySqlParameter("total_holders", token.TotalHolders));

                var result = await cmd.ExecuteNonQueryAsync();

                return result;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] TokenModel token)
        {
            var result = 0;
            var message = string.Empty;
            try
            {
                if (token.Id > 0)
                {
                    result = await Update(token);
                }
                else
                {
                    var tokenId = await FindIdBySymbol(token.Symbol);
                    if (tokenId > 0)
                    {
                        token.Id = tokenId;
                        result = await Update(token);
                    }
                    else
                    {
                        result = await Create(token);
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(new { Success = result, Message = message });
        }
        
        #endregion
    }
}
