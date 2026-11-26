namespace FIWARE.SmartDataModels.IssueTracking;

// 20221109174242
// https://smart-data-models.github.io/dataModel.IssueTracking/IssueReporting/examples/example.json
public class IssueReporting
{
  public string Id { get; set; }
  public string Type { get; set; }
  public string Comments { get; set; }
  public string WardId { get; set; }
  public string DepartmentId { get; set; }
  public string Title { get; set; }
  public string SubCategoryCode { get; set; }
  public string TitleCode { get; set; }
  public string SubCategory { get; set; }
  public string ReportId { get; set; }
  public string CategoryCode { get; set; }
  public string Category { get; set; }
  public string Landmark { get; set; }
  public DateTime ObservationDateTime { get; set; }
  public string MediaURL { get; set; }
  public string ResolutionStatus { get; set; }
}