using System;
using System.Collections.Generic;
using System.Linq;
using Golfler.Repositories;

namespace Golfler.Models
{
    public class Role
    {
        public GF_Roles RoleObj { get; private set; }

        protected GolflerEntities Db;

        public string Message { get; private set; }

        #region Constructors
        public Role()
        {
            Db = new GolflerEntities();
        }

        public Role(long? id)
        {
            Db = new GolflerEntities();
            RoleObj = Db.GF_Roles.FirstOrDefault(u => u.ID == id);
        }
        #endregion

        public IQueryable<GF_Roles> GetRoles(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_Roles> list
                = !String.IsNullOrWhiteSpace(filterExpression)
                      ? Db.GF_Roles.Where(x => x.Name.ToLower().Contains(filterExpression.ToLower()) && x.Status != StatusType.Delete && x.CourseUserId == LoginInfo.CourseId)
                      : Db.GF_Roles.Where(x => x.Status != StatusType.Delete && x.CourseUserId == LoginInfo.CourseId);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public GF_Roles GetRole(long? id, bool adminSec = false)
        {
            Db = new GolflerEntities();
            RoleObj = Db.GF_Roles.FirstOrDefault(u => u.ID == id);
            RoleObj = RoleObj ?? new GF_Roles();

            var mod = new Module();
            var i = 0;
            var mods = mod.GetModules(ModuleValue, adminSec);
            var roleMods = RoleObj.GF_RoleModules;
            RoleObj.ArrRoleModules = new GF_RoleModules[mods.Count()];
            foreach (var m in mods)
            {
                var exestMod = roleMods.FirstOrDefault(x => x.ModuleID == m.ID);
                if (exestMod != null)
                {
                    exestMod.Name = m.Name;
                    exestMod.Value = m.Value;
                    exestMod.IsAdmin = m.IsAdmin;
                    exestMod.IsFrontEnd = m.IsFrontEnd;
                    exestMod.ModuleGroupName = Convert.ToString(Db.GF_Modules_Group.Where(x => x.MD_Id == m.GroupId).Select(x => x.GroupName).FirstOrDefault());
                    RoleObj.ArrRoleModules[i] = exestMod;
                }
                else
                    RoleObj.ArrRoleModules[i] = new GF_RoleModules()
                    {
                        ModuleID = m.ID,
                        Name = m.Name,
                        Value = m.Value,
                        IsAdmin = m.IsAdmin,
                        IsFrontEnd = m.IsFrontEnd,
                        ModuleGroupName = Convert.ToString(Db.GF_Modules_Group.Where(x => x.MD_Id == m.GroupId).Select(x => x.GroupName).FirstOrDefault()),
                    };
                i++;
            }
            return RoleObj;
        }

        public IEnumerable<GF_RoleModules> GetRoleModules()
        {
            return RoleObj.GF_RoleModules;
        }

        public bool Save(GF_Roles obj)
        {
            try
            {
                GF_RoleModules objModuleRole = new GF_RoleModules();

                //if (LoginInfo.IsSuper)
                //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
                //else
                //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Role);

                //if (obj.ID > 0)
                //{
                //    if (!objModuleRole.UpdateFlag)
                //    {
                //        Message = "";//Resources.Resources.unaccess;
                //        return false;
                //    }
                //}
                //else
                //{
                //    if (!objModuleRole.AddFlag)
                //    {
                //        Message = "";//Resources.Resources.unaccess;
                //        return false;
                //    }
                //}
                if (ValidPage(obj))
                {
                    if (obj.ID > 0)
                    {
                        if (RoleObj != null)
                        {

                            RoleObj.Active = obj.Active;

                            RoleObj.Description = obj.Description;
                            RoleObj.Name = obj.Name;
                            RoleObj.ModifiedBy = LoginInfo.UserId;
                            RoleObj.ModifiedOn = DateTime.Now;
                            foreach (var m in obj.ArrRoleModules)
                            {
                                var module = RoleObj.GF_RoleModules.FirstOrDefault(x => x.ModuleID == m.ModuleID);
                                if (module == null)
                                {
                                    module = new GF_RoleModules { ModuleID = m.ModuleID };
                                    RoleObj.GF_RoleModules.Add(module);
                                }

                                module.ReadFlag = m.ReadFlag;
                                //module.DeleteFlag = m.DeleteFlag;
                                //module.AddFlag = m.AddFlag;
                                module.UpdateFlag = m.UpdateFlag;
                                module.DeleteFlag = m.UpdateFlag;
                                module.AddFlag = m.UpdateFlag;
                            }
                            Message = "update";
                        }
                    }
                    else
                    {
                        obj.CourseUserId = LoginInfo.CourseId;
                        obj.CreatedBy = LoginInfo.UserId;
                        obj.CreatedOn = DateTime.Now;
                        foreach (var m in obj.ArrRoleModules)
                        {
                            obj.GF_RoleModules.Add(new GF_RoleModules()
                            {
                                ModuleID = m.ModuleID,
                                ReadFlag = m.ReadFlag,
                                //DeleteFlag = m.DeleteFlag,
                                //AddFlag = m.AddFlag,
                                UpdateFlag = m.UpdateFlag,
                                DeleteFlag = m.UpdateFlag,
                                AddFlag = m.UpdateFlag
                            });
                        }
                        Db.GF_Roles.Add(obj);
                        Message = "add";
                    }
                    Db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Message = ex.ToString();
                return false;
            }
        }

        private bool ValidPage(GF_Roles obj)
        {
            Message = string.Empty;
            if (Db.GF_Roles.Count(x => x.Name.ToLower() == obj.Name.ToLower() &&
                x.ID != obj.ID && x.CourseUserId == LoginInfo.CourseUserId && x.Status != StatusType.Delete) > 0)
                Message = "Name";
            return Message.Length == 0;
        }

        internal bool ChangeStatus(bool status)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();

            //if (LoginInfo.IsSuper)
            //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            //else if (LoginInfo.LoginUserType == UserType.CourseAdmin)
            //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            //else
            //{
            //    if (LoginInfo.LoginUserType == "CK" || LoginInfo.LoginUserType == "CC" || LoginInfo.LoginUserType == "CR" || LoginInfo.LoginUserType == "CA" || LoginInfo.LoginUserType == "CP")
            //    {
            //        objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Role);
            //    }
            //    else
            //    {
            //        objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.RolesAdmin);
            //    }
            //}

            //if (!objModuleRole.UpdateFlag)
            //{
            //    Message = "";//Resources.Resources.unaccess;
            //    return false;
            //}
            if (RoleObj != null)
            {
                RoleObj.Active = !status;
                RoleObj.ModifiedBy = LoginInfo.UserId;
                RoleObj.ModifiedOn = DateTime.Now;
                Db.SaveChanges();

                if (status == true)
                {
                    //var user = Db.GF_Users.Where(x => x.RoleId == RoleObj.ID).ToList();
                    //foreach (var objUser in user)
                    //{
                    //    objUser.RoleId = null;
                    //    Db.SaveChanges();
                    //}
                }
                return true;
            }
            return false;
        }

        internal bool DeleteRoles(long[] ids)
        {
            var roles = Db.GF_Roles.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var r in roles)
            {
                r.Status = StatusType.Delete;
                r.ModifiedBy = LoginInfo.UserId;
                r.ModifiedOn = DateTime.Now;

                #region Remove Roles from admin table
                
                Db.GF_AdminUsers.Where(x => x.RoleId == r.ID).ToList().ForEach(x => x.RoleId = null);

                #endregion
            }
            Db.SaveChanges();
            return true;
        }

