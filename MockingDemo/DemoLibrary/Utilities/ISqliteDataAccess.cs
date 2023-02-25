using System.Collections.Generic;

namespace DemoLibrary.Utilities
{
    public interface ISqliteDataAccess
    {
        List<T> LoadData<T>(string sql);
        List<T> LoadDataLongNames<T>();
        void SaveData<T>(T person, string sql);
        void UpdateData<T>(T person, string sql);
    }
}