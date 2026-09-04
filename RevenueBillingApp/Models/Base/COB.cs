using System;

namespace RevenueBillingApp.Models.Base
{
    public class COB
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

    #region ModelClass for workflow
    public class ClientInfo
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }

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
        public int? account_owner_id { get; set; }
        public int? support_owner_id { get; set; }

        public DateTime? onboarding_start_date { get; set; }
        public DateTime? onboarding_completion_date { get; set; }
        public DateTime? billing_start_date { get; set; }
        public DateTime? contract_start_date { get; set; }
        public DateTime? contract_end_date { get; set; }

        //public string? contract_upload { get; set; }
        //public string? nda_upload { get; set; }
        //public string? proposal_upload { get; set; }

        public bool high_value_client { get; set; }
        public string priority_level { get; set; }

        public int createdby { get; set; }
        public int? userid { get; set; }

        //public IFormFile contract_upload { get; set; }
        //public IFormFile nda_upload { get; set; }
        //public IFormFile proposal_upload { get; set; }

        public string? EditID { get; set; }

    }

    public class ContractInfo
    {
        public string workflow { get; set; }

        public int? instanceid { get; set; }
        public int? contract_id { get; set; }

        // =========================
        // CONTRACT CORE
        // =========================
        public int? client_id { get; set; }
        public int? basic_contract_type { get; set; }

        public decimal? contract_value { get; set; }

        public int? currency_id { get; set; }

        public decimal? discount_percent { get; set; }

        public decimal? tax_percent { get; set; }

        // =========================
        // BILLING
        // =========================
        public int? bill_freq_id { get; set; }

        public DateTime? bill_start_date { get; set; }

        public DateTime? bill_end_date { get; set; }

        public int? billing_cycle { get; set; }

        // =========================
        // RENEWAL
        // =========================
        public string? renewal_terms_months { get; set; }

        public bool auto_renew { get; set; }

        // =========================
        // PROJECT INFO
        // =========================
        public string project_name { get; set; }

        public decimal? project_value { get; set; }

        public int? no_of_milestones { get; set; }

        // =========================
        // PAYMENT / SUPPORT
        // =========================
        public int? payment_term_id { get; set; }

        public string penalty_terms { get; set; }

        public string support_hours { get; set; }

        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }
        public int? userid { get; set; }

        public string? EditID { get; set; }
    }

    public class ClientDocuments
    {
        public IFormFile contract_upload { get; set; }
        public IFormFile nda_upload { get; set; }
        public IFormFile proposal_upload { get; set; }
    }

    #endregion

    #region ModelClass for separate Forms
    public class ClientForm
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? client_id { get; set; }

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
        public int? account_owner_id { get; set; }
        public int? support_owner_id { get; set; }

        public DateTime? onboarding_start_date { get; set; }
        public DateTime? onboarding_completion_date { get; set; }
        public DateTime? billing_start_date { get; set; }
        public DateTime? contract_start_date { get; set; }
        public DateTime? contract_end_date { get; set; }

        public string? contract_upload { get; set; }
        public string? nda_upload { get; set; }
        public string? proposal_upload { get; set; }

        public bool high_value_client { get; set; }
        public string priority_level { get; set; }

        public int createdby { get; set; }
        public int? userid { get; set; }

        //public IFormFile? contract_upload { get; set; }
        //public IFormFile? nda_upload { get; set; }
        //public IFormFile? proposal_upload { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }

    }

    public class ContractForm
    {
        public string workflow { get; set; }

        public int? instanceid { get; set; }
        public int? contract_id { get; set; }


        // =========================
        // CONTRACT CORE
        // =========================
        public int? client_id { get; set; }

        public int? basic_contract_type { get; set; }

        public decimal? contract_value { get; set; }

        public int? currency_id { get; set; }

        public decimal? discount_percent { get; set; }

        public decimal? tax_percent { get; set; }

        // =========================
        // BILLING
        // =========================
        public int? bill_freq_id { get; set; }

        public DateTime? bill_start_date { get; set; }

        public DateTime? bill_end_date { get; set; }

        public int? billing_cycle { get; set; }

        // =========================
        // RENEWAL
        // =========================
        public string? renewal_terms_months { get; set; }

        public bool auto_renew { get; set; }

        // =========================
        // PROJECT INFO
        // =========================
        public string project_name { get; set; }

        public decimal? project_value { get; set; }

        public int? no_of_milestones { get; set; }

        // =========================
        // PAYMENT / SUPPORT
        // =========================
        public int? payment_term_id { get; set; }

        public string penalty_terms { get; set; }

        public string support_hours { get; set; }

        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }
        public int? userid { get; set; }

        public string? EditID { get; set; }
    }

    public class BillingForm
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? billing_id { get; set; }

        public int? client_id { get; set; }
        public int? contract_id { get; set; }
        public int? billing_type_id { get; set; }
        public int? billing_freq_id { get; set; }
        public DateTime? next_billing_date { get; set; }
        public string? billing_method { get; set; }
        public string? delivery_method { get; set; }


        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }
        public int? userid { get; set; }

        public string? EditID { get; set; }

    }

    public class RecurringBillingForm
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? rbilling_id { get; set; }

        public int? client_id { get; set; }
        public int? contract_id { get; set; }

        public DateTime? bill_start_date { get; set; }
        public DateTime? bill_end_date { get; set; }
        public decimal? amount { get; set; }
        public decimal? tax { get; set; }
        public decimal? total_amount { get; set; }
        public DateTime? due_date { get; set; }
        public int? bill_status_id { get; set; }


        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }
        public int? userid { get; set; }

        public string? EditID { get; set; }

    }

    public class MilestoneForm
    {
        public string? workflow { get; set; }

        public int? instanceid { get; set; }
        public int? milestone_id { get; set; }

        // =========================
        // CLIENT & CONTRACT
        // =========================
        public int? client_id { get; set; }
        public int? contract_id { get; set; }

        // =========================
        // MILESTONE DETAILS
        // =========================
        public string? milestone_name { get; set; }
        public decimal? milestone_amount { get; set; }
        public DateTime? exp_completion_date { get; set; }
        public DateTime? completion_date { get; set; }
        public string? milestone_desc { get; set; }

        // =========================
        // TEAM & APPROVAL
        // =========================
        public int? internal_owner_id { get; set; }
        public int? client_approver_id { get; set; }

        // =========================
        // DELIVERABLES & DEPENDENCIES
        // =========================
        public string? deliverables_list { get; set; }
        public string? dependencies { get; set; }

        // =========================
        // DOCUMENTS
        // =========================
        public string? docUpload { get; set; }
        
        // =========================
        // CLIENT APPROVAL
        // =========================
        public string? client_approval { get; set; }
        public string? client_comments { get; set; }

        // =========================
        // QA
        // =========================
        public string? qa_comments { get; set; }

        // =========================
        // SYSTEM
        // =========================
        public int createdby { get; set; }
        public int? userid { get; set; }

        public DateTime? createdon { get; set; }
        public int? updatedby { get; set; }
        public DateTime? updatedon { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }
    }


    public class InvoiceForm
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }

        public int? invoice_id { get; set; }
        public string invoice_no { get; set; }

        public DateTime? invoice_date { get; set; }
        public DateTime? due_date { get; set; }

        public int? client_Id { get; set; }
        public int? contract_Id { get; set; }

        public int? billing_type_id { get; set; }
        public int? rbilling_Id { get; set; }
        public int? milestone_Id { get; set; }

        public string? attach1_upload { get; set; }
        public string? attach2_upload { get; set; }

        public decimal? subtotal { get; set; }
        public decimal? taxtotal { get; set; }
        public decimal? grandtotal { get; set; }

        public int createdby { get; set; }
        public DateTime? createdon { get; set; }

        public int? updatedby { get; set; }
        public DateTime? updatedon { get; set; }

        public int? userid { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }

        // ✅ Detail List
        //public List<InvoiceDetailForm> invoice_details { get; set; }
        public List<InvoiceDetailForm> invoice_details { get; set; } = new();
    }

    public class InvoiceDetailForm
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

    public class ARExceptionForm
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

    public class RenewalForm
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? renew_id { get; set; }

        public int? client_id { get; set; }
        public int? contract_id { get; set; }

        public string? renewal_term { get; set; }
        public decimal? contract_value { get; set; }
        public int? discount { get; set; }

        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }

        public string? doc_upload { get; set; }


        public int createdby { get; set; }
        public int? userid { get; set; }

        //public IFormFile? doc_upload { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }

    }
    public class PaymentForm
    {
        public string workflow { get; set; }
        public int? instanceid { get; set; }
        public int? payment_id { get; set; }

        public int? client_id { get; set; }
        public int? invoice_id { get; set; }
        public decimal? amount_received { get; set; }

        public string? payment_mode { get; set; }
        public string? chq_no { get; set; }

        public string? bank_name { get; set; }
        public DateTime? value_date { get; set; }

        public string? receipt_upload { get; set; }

        public int createdby { get; set; }
        public int? userid { get; set; }

        //public IFormFile? receipt_upload { get; set; }

        public string? EditID { get; set; }
        public string? InvitationToken { get; set; }

    }

    public class DeleteData
    {
        public string? workflow { get; set; }
        public int? instanceid { get; set; }
        public int? PrimaryID { get; set; }
        public string? EditID { get; set; }
    }

    #endregion



}