        internal GF_Roles GetRoleByUserId(long uId, bool adminSec = false)
        {
            //string logintype = "A";
            //try
            //{
            //    if (LoginInfo.IsSuper)
            //    {
            //        logintype = "A";
            //    }
            //    else
            //    {
            //        if (Convert.ToInt64(LoginInfo.UserId) > 0)
            //        {
            //            logintype = "E";
            //        }
            //        else
            //        {
            //            logintype = "A";
            //        }
            //    }
            //}
            //catch { logintype = "A"; }

            //if (logintype == "A")
            //{
                Users userObj = new Users(uId);
                if (userObj.UserObj != null)
                {
                    if (userObj.UserObj.Type.Contains(UserType.SuperAdmin) || ModuleValue == ModuleValues.AllRights)
                        return GetRole(true, adminSec);
                    else if (Convert.ToInt64(userObj.UserObj.RoleId) > 0)
                        return GetRole(userObj.UserObj.RoleId, adminSec);
                    else
                        return GetRole(false, adminSec);

                }
            //}
            //else
            //{
            //    return GetRoleForEndUsers(true, adminSec);
            //}
            return GetRole(false, adminSec);
        }

        private GF_Roles GetRoleForEndUsers(bool access, bool adminSec)
        {
            Db = new GolflerEntities();
            RoleObj = new GF_Roles();
            ModuleValue = ModuleValues.AllRights;
            var mod = new Module();
            var i = 0;
            if (ModuleValue == ModuleValues.AllRights)
            {
                RoleObj.ArrRoleModules = new GF_RoleModules[1];
                RoleObj.ArrRoleModules[0] = new GF_RoleModules()
                {
                    Value = ModuleValues.AllRights,
                    ReadFlag = access,
                    AddFlag = access,
                    DeleteFlag = access,
                    UpdateFlag = access,
                    IsSpecial = access,
                    IsBasic = true
                };
            }
            else
            {
                var mods = mod.GetModules(ModuleValue, adminSec);
                var roleMods = RoleObj.GF_RoleModules;
                RoleObj.ArrRoleModules = new GF_RoleModules[mods.Count()];
                foreach (var m in mods)
                {
                    RoleObj.ArrRoleModules[i] = new GF_RoleModules()
                    {
                        ModuleID = m.ID,
                        Name = m.Name,
                        Value = m.Value,
                        ReadFlag = access,
                        AddFlag = access,
                        DeleteFlag = access,
                        UpdateFlag = access,
                        IsSpecial = access
                    };
                    i++;
                }
            }
            return RoleObj;
        }


