using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public partial class GF_CourseMenu
    {
        GolflerEntities _db = null;

        //public GF_CourseMenu()
        //{
        //    _db = new GolflerEntities();
        //}

        #region Get menu Item by Course ID
        /// <summary>
        /// Create By:Arun
        /// Created By:24 March 2015
        /// Purpose: Get menu items by CourseID
        /// </summary>
        /// <param name="objMenu"></param>
        /// <returns></returns>
        public Result GetMenuByCourse(GF_CourseMenu objMenu)
        {

            try
            {
                _db = new GolflerEntities();
                var lstMenu = _db.GF_CourseMenu.Where(x => x.CourseID == objMenu.CourseID).Select(y => new
                {
                    CourseMenuID = y.ID,
                    CategoryID = y.CategoryID,
                    CategoryName = y.GF_Category.Name,
                    SubCategory = y.GF_Category.GF_SubCategory.Where(q => q.Status == StatusType.Active).Select(x => new
                    {
                        x.ID,
                        x.Name,
                        x.CategoryID,
                        MenuItems = x.GF_MenuItems.Where(z => z.Status == StatusType.Active).Select(z => new
                        {
                            z.ID,
                            z.Name,
                            z.Amount
                        }),
                    }),
                    y.GF_CourseInfo.PlateformFee,
                    y.GF_CourseInfo.Tax,
                    PromoCode = y.GF_CourseInfo.GF_PromoCode.Where(a => a.Status == StatusType.Active).Select(a => new
                    {
                        a.ID,
                        a.Discount,
                        a.PromoCode,
                        a.ReferenceID,
                        a.CourseID,
                        a.CreatedDate,
                        a.ExpiryDate,
                        a.ReferenceType
                    }),
                    y.GF_Category.DisplayOrder
                    #region Stripe accounts info
,
                    StripeAccountGolfler = _db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeAccount").FirstOrDefault().Value
                 ,
                    StripeApiKeyGolfler = _db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeApiKey").FirstOrDefault().Value
                 ,
                    StripeAccountCourse = _db.GF_Settings.Where(x => x.CourseID == objMenu.CourseID && x.Name == "StripeAccount").FirstOrDefault().Value == null ? "0" : _db.GF_Settings.Where(x => x.CourseID == objMenu.CourseID && x.Name == "StripeAccount").FirstOrDefault().Value
                 ,
                    StripeApiKeyCourse = _db.GF_Settings.Where(x => x.CourseID == objMenu.CourseID && x.Name == "StripeApiKey").FirstOrDefault().Value == null ? "0" : _db.GF_Settings.Where(x => x.CourseID == objMenu.CourseID && x.Name == "StripeApiKey").FirstOrDefault().Value
                    #endregion
                }).OrderBy(y => y.DisplayOrder).ToList();


                if (lstMenu != null && lstMenu.Count() > 0)
                {
                    return new Result
                    {
                        Id = 1,
                        Status = 1,
                        Error = "Success",
                        record = lstMenu
                    };

                }
                return new Result { Id = 0, Status = 0, Error = "No record found!!", record = null };
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }

        #endregion

        #region Get menu Item by Course ID New

        /// <summary>
        /// Create By: Amit Kumar
        /// Created By: 23 April 2015
        /// Purpose: Get menu items by CourseID
        /// </summary>
        /// <param name="objMenu"></param>
        /// <returns></returns>
        public Result GetMenuByCourseNew(GF_CourseVisitLog obj)
        {
            try
            {
                _db = new GolflerEntities();

                #region Set Parent Club ID

                var courseData = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == obj.CourseID && !(x.IsClubHouse ?? true));
                if (courseData != null)
                {
                    obj.CourseID = courseData.ClubHouseID;
                }

                #endregion

                var lstGolferPlateformFee = _db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == Settings.GolferPlateFormfee && x.CourseID == obj.CourseID);
                var GolferPlatformFee = (lstGolferPlateformFee != null ? Convert.ToDecimal(lstGolferPlateformFee.Value) : 0);
                var lstCoursePlatformfee = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == (obj.CourseID ?? 0));
                var CoursePlatformfee = (lstCoursePlatformfee.IsPlateformFeeActive ?? false) ? GolferPlatformFee : GolferPlatformFee + lstCoursePlatformfee.PlateformFee;
                var Tax = lstCoursePlatformfee.Tax;

                var lstMenu = _db.GF_CourseMenu.Where(x => x.CourseID == obj.CourseID).ToList().Select(y => new
                {
                    CourseMenuID = y.ID,
                    CategoryID = y.CategoryID,
                    CategoryName = y.GF_Category.Name,
                    SubCategory = y.GF_CourseFoodItem.Where(q => q.AllowStatus == StatusType.Active &&
                        q.GF_SubCategory.Status == StatusType.Active).Select(x => new
                    {
                        ID = x.SubCategoryID,
                        x.GF_SubCategory.Name,
                        x.GF_SubCategory.CategoryID,
                        MenuItems = x.GF_CourseFoodItemDetail.Where(m => m.Status == StatusType.Active).ToList().Select(m => new
                        {
                            ID = m.MenuItemID,
                            Name = m.GF_MenuItems.Name,
                            MenuOption = m.GF_CourseFoodItemOption.Where(k => k.CourseID == obj.CourseID && (k.IsActive ?? false)).Select(k => new { ID = k.MenuItemOptionID, Name = k.Name }),
                            //MenuOption = m.GF_MenuItems.GF_MenuItemOption.Where(k => k.Status == StatusType.Active).Select(k => new { k.ID, k.Name }),

                            //For Amount: If platform fee is active then simple price is applicable, on other hand add commission on per item as decided by super admin
                            Amount = (y.GF_CourseInfo.IsPlateformFeeActive ?? false) ? (m.Price ?? 0) :
                                      (m.Price ?? 0) + (((m.Price ?? 0) * (y.GF_CourseInfo.GF_FoodCommission.FirstOrDefault(z => z.CategoryID == y.CategoryID &&
                                                 z.CourseID == obj.CourseID).Commission ?? 0)) / 100),
                            TaxAmount = ((y.GF_CourseInfo.IsPlateformFeeActive ?? false) ? (m.Price ?? 0) :
                                            (m.Price ?? 0) + (((m.Price ?? 0) * (y.GF_CourseInfo.GF_FoodCommission.FirstOrDefault(z => z.CategoryID == y.CategoryID &&
                                                 z.CourseID == obj.CourseID).Commission ?? 0)) / 100)) * ((y.TaxPercentage ?? 0) / 100),
                            //AppliedPromoCode = string.Join(",", m.GF_CourseFoodItem.GF_CourseInfo.GF_PromoCode.Where(z => z.ReferenceID == m.MenuItemID && z.ReferenceType == "M" && z.CourseID == obj.CourseID).Select(q => q.PromoCode).ToList()
                            //                             .Union(m.GF_CourseFoodItem.GF_CourseInfo.GF_PromoCode.Where(z => z.ReferenceID == y.CategoryID && z.ReferenceType == "C" && z.CourseID == obj.CourseID).Select(q => q.PromoCode).ToList())),
                            AppliedPromoCode = m.GF_CourseFoodItem.GF_CourseInfo.GF_PromoCode.Where(z => z.ReferenceID == m.MenuItemID && z.ReferenceType == "M" && z.CourseID == obj.CourseID).Select(q => new { PromoID = q.ID, PromoCode = q.PromoCode, Discount = q.Discount, DiscountType = (q.DiscountType ?? "P") }).ToList()
                                        .Union(m.GF_CourseFoodItem.GF_CourseInfo.GF_PromoCode.Where(z => z.ReferenceID == y.CategoryID && z.ReferenceType == "C" && z.CourseID == obj.CourseID).Select(q => new { PromoID = q.ID, PromoCode = q.PromoCode, Discount = q.Discount, DiscountType = (q.DiscountType ?? "P") }).ToList())
                                        .Union(m.GF_CourseFoodItem.GF_CourseInfo.GF_PromoCode.Where(z => (z.ReferenceID ?? 0) == 0 && z.ReferenceType == "A" && z.CourseID == obj.CourseID).Select(q => new { PromoID = q.ID, PromoCode = q.PromoCode, Discount = q.Discount, DiscountType = "TA" }).ToList()),
                            IsItemSoldOut = ((m.Quantity ?? 0) > 0 ? 0 : 1),
                            Quantity = (m.Quantity ?? 0)
                        }).AsQueryable()
                    }).Where(q => q.MenuItems.Count() > 0),
                    //PlateformFee = (y.GF_CourseInfo.IsPlateformFeeActive ?? false) ? GolferPlatformFee + y.GF_CourseInfo.PlateformFee : GolferPlatformFee,
                    PlateformFee = GolferPlatformFee,
                    y.GF_CourseInfo.Tax,
                    PromoCode = y.GF_CourseInfo.GF_PromoCode.Where(a => a.Status == StatusType.Active &&
                        a.CourseID == obj.CourseID).Select(a => new
                    {
                        a.ID,
                        a.Discount,
                        a.PromoCode,
                        a.ReferenceID,
                        a.CourseID,
                        a.CreatedDate,
                        a.ExpiryDate,
                        a.ReferenceType,
                        DiscountType = ((a.DiscountType ?? "P") == "P" ? "P" : "TA")
                    }),
                    y.GF_Category.DisplayOrder,
                    StripeAccountGolfler = "",//_db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeAccount").FirstOrDefault() == null ? "0" : _db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeAccount").FirstOrDefault().Value,
                    StripeApiKeyGolfler = "",//_db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeApiKey").FirstOrDefault() == null ? "0" : _db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeApiKey").FirstOrDefault().Value,
                    StripeAccountCourse = "",//_db.GF_Settings.Where(x => x.CourseID == obj.CourseID && x.Name == "StripeAccount").FirstOrDefault() == null ? "0" : _db.GF_Settings.Where(x => x.CourseID == obj.CourseID && x.Name == "StripeAccount").FirstOrDefault().Value,
                    StripeApiKeyCourse = ""//_db.GF_Settings.Where(x => x.CourseID == obj.CourseID && x.Name == "StripeApiKey").FirstOrDefault() == null ? "0" : _db.GF_Settings.Where(x => x.CourseID == obj.CourseID && x.Name == "StripeApiKey").FirstOrDefault().Value
                }).OrderBy(y => y.DisplayOrder).ToList();

                //Revert Back club id to course id
                if (courseData != null)
                {
                    obj.CourseID = courseData.ID;
                }

                #region Course Holes Detail

                var courseBuilderID = _db.GF_CourseBuilder.FirstOrDefault(x => x.CourseID == obj.CourseID && x.CoordinateType == "O");

                var courseRec = _db.GF_CourseBuilderRecDates.FirstOrDefault(x => x.RecDate == DateTime.Now && x.Status == StatusType.Active);
                long? courseBuilderId = (courseRec != null ? courseRec.CourseBuilderId : (courseBuilderID != null ? courseBuilderID.ID : 0));

                var lstCourseHole = _db.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == (courseBuilderId ?? 0)).ToList()
                                    .Select(x => new
                                    {
                                        x.HoleNumber,
                                        x.Latitude,
                                        x.Longitude,
                                        DragItemType = DragItemType.GetFullDragItemType(x.DragItemType)
                                    });

                #endregion

                #region Maintain Course Visit History of Golfer

                ///This table is used to maintain the history of golfer user
                ///which tells that which coures he/she visted

                var visitHistory = _db.GF_CourseVisitLog.FirstOrDefault(x => x.GolferID == obj.GolferID &&
                    x.CourseID == obj.CourseID);

                if (visitHistory != null)
                {
                    visitHistory.ModifyDate = DateTime.Now;
                    _db.SaveChanges();
                }
                else
                {
                    visitHistory = new GF_CourseVisitLog();
                    visitHistory.GolferID = obj.GolferID;
                    visitHistory.CourseID = obj.CourseID;
                    visitHistory.CreatedDate = DateTime.Now;
                    _db.GF_CourseVisitLog.Add(visitHistory);
                    _db.SaveChanges();
                }

                #region Update Golfer User Status

                var golferUserList = _db.GF_GolferUser.Where(x => x.GolferID == obj.GolferID).ToList();
                if (golferUserList != null)
                {
                    if (golferUserList.Count > 0)
                    {
                        //CommonFunctions.GolferLogoutExceptCurrentCourse(Convert.ToInt64(obj.GolferID), Convert.ToInt64(obj.CourseID), ref _db);

                        try
                        {

                            Int64 userid = Convert.ToInt64(obj.GolferID);
                            Int64 courseid = Convert.ToInt64(obj.CourseID);
                            // var _db = new GolflerEntities();
                            var objUser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);


                            #region Quit from all Game Round
                            var allActiveGameRound = _db.GF_GamePlayRound.Where(y => y.GolferID == userid && y.IsQuit == false && y.CourseID != courseid).ToList();
                            foreach (var gameplay in allActiveGameRound)
                            {
                                gameplay.IsQuit = true;
                                _db.SaveChanges();
                            }
                            #endregion

                            #region delete relation with course
                            _db.GF_GolferUser.Where(x => x.GolferID == userid && x.CourseID != courseid).ToList().ForEach(x => _db.GF_GolferUser.Remove(x));
                            _db.SaveChanges();
                            #endregion

                            #region delete from paceofplay
                            _db.GF_GolferPaceofPlay.Where(x => x.GolferId == userid && x.Status != StatusType.Delete && x.CourseId != courseid).ToList().ForEach(x => _db.GF_GolferPaceofPlay.Remove(x));
                            _db.SaveChanges();
                            #endregion
                        }
                        catch
                        {

                        }
                    }
                }

                var golferUser = _db.GF_GolferUser.FirstOrDefault(x => x.GolferID == obj.GolferID);

                bool deleteOldEntries = false;
                if (golferUser != null)
                {
                    #region remove data for changing location if no course relation in golfer user table
                    if (golferUser.CourseID != obj.CourseID)
                    {
                        deleteOldEntries = true;
                    }
                    #endregion

                    _db.GF_GolferUser.Where(x => x.GolferID == obj.GolferID).ToList().ForEach(x => _db.GF_GolferUser.Remove(x));
                    _db.SaveChanges();
                }
                else
                {
                    #region remove data for changing location if no course relation in golfer user table
                    deleteOldEntries = true;
                    #endregion
                }

                if (deleteOldEntries)
                {
                    try
                    {
                        _db.GF_SP_deleteGolferChangingLocation_unused(Convert.ToInt64(obj.GolferID));
                        //  _db.GF_GolferChangingLocation.Where(y => y.GolferId == obj.GolferID).ToList().ForEach(x => _db.GF_GolferChangingLocation.Remove(x));
                        // _db.SaveChanges();
                    }
                    catch
                    { }
                }

                //This table is used to link the golfer user with current course where he/she
                golferUser = new GF_GolferUser();
                golferUser.CourseID = obj.CourseID;
                golferUser.GolferID = obj.GolferID;
                _db.GF_GolferUser.Add(golferUser);
                _db.SaveChanges();

                #endregion

                #endregion

                decimal searchRadius = CommonFunctions.GetRadiusInMeters(Convert.ToInt64(obj.CourseID));
                Int64 timeInterval = CommonFunctions.GetIntervalForCourse(Convert.ToInt64(obj.CourseID));
                Int64 GolferIdleStateHours = CommonFunctions.GetIdleStateHours();

                #region TimeZone update

                var GolferDetails = _db.GF_Golfer.Where(x => x.GF_ID == obj.GolferID).FirstOrDefault();
                if (GolferDetails != null)
                {
                    var timeZone = _db.GF_Timezone.FirstOrDefault(x => x.timezone_standard_identifier.CompareTo(obj.strTimeZone) == 0);

                    GolferDetails.strTimeZone = obj.strTimeZone;
                    GolferDetails.TimeZoneId = timeZone != null ? timeZone.timezone_id : 0;
                    _db.SaveChanges();
                }

                #endregion

                //Revert Back course id to club id
                if (courseData != null)
                {
                    obj.CourseID = courseData.ClubHouseID;
                }

                if (lstMenu != null && lstMenu.Count() > 0)
                {
                    return new Result
                    {
                        Id = 1,
                        Status = 1,
                        Error = "Success",
                        record = new
                        {
                            lstMenu,
                            lstCourseHole,
                            searchRadius,
                            timeInterval,
                            GolferIdleStateHours
                        }
                    };

                }
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = "No record found!!",
                    record = new
                    {
                        lstMenu = lstMenu,
                        lstCourseHole = lstCourseHole,
                        searchRadius,
                        timeInterval,
                        GolferIdleStateHours
                    }
                };
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }

        #endregion

        #region Promo Code Check

        /// <summary>
        /// Create By: Amit Kumar
        /// Created By: 11 June 2015
        /// Purpose: Check either enter promo code is valid or not
        /// </summary>
        /// <param name="promoCode"></param>
        /// <returns></returns>
        public Result PromoCodeValid(GF_PromoCode promoCode)
        {
            try
            {
                _db = new GolflerEntities();

                var lstPromoCode = _db.GF_PromoCode.FirstOrDefault(x => x.PromoCode.ToLower() == promoCode.PromoCode.ToLower() &&
                    x.Status == StatusType.Active &&
                    x.CourseID == promoCode.CourseID);

                if (lstPromoCode != null)
                {
                    if ((lstPromoCode.IsOneTimeUse ?? false) && (lstPromoCode.IsUsed ?? false))
                    {
                        return new Result { Id = 0, Status = 0, Error = "Invalid Promo Code", record = null };
                    }
                    else if ((lstPromoCode.IsOneTimeUse ?? false) && !(lstPromoCode.IsUsed ?? false))
                    {
                        return new Result { Id = 0, Status = 1, Error = "Success", record = null };
                    }
                    else if (!(lstPromoCode.IsOneTimeUse ?? false))
                    {
                        return new Result { Id = 0, Status = 1, Error = "Success", record = null };
                    }
                    else
                    {
                        return new Result { Id = 0, Status = 0, Error = "Invalid Promo Code", record = null };
                    }
                }
                else
                {
                    return new Result { Id = 0, Status = 0, Error = "Invalid Promo Code", record = null };
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }

        #endregion
    }

    public partial class GF_CourseFoodItemDetail
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string AppliedPromoCode { get; set; }
    }
}