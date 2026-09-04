using Microsoft.AspNetCore.Http.HttpResults;
using System;

namespace RevenueBillingApp.Models.Base
{
    public class MasterSetup
    {

    }

    #region Master Form Model
    public class WarehouseMaster
    {
        public int? WarehouseId { get; set; }
        public string? WarehouseDesc { get; set; }
        public string? Address { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
    }

    public class ManufacturerMaster
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
    }

    public class Party
    {
        public int? PartyId { get; set; }
        public int? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? Contact { get; set; }
        public string? Address { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Cell { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? PartyGroup { get; set; }
        public string? Remarks { get; set; }
        public float Commission { get; set; }
        public float VanSharing { get; set; }
        public int? CategoryCode { get; set; }
        public string? GLCode { get; set; }
        public string? NTNNo { get; set; }
        public string? GSTNo { get; set; }
        public float GST { get; set; }
        public float SST { get; set; }
        public float WHT { get; set; }
        public float OpeningBalance { get; set; }
        public DateTime? OpeningDate { get; set; }
        public string? PaymentTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? BranchId { get; set; }

    }

    public class ProductMaster
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public float? CP { get; set; }
        public float? SP { get; set; }
        public int? ReOrderQty { get; set; }
        public float Packing { get; set; }
        public string? ProductType { get; set; }
        public int? PartyId { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? BranchId { get; set; }

    }

    public class CurrencyMaster
    {
        public int? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencyDesc { get; set; }
        public string? CurrencyAbbr { get; set; }
        public float? CurrencyRate { get; set; }
        public bool? DefaultCurrency { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }
    }

    public class ConversionRate
    {
        public int? id { get; set; }
        public int? currency_id { get; set; }
        public DateTime? conversion_date { get; set; }
        public float? conversion_rate { get; set; }
        public int? instanceid { get; set; }
        public bool isdeleted { get; set; }
        public string? createdby { get; set; }
        public DateTime? createdon { get; set; }
        public string? updatedby { get; set; }
        public DateTime? updatedon { get; set; }

        public bool? IsActive { get; set; }
        public string? EditID { get; set; }

    }


    public class JobOrder
    {
        public int? JobOrderId { get; set; }
        public string? JobNo { get; set; }
        public string? JobDesc { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

    }

    #endregion

    #region Account Model

    #region ChartofAccount Master Tbl
    public class tbl_Account_Level1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Nature { get; set; }
    }

    public class tbl_Account_Level2
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? GLCode { get; set; }
        public string? FC { get; set; }
        public string? FC_Nature { get; set; }
        public int FK_InstanceID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool? IsActive { get; set; }
        public string? Action { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

    }

    public class tbl_Account_Level3
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GLCode { get; set; }
        public int? FK_AccountId { get; set; }
        public int FK_InstanceID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool? IsActive { get; set; }
        public string? Action { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

    }

    public class tbl_Account_Level4
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string?  GLCode { get; set; }
        public string? CB { get; set; }
        public int? FK_CategoryId { get; set; }
        public int FK_InstanceID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool? IsActive { get; set; }
        public string Action { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

    }
    #endregion

    #region Voucher Tbls (Master & Detail)
    public class tbl_Voucher
    {
        public int? VoucherId { get; set; }
        public string? VType { get; set; }
        public int? VCode { get; set; }
        public DateTime? VDate { get; set; }
        public string? VNo { get; set; }
        public string? Folio { get; set; }
        public string? ModeOfPayment { get; set; }
        public string? BankName { get; set; }
        public string? Chq_PO_Draft_No { get; set; }
        public string? CashType { get; set; }
        public string? Narration { get; set; }
        public int? DCCategory { get; set; }
        public int? DCCode { get; set; }
        public decimal? Amount { get; set; }


        // Tax Fields WHT / ST
        public bool? ITTax { get; set; }
        public int? ITTaxCode { get; set; }
        public decimal? ITTaxAmount { get; set; }
        public decimal? ITTaxPercent { get; set; }

        public bool? STTax { get; set; }
        public int? STTaxCode { get; set; }
        public decimal? STTaxAmount { get; set; }
        public decimal? STTaxPercent { get; set; }

        public List<tbl_Voucher_Detail>? DetailSection { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

        //public List<Account_Balance>? AccountBalance { get; set; }
        //public List<Party_Balance>? PartyBalance { get; set; }
    }

    public class tbl_Voucher_Detail
    {
        public int? DetailId { get; set; }
        public int? FK_VoucherID { get; set; }
        public string? VType { get; set; }
        public int? VCode { get; set; }
        public string? VNo { get; set; }
        public int CategoryCode { get; set; }
        public int HeadCode { get; set; }
        public string? Remarks { get; set; }
        public string? HeadType { get; set; }
        public string? JobNo { get; set; }
        public string? BillNo { get; set; }
        public string? ChqNo { get; set; }
        public DateTime? ChqDate { get; set; }
        public DateTime? ClearDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }

        public List<Account_Balance>? AccountBalance { get; set; }
        public List<Party_Balance>? PartyBalance { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

    }
    #endregion

    #region Balance Tbls (Head & Party [Vendor, Donor])
    public class Account_Balance
    {
        public int RowID { get; set; }
        public int CategoryId { get; set; }
        public int HeadId { get; set; }
        public DateTime TransDate { get; set; }
        public string TransMode { get; set; }
        public int TransCode { get; set; }
        public int VNo { get; set; }
        public decimal DEBIT { get; set; }
        public decimal CREDIT { get; set; }
    }

    public class Party_Balance
    {
        public int RowID { get; set; }
        public int CategoryId { get; set; }
        public int PartyId { get; set; }      //Vendor or Donor
        public string PartyType { get; set; }
        public DateTime TransDate { get; set; }
        public string TransMode { get; set; }
        public int TransCode { get; set; }
        public int VNo { get; set; }
        public decimal DEBIT { get; set; }
        public decimal CREDIT { get; set; }
    }

    #endregion

    #endregion

    #region Payable Model

    #region PurchaseOrder Tbls (Master & Detail)
    public class Purchase_Order
    {
        public int? Id { get; set; }
        public int? POId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? RefNo { get; set; }
        public int? PartyId { get; set; }
        public int? WarehouseId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Remarks { get; set; }
        public string? Terms_Cond { get; set; }
        public string? QuoteNo { get; set; }
        public DateTime? QuoteDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool? Cancel { get; set; }
        public bool? Clear { get; set; }
        public int? BranchId { get; set; }
        public int? InstanceId { get; set; }

        //Detail Table
        public List<Purchase_Order_Detail>? DetailSection { get; set; }

        //Common Fields...
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? EditID { get; set; }

        //public List<Account_Balance>? AccountBalance { get; set; }
        //public List<Party_Balance>? PartyBalance { get; set; }
    }

    public class Purchase_Order_Detail
    {
        public int? Id { get; set; }
        public int? POId { get; set; }
        public int? ProductId { get; set; }
        public int? OQty { get; set; }
        public decimal? Rate { get; set; }
        public int? RQty { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? JobNo { get; set; }

        //public List<Account_Balance>? AccountBalance { get; set; }
        //public List<Party_Balance>? PartyBalance { get; set; }

        // common fields ...
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? EditID { get; set; }
        public int? BranchId { get; set; }

    }

    #endregion


    //GRN


    #endregion


    // Revenue & Billing Suite

    #region Revenue&Billing_Suite

    public class Client
    {
        public int? PartyId { get; set; }
        public int? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? Contact { get; set; }
        public string? Address { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Cell { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? PartyGroup { get; set; }
        public string? Remarks { get; set; }
        public float Commission { get; set; }
        public float VanSharing { get; set; }
        public int? CategoryCode { get; set; }
        public string? GLCode { get; set; }
        public string? NTNNo { get; set; }
        public string? GSTNo { get; set; }
        public float GST { get; set; }
        public float SST { get; set; }
        public float WHT { get; set; }
        public float OpeningBalance { get; set; }
        public DateTime? OpeningDate { get; set; }
        public string? PaymentTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? EditID { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? BranchId { get; set; }

    }


    public class TestClientOnboarding
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? EditID { get; set; }

    }


    #endregion


}
