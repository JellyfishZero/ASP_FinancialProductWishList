using System.Data;
using System.Data.Common;
using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Repositories.Interfaces;

namespace ASP_FinancialProductWishList.Repositories.Implementations
{
    public class LikeListRepository : ILikeListRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public LikeListRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<LikeListItem>> GetByUserIdAsync(
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            var items = new List<LikeListItem>();

            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_LikeList_GetByUserId";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserID", DbType.Int64, userID);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(MapLikeListItem(reader));
            }

            return items;
        }

        public async Task<LikeListItem?> GetByIdAndUserIdAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_LikeList_GetByIdAndUserId";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@LikeListID", DbType.Int64, likeListID);

            AddParameter(command, "@UserID", DbType.Int64, userID);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken) ? MapLikeListItem(reader) : null;
        }

        public async Task<LikeListItem> CreateAsync(
            long userID,
            int productID,
            int quantity,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_LikeList_Create";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserID", DbType.Int64, userID);

            AddParameter(command, "@ProductID", DbType.Int32, productID);

            AddParameter(command, "@Quantity", DbType.Int32, quantity);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "新增喜好項目後，Stored Procedure 未回傳資料。"
                );
            }

            return MapLikeListItem(reader);
        }

        public async Task<LikeListItem> UpdateAsync(
            long likeListID,
            long userID,
            int productID,
            int quantity,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_LikeList_Update";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@LikeListID", DbType.Int64, likeListID);

            AddParameter(command, "@UserID", DbType.Int64, userID);

            AddParameter(command, "@ProductID", DbType.Int32, productID);

            AddParameter(command, "@Quantity", DbType.Int32, quantity);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "修改喜好項目後，Stored Procedure 未回傳資料。"
                );
            }

            return MapLikeListItem(reader);
        }

        public async Task DeleteAsync(
            long likeListID,
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_LikeList_Delete";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@LikeListID", DbType.Int64, likeListID);

            AddParameter(command, "@UserID", DbType.Int64, userID);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is not bool isDeleted || !isDeleted)
            {
                throw new InvalidOperationException(
                    "刪除喜好項目後，Stored Procedure 未回傳成功結果。"
                );
            }
        }

        private static void AddParameter(DbCommand command, string name, DbType type, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value;

            command.Parameters.Add(parameter);
        }

        private static LikeListItem MapLikeListItem(DbDataReader reader)
        {
            return new LikeListItem
            {
                LikeListID = reader.GetInt64(reader.GetOrdinal("LikeListID")),
                UserID = reader.GetInt64(reader.GetOrdinal("UserID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                FeeRate = reader.GetDecimal(reader.GetOrdinal("FeeRate")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                DebitAccount = reader.GetString(reader.GetOrdinal("DebitAccount")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
            };
        }
    }
}
