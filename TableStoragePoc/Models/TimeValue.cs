namespace TableStoragePoc.Models;

public abstract class TimeValue
{
    public string AQTVL_GUID { get; set; } = null!;
    public string AQTE_GUID { get; set; } = null!;
    public string TE_VALUE { get; set; } = null!;
    public string TE_FROMTIME { get; set; } = null!;
    public string TE_TOTIME { get; set; } = null!;
    public int TS_ID { get; set; }
    public string TS_NAME { get; set; } = null!;
    public string QA_NAME { get; set; } = null!;
    public string QC_NAME { get; set; } = null!;
}