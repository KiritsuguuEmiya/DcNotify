using System.Linq;
using Lumina.Excel.Sheets;

namespace Dnc.Util;

public static class LuminaDataUtil
{
    public static ClassJob? GetClassJob(uint jobId)
    {
        var job = Service.DataManager.GetExcelSheet<ClassJob>()
            .FirstOrDefault(a => a.RowId == jobId);
        return job.RowId == 0 ? null : job;
    }

    public static string GetJobAbbreviation(uint jobId)
    {
        var job = GetClassJob(jobId);
        return job?.Abbreviation.ToString() ?? "???";
    }
}
