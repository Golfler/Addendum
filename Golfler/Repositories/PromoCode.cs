using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Golfler.Models;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Validation;

namespace Golfler.Repositories
{
    public class PromoCode
    {
        public GF_PromoCode promoCode { get; private set; }

        protected GolflerEntities Db;

        public string Message { get; private set; }

        #region Constructors

        public PromoCode()
        {
            Db = new GolflerEntities();
        }

        public PromoCode(long? id)
        {
            Db = new GolflerEntities();
            promoCode = Db.GF_PromoCode.FirstOrDefault(u => u.ID == id);
        }

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 01 April 2015
        /// </summary>
        /// <remarks>Get Promo Code Listing</remarks>
        public IQueryable<GF_PromoCode> GetPromoCode(string filterExpression, string sortExpression, string sortDirection, int pageIndex,
            int pageSize, ref int totalRecords)
        {
            IQueryable<GF_PromoCode> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_PromoCode.Where(x => x.PromoCode.ToLower().Contains(filterExpression.ToLower()) &&
                    x.Status != StatusType.Delete &&
                    x.CourseID == LoginInfo.CourseId).OrderByDescending(x => x.ID);
            else
                list = Db.GF_PromoCode.Where(x => x.Status != StatusType.Delete &&
                    x.CourseID == LoginInfo.CourseId).OrderByDescending(x => x.ID);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 01 April 2015
        /// </summary>
        /// <remarks>Get Menu Items Listing</remarks>
        public IEnumerable<GF_MenuItems> GetMenuItemList()
        {
            var lstMenu = Db.GF_MenuItems.Where(x => x.Status == StatusType.Active)
                .Select(x => new { x.ID, Name = x.GF_SubCategory.Name + " - " + x.Name }).ToList()
                .Select(y => new GF_MenuItems { ID = y.ID, Name = y.Name }).OrderBy(x => x.Name);

            return lstMenu;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 01 April 2015
        /// </summary>
        /// <remarks>Get Caetegory Listing</remarks>
        public IEnumerable<GF_Category> GetCategoryList()
        {
            var lstMenu = Db.GF_Category.Where(x => x.Status == StatusType.Active)
                .Select(x => new { x.ID, Name = x.Name }).ToList()
                .Select(y => new GF_Category { ID = y.ID, Name = y.Name }).OrderBy(x => x.Name);

            return lstMenu;
        }

        public GF_PromoCode GetPromoCodeByID(long? id)
        {
            if (Convert.ToInt64(id) > 0)
                promoCode = Db.GF_PromoCode.FirstOrDefault(x => x.ID == id);

            return promoCode ?? new GF_PromoCode();
        }

        public bool Save(GF_PromoCode obj)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();

            objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);

            try
            {
                #region check access

                if (obj.ID > 0)
                {
                    if (!objModuleRole.UpdateFlag)
                    {
                        Message = "update unaccess";
                        return false;
                    }
                }
                else
                {
                    if (!objModuleRole.AddFlag)
                    {
                        Message = "add unaccess";
                        return false;
                    }
                }

                #endregion

                if (obj.ID > 0)
                {
                    #region Update Promo Code

                    if (promoCode != null)
                    {
                        promoCode.CourseID = LoginInfo.CourseId;
                        promoCode.PromoCode = obj.PromoCode;
                        promoCode.DiscountType = obj.DiscountType;
                        promoCode.Discount = obj.Discount;
                        promoCode.ExpiryDate = obj.ExpiryDate;
                        promoCode.ReferenceID = obj.ReferenceID;
                        promoCode.ReferenceType = obj.ReferenceType;
                        promoCode.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                        promoCode.IsActive = obj.Active;
                        promoCode.IsOneTimeUse = obj.OneTimeUse;
                        if (obj.OneTimeUse)
                            promoCode.IsUsed = false;
                        promoCode.ModifyBy = LoginInfo.UserId.ToString();
                        promoCode.ModifyDate = DateTime.Now;

                        if (obj.DiscountType == DiscountType.Amount)
                        {
                            promoCode.ReferenceID = 0;
                            promoCode.ReferenceType = DiscountType.Amount;
                            Db.Configuration.ValidateOnSaveEnabled = false;
                        }
                    }

                    Message = "update";
                    Db.SaveChanges();

                    #endregion update
                }
                else
                {
                    #region Duplicate Check

                    var lstDuplicate = Db.GF_PromoCode.FirstOrDefault(x => x.PromoCode.ToLower() == obj.PromoCode.ToLower() &&
                    x.Status != StatusType.Delete && x.CourseID == LoginInfo.CourseId);

                    if (lstDuplicate != null)
                    {
                        Message = "Promo code already exists.";
                        return false;
                    }

                    #endregion

                    #region new Promo Code

                    obj.CourseID = LoginInfo.CourseId;
                    obj.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                    obj.IsActive = obj.Active;
                    obj.IsOneTimeUse = obj.OneTimeUse;
                    obj.CreatedBy = LoginInfo.UserId.ToString();
                    obj.CreatedDate = DateTime.Now;
                    Db.GF_PromoCode.Add(obj);

                    if (obj.DiscountType == DiscountType.Amount)
                    {
                        obj.ReferenceID = 0;
                        obj.ReferenceType = DiscountType.Amount;
                        Db.Configuration.ValidateOnSaveEnabled = false;
                    }

                    Db.SaveChanges();

                    Message = "add";

                    #endregion
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }

        internal bool ChangeStatus(bool status)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();

            objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);

            if (!objModuleRole.UpdateFlag)
            {
                Message = "unaccess";
                return false;
            }

            if (promoCode != null)
            {
                if (promoCode.ID != LoginInfo.UserId)
                {
                    promoCode.Active = !status;
                    promoCode.IsActive = !status;
                    promoCode.ModifyBy = LoginInfo.UserId.ToString();
                    promoCode.ModifyDate = DateTime.Now;
                }
                Db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeletePromoCodes(List<long> ids)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();

            objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);

            if (!objModuleRole.DeleteFlag)
            {
                Message = "";// Resources.Resources.unaccess;
                return false;
            }

            var users = Db.GF_PromoCode.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var u in users)
            {
                u.Status = StatusType.Delete;
                u.IsActive = false;
                u.ModifyBy = LoginInfo.UserId.ToString();
                u.ModifyDate = DateTime.Now;
            }

