namespace RevenueBillingApi.Models.BillingRevenue
{
    public class BillingRevenue
    {
        public string workflow { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public int EstimatedAmount { get; set; }
        public DateTime RequiredBy { get; set; }
        public string Priority { get; set; }
        public int? CreatedBy { get; set; }
        public int? userid { get; set; }
        public int? instanceid { get; set; }
    }

    #region ModelClass
    public class BR_ClientInfo
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? id { get; set; }
        public string? client_refno { get; set; }
        public string client_name { get; set; }
        public int? industry_id { get; set; }
        public int? company_size_id { get; set; }
        public string website { get; set; }
        public string billing_address { get; set; }
        public int? country_id { get; set; }
        public string tax_registration_no { get; set; }

        public string primary_contact_name { get; set; }
        public string primary_contact_email { get; set; }
        public string primary_contact_phone { get; set; }

        public int contract_type_id { get; set; }
        public string? account_owner { get; set; }
        public int? support_owner_id { get; set; }

        public DateTime? onboarding_start_date { get; set; }
        public DateTime? onboarding_completion_date { get; set; }
        public DateTime? billing_start_date { get; set; }
        public DateTime? contract_start_date { get; set; }
        public DateTime? contract_end_date { get; set; }


        public bool high_value_client { get; set; }
        public string priority_level { get; set; }

        //isdeleted= new Field
        public bool isdeleted { get; set; }

        public int createdby { get; set; }
        public int? userid { get; set; }

        // Files mapping properties
        public string? nda_filename { get; set; }
        public string? tax_verify_filename { get; set; }

        public IFormFile? nda_upload { get; set; }
        public IFormFile? tax_verify { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }
        //Action (Add/Edit)
        public string? Action { get; set; }

    }

    public class BR_ContractInfo
    {
        public string workflow { get; set; }

        public int? instanceid { get; set; }

        public int? id { get; set; }

        public string? contract_refno { get; set; }

        public int? client_id { get; set; }

        public int? basic_contract_type { get; set; }

        public decimal? contract_value { get; set; }

        public int? currency_id { get; set; }

        public decimal? discount_percent { get; set; }

        public decimal? tax_percent { get; set; }

        public decimal? total_saas_amount { get; set; }

        // =========================
        // BILLING
        // =========================
        public int? bill_freq_id { get; set; }

        public DateTime? bill_start_date { get; set; }

        public DateTime? bill_end_date { get; set; }

        public int? billing_cycle { get; set; }

        public int? bill_type_id { get; set; }

        public bool auto_renew { get; set; }

        public string? doc1_filename { get; set; }

        public string? doc2_filename { get; set; }

        public string? doc3_filename { get; set; }

        public string? doc4_filename { get; set; }


        public IFormFile? doc1_upload { get; set; }
        public IFormFile? doc2_upload { get; set; }
        public IFormFile? doc3_upload { get; set; }
        public IFormFile? doc4_upload { get; set; }


        // =========================
        // PROJECT INFO
        // =========================
        public string? project_name { get; set; }

        public decimal? project_value { get; set; }
        public decimal? m_discount { get; set; }
        public decimal? m_tax_percent { get; set; }
        public decimal? total_milestone_amount { get; set; }


        public int? no_of_milestones { get; set; }

        public int? payment_term_id { get; set; }

        public string? penalty_terms { get; set; }

        public string? support_hours { get; set; }

        //isdeleted= new Field
        public bool isdeleted { get; set; }

        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }

        public int? userid { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }
        //Action (Add/Edit)
        public string? Action { get; set; }

