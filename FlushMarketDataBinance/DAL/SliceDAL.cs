using FlushMarketDataBinance.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace FlushMarketDataBinance.DAL
{
    public class SliceDAL
    {
        private string _connectionString;
        public SliceDAL(IConfiguration iconfiguration)
        {
            _connectionString = iconfiguration.GetConnectionString("Default");
        }

        public void AddSlice(List<Slice> slices)
        {
            var sqlExpressionInsert = "INSERT INTO Slices (UpdateTime, Bids, Asks) VALUES ";

            foreach (var slice in slices)
                sqlExpressionInsert += $"({slice.LastUpdate}, {slice.Bids}, {slice.Asks}),";

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand command = new SqlCommand(sqlExpressionInsert, con);
                    
                    SqlDataReader rdr = command.ExecuteReader();
                    int number = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
