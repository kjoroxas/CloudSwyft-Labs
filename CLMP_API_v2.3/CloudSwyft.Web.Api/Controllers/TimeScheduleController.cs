using CloudSwyft.Web.Api.Models;
using ExcelDataReader;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CloudSwyft.Web.Api.Controllers
{

    [RoutePrefix("api/TimeSchedule")]
    public class TimeScheduleController: ApiController
    {
        private VirtualEnvironmentDbContext _db = new VirtualEnvironmentDbContext();
        private VirtualEnvironmentDBTenantContext _dbTenant = new VirtualEnvironmentDBTenantContext();


        [HttpPost]
        [Route("AddBulkTimeSchedule")]
        public async Task<IHttpActionResult> AddBulkTimeSchedule(int VEProfileID, string TimeZone, DateTime StartTime, int IdleTime, int ScheduledBy)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                DataSet dsexcelRecords = new DataSet();
                IExcelDataReader reader = null;
                HttpPostedFile Inputfile = null;
                Stream FileStream = null;
                var startDateTime= StartTime.ToUniversalTime();
                startDateTime.AddSeconds(-startDateTime.Second);
                startDateTime.AddMilliseconds(-startDateTime.Millisecond);


                var listOfExisting = _db.TimeSchedules.Join(_db.MachineLabs, a => a.MachineLabsId, b => b.MachineLabsId, (a, b) => new { a, b })
                                        .Select(x => x.a).ToList();

                if (httpRequest.Files.Count > 0)
                {
                    Inputfile = httpRequest.Files[0];
                    FileStream = Inputfile.InputStream;

                    if (Inputfile != null && FileStream != null)
                    {
                        if (Inputfile.FileName.EndsWith(".xls"))
                            reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                        else if (Inputfile.FileName.EndsWith(".xlsx"))
                            reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
                        else if (Inputfile.FileName.EndsWith(".csv"))
                            reader = ExcelReaderFactory.CreateCsvReader(FileStream);
                        else
                            return BadRequest();

                        dsexcelRecords = reader.AsDataSet();
                        reader.Close();

                        List<string> student = new List<string>();

                        if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                        {
                            await Task.Run(() =>
                            {
                                DataTable dtStudentRecords = dsexcelRecords.Tables[0];

                                foreach (DataRow item in dtStudentRecords.Rows.Cast<DataRow>().Skip(1))
                                {
                                    student.Add(item.ItemArray[0].ToString());
                                }
                                IdentityResult result;

                                foreach (var email in student)
                                {
                                    try
                                    {
                                        var sched = new TimeSchedules();
                                        var userId = _db.CloudLabUsers.Where(q => q.Email == email).FirstOrDefault().UserId;
                                        var timeSched = _db.TimeSchedules.Where(q => q.UserId == userId && q.VEProfileID == VEProfileID && q.IsEnabled).ToList();
                                        var vmExist = _db.MachineLabs.Any(q => q.UserId == userId && q.VEProfileId == VEProfileID);
                                        
                                        if(vmExist)
                                        {
                                            var machineLabsId = _db.MachineLabs.Where(q => q.UserId == userId && q.VEProfileId == VEProfileID).FirstOrDefault().MachineLabsId;

                                            if (listOfExisting.Count != 0)
                                            {
                                                if (listOfExisting.Intersect(timeSched).Any())
                                                {
                                                    var timeScheds = _db.TimeSchedules.Where(q => q.UserId == userId && q.VEProfileID == VEProfileID && q.IsEnabled).FirstOrDefault();
                                                    timeScheds.StartTime = startDateTime;
                                                    timeScheds.ScheduledBy = ScheduledBy;
                                                    timeScheds.DateModified = DateTime.UtcNow;
                                                    timeScheds.IdleTime = IdleTime;
                                                    timeScheds.IsEnabled = true;
                                                    timeScheds.TimeZone = TimeZone;

                                                    _db.SaveChanges();
                                                }
                                                else
                                                {
                                                    sched.StartTime = startDateTime;
                                                    sched.DateCreated = DateTime.UtcNow;
                                                    sched.ScheduledBy = ScheduledBy;
                                                    sched.UserId = userId;
                                                    sched.DateModified = DateTime.UtcNow;
                                                    sched.IdleTime = IdleTime;
                                                    sched.IsEnabled = true;
                                                    sched.TimeZone = TimeZone;
                                                    sched.VEProfileID = VEProfileID;
                                                    sched.MachineLabsId = machineLabsId;

                                                    _db.TimeSchedules.Add(sched);
                                                    _db.SaveChanges();

                                                }
                                            }
                                            else
                                            {
                                                sched.StartTime = startDateTime;
                                                sched.DateCreated = DateTime.UtcNow;
                                                sched.ScheduledBy = ScheduledBy;
                                                sched.UserId = userId;
                                                sched.DateModified = DateTime.UtcNow;
                                                sched.IdleTime = IdleTime;
                                                sched.IsEnabled = true;
                                                sched.TimeZone = TimeZone;
                                                sched.VEProfileID = VEProfileID;
                                                sched.MachineLabsId = machineLabsId;

                                                _db.TimeSchedules.Add(sched);
                                                _db.SaveChanges();
                                            }
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                    }

                                }
                            });
                        }
                        else
                            return BadRequest();
                    }
                    else
                        return BadRequest();
                }
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpGet]
        [Route("GetTimeShedule")]
        public IHttpActionResult GetTimeSchedule(int userGroup)
        {
            var scheds = _db.TimeSchedules.Join(_db.VEProfiles, a => a.VEProfileID, b => b.VEProfileID, (a, b) => new { a, b })
                .Join(_db.CloudLabUsers, c => c.a.UserId, d => d.UserId, (c, d) => new { c, d })
                .Join(_db.MachineLabs, e => e.c.a.MachineLabsId, f => f.MachineLabsId, (e, f) => new { e, f })
                .Where(w => w.e.d.UserGroup == userGroup)
                .Select(q => new TimeSchedulesParam
                {
                    TimeScheduleId = q.e.c.a.TimeScheduleId,
                    Email = q.e.d.Email,
                    CourseName = q.e.c.b.Name,
                    StartTime = q.e.c.a.StartTime,
                    TimeZone = q.e.c.a.TimeZone,
                    CourseEmail = q.e.d.Email + " " + q.e.c.b.Name
                }).ToList().OrderBy(l=>l.Email);
            foreach (var item in scheds)
            {
                TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById(item.TimeZone);

                DateTime timeDate = TimeZoneInfo.ConvertTime(item.StartTime, TimeZoneInfo.Utc, tst);

                item.StartTime = timeDate;

            }
            return Ok(scheds);
        }

        [HttpDelete]
        [Route("DeleteTimeShedule")]
        public IHttpActionResult DeleteTimeShedule(int schedId, int userGroup)
        {
            var sched = _db.TimeSchedules.Find(schedId);

            if (sched != null)
            {
                _db.TimeSchedules.Remove(sched);
                _db.SaveChanges();
            }

            var scheds = _db.TimeSchedules.Join(_db.VEProfiles, a => a.VEProfileID, b => b.VEProfileID, (a, b) => new { a, b })
               .Join(_db.CloudLabUsers, c => c.a.UserId, d => d.UserId, (c, d) => new { c, d })
               .Join(_db.MachineLabs, e => e.c.a.MachineLabsId, f => f.MachineLabsId, (e, f) => new { e, f })
               .Where(w => w.e.d.UserGroup == userGroup)
               .Select(q => new
               {
                   TimeScheduleId = q.e.c.a.TimeScheduleId,
                   Email = q.e.d.Email,
                   CourseName = q.e.c.b.Name,
                   StartTime = q.e.c.a.StartTime,
                   TimeZone = q.e.c.a.TimeZone,
                   CourseEmail = q.e.d.Email + " " + q.e.c.b.Name
               }).ToList().OrderBy(l => l.Email);

            return Ok(scheds);
        }
    }
}