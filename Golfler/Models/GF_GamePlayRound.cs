//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Golfler.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GF_GamePlayRound
    {
        public GF_GamePlayRound()
        {
            this.GF_GamePlayPlayerInfo = new HashSet<GF_GamePlayPlayerInfo>();
            this.GF_GamePlayScoreCard = new HashSet<GF_GamePlayScoreCard>();
        }
    
        public long ID { get; set; }
        public Nullable<long> CourseID { get; set; }
        public Nullable<long> GolferID { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public Nullable<bool> IsQuit { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string Pressure { get; set; }
        public string TempMin { get; set; }
        public string TempMax { get; set; }
        public string WindSpeed { get; set; }
        public string WeatherDescription { get; set; }
        public string Rain { get; set; }
        public string RoundStartFrom { get; set; }
        public string strTimeZone { get; set; }
        public string Status { get; set; }
    
        public virtual GF_CourseInfo GF_CourseInfo { get; set; }
        public virtual ICollection<GF_GamePlayPlayerInfo> GF_GamePlayPlayerInfo { get; set; }
        public virtual GF_Golfer GF_Golfer { get; set; }
        public virtual ICollection<GF_GamePlayScoreCard> GF_GamePlayScoreCard { get; set; }
    }
}
