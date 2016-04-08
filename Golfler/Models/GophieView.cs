using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    public class GophieView
    {
        public GF_Messages MsgObj { get; private set; }
       
        protected GolflerEntities Db;

        public string Message { get; private set; }

        #region Constructors
        public GophieView()
        {
            Db = new GolflerEntities();
        }

      
        #endregion

        public IQueryable<GF_Messages> GetMessagesSent(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
         {
             DateTime dtNow = DateTime.UtcNow.Date;
             IQueryable<GF_Messages> list = Db.GF_Messages.Where(x => (x.MsgFrom == LoginInfo.UserId) && (x.IsMessagesFromGolfer == "0"));

            list = list.ToList().Where(x => x.CreatedDate.Value.Date == dtNow).AsQueryable();
            totalRecords = list.Count();
            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}