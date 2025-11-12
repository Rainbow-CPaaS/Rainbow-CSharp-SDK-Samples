using Microsoft.Extensions.Logging;
using Rainbow;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotRAG.Model
{
    internal class DatabaseManagement
    {
        private static DatabaseManagement? _instance = null;
        private ILogger _log;

        private Application? _rbApplication;
        private SQLiteConnection sqlite_conn;

        private List<String> tableForBubbleCreated;

        /// <summary>
        /// Get instance of <see cref="DatabaseManagement"/> service
        /// </summary>
        public static DatabaseManagement Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        private DatabaseManagement()
        {
            sqlite_conn = CreateConnection();
            tableForBubbleCreated = [];
        }

        public void SetApplication(Application rbApplication)
        {
            if (_rbApplication is not null) return;

            _rbApplication = rbApplication;

            _log = LogFactory.CreateLogger<DatabaseManagement>(rbApplication.LoggerPrefix);
            _log.LogInformation("Application set");
        }

        private SQLiteConnection CreateConnection()
        {
            try
            {
                Directory.CreateDirectory(".\\database");

                // Create a new database connection:
                SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=.\\database\\database.db;Version=3;New=True;Compress=True;");
                
                // Open the connection:
                sqlite_conn.Open();
                return sqlite_conn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void CreateTableForBubble(String bubbleId)
        {
            if (tableForBubbleCreated.Contains(bubbleId))
                return;
            String tableName = "Bubble_" + bubbleId;
            String sql = $"CREATE TABLE IF NOT EXISTS {tableName} (type VARCHAR(1), id VARCHAR(30), status VARCHAR(30), date DATETIME, ragDocumentId VARCHAR(256), PRIMARY KEY (type, id));";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.ExecuteNonQuery();
            tableForBubbleCreated.Add(bubbleId);
        }

        public Boolean InsertElement(String bubbleId, String type, String id, String status, String? ragDocumentId)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();

            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"INSERT OR REPLACE INTO {tableName} (type, id, status, date, ragDocumentId) VALUES (@type, @id, @status, @date, @ragDocumentId);";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@date", DateTime.UtcNow);
            command.Parameters.AddWithValue("@ragDocumentId", ragDocumentId);
            try
            {
               command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public Boolean IsElementWithStatus(String bubbleId, String type, String id, String status)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();

            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT id FROM  {tableName} WHERE type=@type AND id=@id AND status=@status;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@status", status);
            
            Boolean result = false;
            try
            {
                var idFound = (String?)command.ExecuteScalar(); // Get first column of first row
                result = !String.IsNullOrEmpty(idFound);
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public String? GetElementStatus(String bubbleId, String type, String id)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();

            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT status FROM  {tableName} WHERE type=@type AND id=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@id", id);

            String ? result = null;
            try
            {
                result = (String?)command.ExecuteScalar(); // Get first column of first row
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<(String status, int count)>? GetCountByStatusFromType(String bubbleId, String type)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();
            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT status, COUNT(*) FROM {tableName} WHERE type=@type GROUP BY status;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);

            List<(String status, int count)>? result = null;
            try
            {
                using var sqlite_datareader = command.ExecuteReader();
                if (sqlite_datareader.HasRows)
                {
                    result = [];
                    String status;
                    int count;
                    while (sqlite_datareader.Read())
                    {
                        status = sqlite_datareader.GetString(0);
                        count = sqlite_datareader.GetInt32(1);
                        result.Add((status, count));
                    }
                }
                sqlite_datareader.Close();
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public String? GetElementRagDocumentationId(String bubbleId, String type, String id)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();

            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT ragDocumentId FROM  {tableName} WHERE type=@type AND id=@id;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@id", id);

            String? result = null;
            try
            {
                result = (String?)command.ExecuteScalar(); // Get first column of first row
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public String? GetOlderElementIdByStatus(String bubbleId, String type, String status)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();

            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT id FROM  {tableName} WHERE type=@type AND status=@status ORDER BY date DESC;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@status", status);

            String? result = null;
            try
            {
                result = (String ?)command.ExecuteScalar(); // Get first column of first row
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<String>? GetElementsIdByStatus(String bubbleId, String type, String status)
        {
            // Get first letter uppercase only
            type = type[..1].ToUpper();

            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT id FROM  {tableName} WHERE type=@type AND status=@status ORDER BY date DESC;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@status", status);

            List<String>? result = null;
            try
            {
                using var sqlite_datareader = command.ExecuteReader();
                if (sqlite_datareader.HasRows)
                {
                    result = [];
                    while (sqlite_datareader.Read())
                    {
                        result.Add(sqlite_datareader.GetString(0));
                    }
                }
                sqlite_datareader.Close();
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public (String? type, String? id) GetElementByRagDocumentId(String bubbleId, String ragDocumentId)
        {
            CreateTableForBubble(bubbleId);
            String tableName = "Bubble_" + bubbleId;
            String sql = $"SELECT type, id FROM  {tableName} WHERE ragDocumentId=@ragDocumentId;";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_conn);
            command.Parameters.AddWithValue("@ragDocumentId", ragDocumentId);

            String type;
            String id;
            try
            {
                using var sqlite_datareader = command.ExecuteReader();
                if (sqlite_datareader.HasRows)
                {
                    while (sqlite_datareader.Read())
                    {
                        type = sqlite_datareader.GetString(0);
                        id = sqlite_datareader.GetString(1);
                        return (type, id);
                    }
                }
                sqlite_datareader.Close();
            }
            catch (Exception ex)
            {

            }
            return (null, null);
        }

    }
}