        private GF_Roles GetRole(bool access, bool adminSec)
        {
            Db = new GolflerEntities();
            RoleObj = new GF_Roles();

            var mod = new Module();
            var i = 0;
            if (ModuleValue == ModuleValues.AllRights)
            {
                RoleObj.ArrRoleModules = new GF_RoleModules[1];
                RoleObj.ArrRoleModules[0] = new GF_RoleModules()
                {
                    Value = ModuleValues.AllRights,
                    ReadFlag = access,
                    AddFlag = access,
                    DeleteFlag = access,
                    UpdateFlag = access,
                    IsSpecial = false,
                    IsBasic = true
                };
            }
            else
            {
                var mods = mod.GetModules(ModuleValue, adminSec);
                var roleMods = RoleObj.GF_RoleModules;
                RoleObj.ArrRoleModules = new GF_RoleModules[mods.Count()];
                foreach (var m in mods)
                {
                    RoleObj.ArrRoleModules[i] = new GF_RoleModules()
                    {
                        ModuleID = m.ID,
                        Name = m.Name,
                        Value = m.Value,
                        ReadFlag = access,
                        AddFlag = access,
                        DeleteFlag = access,
                        UpdateFlag = access
                    };
                    i++;
                }
            }
            return RoleObj;
        }

        string ModuleValue;
        public GF_RoleModules GetRoleModuleByUserId(long uId, string module, bool adminSec)
        {
            ModuleValue = module;
            if (module == ModuleValues.AllRights)
            {
                return GetRole(true, adminSec).ArrRoleModules.FirstOrDefault();
            }

            var rolemodule = GetRoleByUserId(uId, adminSec);

            return rolemodule.ArrRoleModules.FirstOrDefault();
        }
    }
}