            Db.Configuration.ValidateOnSaveEnabled = false;
            Db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 21 July 2015
        /// Discription: Issue promo code to golfer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IssuePromoCode(GF_PromoCode obj)
        {
            try
            {
                #region Duplicate Check

                var lstDuplicate = Db.GF_PromoCode.FirstOrDefault(x => x.PromoCode.ToLower() == obj.PromoCode.ToLower() &&
                x.Status != StatusType.Delete && x.CourseID == LoginInfo.CourseId);

                if (lstDuplicate != null)
                {
                    Message = "Promo code already exists.";
                    return false;
                }

                #endregion

                obj.CourseID = LoginInfo.CourseId;
                obj.ReferenceID = 0;
                obj.ReferenceType = PromoCodeType.AmountWise;
                obj.Status = StatusType.Active;
                obj.IsActive = obj.Active;
                obj.IsOneTimeUse = true;
                obj.DiscountType = DiscountType.Amount;
                obj.CreatedBy = LoginInfo.UserId.ToString();
                obj.CreatedDate = DateTime.Now;
                
                Db.GF_PromoCode.Add(obj);
                Db.Configuration.ValidateOnSaveEnabled = false;
                Db.SaveChanges();

                Message = "add";

                #region Send Mail

                Int64 FromUserid = LoginInfo.UserId;
                string toEmail = "";
                string toUserName = "";
                string mailresult = "";

                var order = Db.GF_Order.Where(x => x.ID == obj.OrderID).FirstOrDefault();
                if (order != null)
                {
                    toEmail = Convert.ToString(order.GF_Golfer.Email);
                    toUserName = Convert.ToString(order.GF_Golfer.FirstName) + " " + Convert.ToString(order.GF_Golfer.LastName);

                    #region send mail
                    try
                    {
                        IQueryable<GF_EmailTemplatesFields> templateFields = null;
                        var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.PromoCode, ref templateFields, LoginInfo.CourseId, LoginType.CourseAdmin, ref mailresult);

                        if (mailresult == "") // means Parameters are OK
                        {
                            if (ApplicationEmails.PromoCodeIssue(ref Db, LoginInfo.CourseId, obj.OrderID.ToString(), obj.PromoCode,
                                toEmail, toUserName, param, ref templateFields, ref mailresult))
                            {
                                // Do steps for Mail Send successful
                                mailresult = "Promo code successfully sent.";
                            }
                            else
                            {
                                // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                            }
                        }
                        else
                        {
                            // Do steps for Parameters not available.Reason can be find on "mailresult"
                        }
                    }
                    catch (Exception ex)
                    {
                        mailresult = ex.Message;
                    }
                    #endregion
                }

                #endregion

                return true;
            }
            catch (DbEntityValidationException ex)
            {
                Message = "Error occured. Please try again.";
                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }
    }
}