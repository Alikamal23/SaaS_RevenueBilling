namespace RevenueBillingApp.Models.Base
{
    public class KeyFactors
    {
        public int? FactorId { get; set; }
        public string? FactorName { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
    }

    public class KPISurveyForm
    {
        public int? QuestionId { get; set; }
        public string? Question { get; set; }
        public int? FactorId { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public int? UserID { get; set; }
    }

    public class KPIForm
    {
        public string? form_id { get; set; }
        public string? question_id { get; set; }
        public string? dealer_type { get; set; }
        public string? question_type { get; set; }
        public string? question { get; set; }
        public string? answertypes { get; set; }
        public bool remarks { get; set; }
        public string? guideline { get; set; }
        public string? attachments { get; set; }
        public string? c_yes { get; set; }
        public string? c_fed { get; set; }
        public string? correctAnswer_yes { get; set; }
        public string? correctAnswer_feed { get; set; }
        public bool status { get; set; }
        public int? UserID { get; set; }

    }

    public class OSForm
    {
        public string? form_id { get; set; }
        public string? board_id { get; set; }
        public string? dealer_type { get; set; }
        public string? category_type { get; set; }
        public string? boarddescription { get; set; }
        public string? answertypes { get; set; }
        public bool remarks { get; set; }
        public bool status { get; set; }
        public string? guideline { get; set; }
        public string? attachments { get; set; }
        public string? c_yes { get; set; }
        public string? c_fed { get; set; }
        public string? correctAnswer_yes { get; set; }
        public string? correctAnswer_feed { get; set; }
        public string? filepaths { get; set; }
        public int? UserID { get; set; }

    }

    public class CWMForm
    {
        public string? category_type { get; set; }
        public string? FilePath { get; set; }
        public string? form_id { get; set; }
        public string? model_id { get; set; }
        public string? companystandard { get; set; }
        public string? standard { get; set; }
        public string? answertypes { get; set; }
        public bool remarks { get; set; }
        public string? guideline { get; set; }
        public string? attachments { get; set; }
        public string? c_yes { get; set; }
        public string? c_fed { get; set; }
        public string? correctAnswer_yes { get; set; }
        public string? correctAnswer_feed { get; set; }
        public bool status { get; set; }
        public int? UserID { get; set; }

    }

    public class DFQForm
    {
        public string? form_id { get; set; }
        public string? questionid { get; set; }
        public string? Question { get; set; }
        public string? answertypes { get; set; }
        public bool remarks { get; set; }
        public string? guideline { get; set; }
        public string? attachments { get; set; }
        public string? c_yes { get; set; }
        public string? c_fed { get; set; }
        public string? correctAnswer_yes { get; set; }
        public string? correctAnswer_feed { get; set; }
        public bool status { get; set; }
        public int? UserID { get; set; }
    }

    public class CustomerDocument
    {
        public List<IFormFile>? files { get; set; }
        public int? UserID { get; set; }
    }

    public class DownloadParam
    {

        //From To Date ...
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ShowExcel { get; set; }
        public bool ShowLogs { get; set; }
        public int Monthd { get; set; }
        public int Year { get; set; }
        public int Zone { get; set; }
        public string type { get; set; }

    }

    public class UploadParam
    {

        //From To Date ...
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ShowExcel { get; set; }
        public bool ShowLogs { get; set; }
        public int Monthd { get; set; }
        public int Year { get; set; }
        public int Zone { get; set; }
        public string type { get; set; }

        //insert xls to db sp parameter ...
        // Add this
        public List<UploadDoctorModel> Doctors { get; set; }

    }

    public class UploadDoctorModel
    {
        public int DocSPO { get; set; }
        public string Territory { get; set; }
        public string DocCode { get; set; }
        public string DocName { get; set; }
        public string ContactPersonName { get; set; }
        public string Speciality { get; set; }
        public string ClassCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string MobileNo { get; set; }
        public string KOL { get; set; }  // Should be string "TRUE"/"FALSE" as per SP
        public string Status { get; set; }
        public string Flag { get; set; } // C, N, U, D
        public string Potential { get; set; }
        public string Propensity { get; set; }
        public string Brand { get; set; }
        public string Market_Name { get; set; }
        public string Month { get; set; } // string "YYYY-MM"
    }

    public class DealerAssign
    {
        public string? userids { get; set; }
        public string? dealerid { get; set; }
        public string? monthofdealer { get; set; }
        public int? UserID { get; set; }
    }

    public class DealerUploader
    {
        public IFormFile? files { get; set; }
        public string? monthyear { get; set; }
    }

}
