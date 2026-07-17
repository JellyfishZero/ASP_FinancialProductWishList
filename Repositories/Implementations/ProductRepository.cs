using System.Data;
using System.Data.Common;
using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Repositories.Interfaces;

namespace ASP_FinancialProductWishList.Repositories.Implementations
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<Product>> GetListAsync(
            CancellationToken cancellationToken = default
        )
        {
            var products = new List<Product>();

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Product_GetList";
            command.CommandType = CommandType.StoredProcedure;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(
            int productID,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_Product_GetById";
            command.CommandType = CommandType.StoredProcedure;

            var productIDParameter = command.CreateParameter();
            productIDParameter.ParameterName = "@ProductID";
            productIDParameter.DbType = DbType.Int32;
            productIDParameter.Value = productID;
            command.Parameters.Add(productIDParameter);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return MapProduct(reader);
        }

        private static Product MapProduct(DbDataReader reader)
        {
            return new Product
            {
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                FeeRate = reader.GetDecimal(reader.GetOrdinal("FeeRate")),
            };
        }
    }
}
