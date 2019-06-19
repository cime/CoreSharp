using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CoreSharp.Breeze {

  public class NumericKeyGenerator : IKeyGenerator {

    public NumericKeyGenerator(DbConnection dbConnection) {
      _connection = dbConnection;
    }

    private DbConnection _connection;

    public void UpdateKeys(List<TempKeyInfo> keys) {

      var nextId = GetNextId(keys.Count);
      keys.ForEach(ki => {
        try {
          var nextIdValue = Convert.ChangeType(nextId, ki.Property.PropertyType);
          ki.RealValue = nextIdValue;
        } catch {
          throw new NotSupportedException("This id generator cannot generate ids of type " + ki.Property.PropertyType);
        }
        nextId++;
      });
    }


    //***********************************
    // Private & Protected
    //***********************************

    private long GetNextId(int pCount) {
      // Serialize access to GetNextId
      lock (Lock) {

        if (_nextId + pCount > _maxNextId) {
          AllocateMoreIds(pCount);
        }
        var result = _nextId;
        _nextId += pCount;
        return result;
      }
    }

    private void AllocateMoreIds(int pCount) {
      const string sqlSelect = "select NextId from NextId where Name = 'GLOBAL'";
      const string sqlUpdate = "update NextId set NextId={0} where Name = 'GLOBAL' and NextId={1}";

      // allocate the larger of the amount requested or the default alloc group size
      pCount = Math.Max(pCount, DefaultGroupSize);

      var wasClosed = _connection.State != ConnectionState.Open;
      try {
        if (wasClosed) {
          _connection.Open();
        }
        IDbCommand aCommand = CreateDbCommand(_connection);

        for (var tries = 0; tries <= MaxTries; tries++) {

          aCommand.CommandText = sqlSelect;
          var aDataReader = aCommand.ExecuteReader();
          if (!aDataReader.Read()) {
            throw new Exception("Unable to locate 'NextId' record");
          }

          var tmp = aDataReader.GetValue(0);
          var nextId = (long) Convert.ChangeType(tmp, typeof (long));
          var newNextId = nextId + pCount;
          aDataReader.Close();

          // do the update;
          aCommand.CommandText = string.Format(sqlUpdate, newNextId, nextId);

          // if only one record was affected - we're ok; otherwise try again.
          if (aCommand.ExecuteNonQuery() == 1) {
            _nextId = nextId;
            _maxNextId = newNextId;
            return;
          }
        }
      } finally {
          if (wasClosed) {
            _connection.Close();
          }
      }
      throw new Exception("Unable to generate a new id");
    }

    private DbCommand CreateDbCommand(IDbConnection connection) {
      var command = connection.CreateCommand();
      try {
        return (DbCommand) command;
      } catch {
        throw new Exception("Unable to cast a DbCommand object from an IDbCommand for current connection");
      }
    }

    private static long _nextId;
    private static long _maxNextId;

    private static readonly object Lock = new object();
    private const int MaxTries = 3;
    private const int DefaultGroupSize = 100;
    private const int MaxGroupSize = 1000;
  }
}
