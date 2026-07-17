using System.Data;
using System.Data.Common;
using ASP_FinancialProductWishList.Models.Entities;
using ASP_FinancialProductWishList.Repositories.Interfaces;

namespace ASP_FinancialProductWishList.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<(bool UserNameExists, bool EmailExists)> ExistsByUserNameOrEmailAsync(
            string userName,
            string email,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_ExistsByUserNameOrEmail";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserName", DbType.String, userName, 50);

            AddParameter(command, "@Email", DbType.String, email, 254);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("Stored Procedure 未回傳使用者重複檢查結果。");
            }

            return (
                reader.GetBoolean(reader.GetOrdinal("UserNameExists")),
                reader.GetBoolean(reader.GetOrdinal("EmailExists"))
            );
        }

        public async Task<User?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_GetByUserName";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserName", DbType.String, userName, 50);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken)
                ? MapUserWithPasswordHash(reader)
                : null;
        }

        public async Task<User?> GetByIdAsync(
            long userID,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_GetById";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserID", DbType.Int64, userID);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
        }

        public async Task<User> CreateAsync(
            User user,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_Create";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserName", DbType.String, user.UserName, 50);

            AddParameter(command, "@Name", DbType.String, user.Name, 100);

            AddParameter(command, "@Email", DbType.String, user.Email, 254);

            AddParameter(command, "@DebitAccount", DbType.AnsiString, user.DebitAccount, 20);

            AddParameter(command, "@PasswordHash", DbType.String, user.PasswordHash, 500);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "建立使用者後，Stored Procedure 未回傳使用者資料。"
                );
            }

            return MapUser(reader);
        }

        public async Task<User> UpdateDebitAccountAsync(
            long userID,
            string debitAccount,
            CancellationToken cancellationToken = default
        )
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_User_UpdateDebitAccount";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@UserID", DbType.Int64, userID);

            AddParameter(command, "@DebitAccount", DbType.AnsiString, debitAccount, 20);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "更新扣款帳號後，Stored Procedure 未回傳使用者資料。"
                );
            }

            return MapUser(reader);
        }

        private static void AddParameter(
            DbCommand command,
            string name,
            DbType type,
            object? value,
            int? size = null
        )
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value ?? DBNull.Value;

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            command.Parameters.Add(parameter);
        }

        private static User MapUser(DbDataReader reader)
        {
            return new User
            {
                UserID = reader.GetInt64(reader.GetOrdinal("UserID")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                DebitAccount = reader.GetString(reader.GetOrdinal("DebitAccount")),
            };
        }

        private static User MapUserWithPasswordHash(DbDataReader reader)
        {
            var user = MapUser(reader);

            user.PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"));

            return user;
        }
    }
}
