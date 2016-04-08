using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    /// <summary>
    /// Created By: Renuka Hira
    /// Created on: 1st July, 2013
    /// </summary>
    /// <remarks>Class for Modules</remarks>
    public class Module
    {
        protected GolflerEntities Db;

        public Module()
        {
            Db = new GolflerEntities();
        }

        public IQueryable<GF_Modules> GetModules(string modulevalue = "", bool adminSec = false)
        {
            if (string.IsNullOrEmpty(modulevalue))
                return Db.GF_Modules.Where(x => x.IsAdmin == adminSec).OrderBy(m => new { m.IsAdmin, m.IsFrontEnd, m.OrderBy }).AsQueryable();
            return Db.GF_Modules.Where(x => x.Value == modulevalue && x.IsAdmin == adminSec).OrderBy(x => x.OrderBy).AsQueryable();
        }
    }
}