using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections.Generic;

public static class DBQueries {

    private static readonly string TABLE_NAME = "metainformation";
    private static readonly string ID_COLUMN = "id";
    private static readonly string DB_PATH = Application.persistentDataPath + "/Metainformation/";
    private static string[] colNames;

    public static void ExtractMetaInformation(Tree tree, string dbName) {
        // Open connection
        string connection = "URI=file:" + DB_PATH + dbName;

        IDbConnection dbcon = new SqliteConnection(connection);

        try
        {
            dbcon.Open();
            //get column name information
            string colNameQuery = "PRAGMA table_info(" + TABLE_NAME + ")";
            IDbCommand cmnd_read = dbcon.CreateCommand();
            IDataReader reader;
            cmnd_read.CommandText = colNameQuery;
            reader = cmnd_read.ExecuteReader();
            List<string> colNameList = new List<string>();
            while (reader.Read()) { 
                colNameList.Add(reader[1].ToString());
            }
            //get node metainformation
            colNames = colNameList.ToArray();
            RecursiveReadMetaInformation(tree.getRoot(), dbcon);
            dbcon.Close();
        }
        catch (SqliteException e)
        {
            GameManager.DebugLog("Database error, no metainformation will be loaded");
        }

    }

    private static void RecursiveReadMetaInformation(Node curr, IDbConnection connection) {
        if (curr.getLabel() != "") // only lookup labelled nodes in the database
        {   
            IDbCommand cmnd_read = connection.CreateCommand();
            IDataReader reader;
            string query = "SELECT * FROM " + TABLE_NAME + " WHERE " + ID_COLUMN + " = '" + curr.getLabel() + "'";
            cmnd_read.CommandText = query;
            reader = cmnd_read.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 1; i < reader.FieldCount; i++)
                {
                    if (reader[i].GetType() != typeof(byte[]))
                    {
                        curr.SetMetaData(curr.GetMetaData() + "\n" +
                            colNames[i] + ": " + reader[i].ToString());
                    }
                }
            }
        }

        foreach (Node child in curr.getChildren())
            RecursiveReadMetaInformation(child, connection);
    }
}
