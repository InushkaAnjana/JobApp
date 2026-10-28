using JobAppHR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Repository
{
    public interface IDBOperations
    {
        DataTable SelectRows(string tableName, string fieldSet, string keyField, string keyValue, string whereClause, string keyFieldDataType = "");
        DataTable SelectRows(string sql);
        List<SelectListItem> AnyDataList(string tableName, string valueField, string textField, string whereClause, string sortOrder);
        string UpdateRecords(string tableName, DataTable tempTable, string keyField, string keyValue, string keyFieldDataType = "", string whereCondition = "");
        string UpdateRecords(string tableName, DataTable tempTable, string keyField);
        string UpdateRecords(string sql);
        string InsertRecords(string tableName, DataTable tempTable, bool isIdentity, string identityField = "");
        string GetJobPositionCodeById(int jobPositionID);
        string GetJobPositionName(string intakeCode = "", string jobPositionCode = "");
        DataTable GetFilteringCriteriaOfJobPosition(string intakeCode);
        bool IsTalentPoolEnabled();
        void UpdateTalentPoolStatus(bool isEnabled);
    }
}
