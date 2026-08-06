using System.Linq;
using Lumina.Excel.Sheets;

namespace Dnc.Util;

public static class LuminaDataUtil
{
    public static string GetJobAbbreviation(uint jobId)
    {
        var job = Service.DataManager.GetExcelSheet<ClassJob>()
            .Where(a => a.RowId == jobId)
            .FirstOrDefault();
        return job.RowId == 0 ? "???" : job.Abbreviation.ToString();
    }
}