        // =========================
        // DETAIL TABLE
        // =========================
        public List<BR_ContractMilestone>? Milestones { get; set; }

    }

    public class BR_ContractMilestone
    {
        public int milestone_id { get; set; }
        public int contract_id { get; set; }
        public int milestone_no { get; set; }
        public string milestone_name { get; set; }
        public decimal milestone_amount { get; set; }
        public string due_date { get; set; }
        public string remarks { get; set; }
        public string status { get; set; }

        public int? createdby { get; set; }

        public DateTime? createdon { get; set; }

        public int? updatedby { get; set; }

        public DateTime? updatedon { get; set; }

    }

    public class BR_InvoiceInfo
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }

        public int invoice_id { get; set; }
        public string invoice_no { get; set; }

        public int client_id { get; set; }
        public int contract_id { get; set; }

        public int contract_type { get; set; }
        public int? milestone_no { get; set; }

        public DateTime? billing_period { get; set; }

        public DateTime invoice_date { get; set; }
        public DateTime due_date { get; set; }

        public decimal invoice_amount { get; set; }
        public decimal tax_amount { get; set; }
        public decimal total_amount { get; set; }
        public decimal? gross_amount { get; set; }
        public decimal? discount_percent { get; set; }
        public decimal? tax_percent { get; set; }

        public byte invoice_status { get; set; }
        public bool payment_status { get; set; }

        public string invoice_type { get; set; }
        public string? remarks { get; set; }

        public string? attach1_filename { get; set; }
        public string? attach2_filename { get; set; }

        public IFormFile? attach1_upload { get; set; }
        public IFormFile? attach2_upload { get; set; }

        public bool isdeleted { get; set; }

        public string? PONo { get; set; }

        public int createdby { get; set; }
        public DateTime? createdon { get; set; }

        public int? updatedby { get; set; }
        public DateTime? updatedon { get; set; }

        public int? userid { get; set; }

        public string? Action { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }


        //// ✅ Detail List
        ////public List<InvoiceDetailForm> invoice_details { get; set; }
        //public List<BR_InvoiceDetail> invoice_details { get; set; } = new();

    }

    public class BR_InvoiceDetail
    {
        public int? invoice_detailId { get; set; }
        public int? invoice_Id { get; set; }

        public string item_desc { get; set; }

        public decimal? quantity { get; set; }
        public decimal? unit_price { get; set; }

        public decimal? amount { get; set; }

        public decimal? tax_percent { get; set; }
        public decimal? tax_amount { get; set; }

        public decimal? total_amount { get; set; }
    }

    public class BR_PaymentInfo
    {
        public string? workflow { get; set; }

        public int? instanceid { get; set; }

        // Primary Key
        public int? payment_id { get; set; }

        // Header Table
        public int? client_id { get; set; }

        public decimal? amount_received { get; set; }

        public string? payment_mode { get; set; }

        public string? ref_no { get; set; }

        public string? bank_name { get; set; }

        public DateTime? value_date { get; set; }

        // Receipt Upload
        public string? receipt_upload { get; set; }

        public IFormFile? receipt_file { get; set; }

        // Audit Fields
        public int? createdby { get; set; }

        public DateTime? createdon { get; set; }

        public int? updatedby { get; set; }

        public DateTime? updatedon { get; set; }

        // Extra Fields
        public int? userid { get; set; }

        public bool isdeleted { get; set; }

        public string? EditID { get; set; }

        public string? InvitationToken { get; set; }

        public string? Action { get; set; }

        // Detail Table
        public List<BR_PaymentDetail>? PaymentDetails { get; set; }
    }
    public class BR_PaymentDetail
    {
        public int payment_detail_id { get; set; }
        public int payment_id { get; set; }
        public int invoice_id { get; set; }
        public string? invoice_no { get; set; }
        public DateTime? invoice_date { get; set; }
        public decimal invoice_amount { get; set; }
        public string? remarks { get; set; }
        public int? createdby { get; set; }
        public DateTime? createdon { get; set; }

    }



    public class BR_ARExceptionInfo
    {
        public string workflow { get; set; }

        public int? instanceid { get; set; }
        public int? exception_id { get; set; }

        public int? client_id { get; set; }
        public int? invoice_id { get; set; }

        public decimal? amount { get; set; }
        public int? days_overdue { get; set; }

        public string? risk_category { get; set; }
        public string? escalation_level { get; set; }
        public string? notes { get; set; }


        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }
        public int? userid { get; set; }

        public string? EditID { get; set; }

    }
    public class BR_RenewalInfo
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? renew_id { get; set; }
        public int? client_id { get; set; }
        public int? contract_id { get; set; }
        public string? renewal_term { get; set; }
        public DateTime? old_end_date { get; set; }
        public decimal? contract_value { get; set; }
        public decimal? discount_percent { get; set; }
        public decimal? tax_percent { get; set; }
        public decimal? final_amount { get; set; }
        public int? currency_id { get; set; }
        public int? bill_freq_id { get; set; }
        public int? billing_cycle { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? renewal_status { get; set; }
        public bool? auto_renew { get; set; }
        public string? doc_upload { get; set; }
        public int createdby { get; set; }
        public int? userid { get; set; }
        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }

    }

    #endregion

    public class BR_DeleteData
    {
        public string? workflow { get; set; }
        public int? instanceid { get; set; }
        public int? PrimaryID { get; set; }
        public string? EditID { get; set; }
    }


    public class EmailInvoiceRequest
    {
        public int InvoiceId { get; set; }
        public string FileName { get; set; }
        public string FileBase64 { get; set; }
    }

    public class EmailTemplateRequest
    {
        public string TestEmail { get; set; }
        public int TemplateId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    // ReportParam
    public class ReportParam
    {
        public string? ClientId { get; set; }
        public string? ClientName { get; set; }
        //industry_id
        public string? industry_id { get; set; }
        //industry_name
        public string? industry_name { get; set; }
        public string? Category { get; set; }
        public string? ProductId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? FromExpiryDate { get; set; }
        public DateTime? ToExpiryDate { get; set; }
        public bool ShowExcel { get; set; }
        public bool ShowLogs { get; set; }

        public bool SendEmail { get; set; }

        public string? StatusId { get; set; }
        public int? DonorId { get; set; }
        //From To Date ...
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? WithZero { get; set; }

        public string? OrderBy { get; set; }

        //Common Fields ...
        public int FK_InstanceID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? EditID { get; set; }

    }

    public class ConvertRenewalRequest
    {
        public int renew_id { get; set; }
        public int createdby { get; set; }
    }


}
