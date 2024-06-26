﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FileUpload.Models;
using System.IO;
using FileUpload.Hubs;
using Microsoft.AspNetCore.SignalR;
using Alachisoft.NCache.Client;

namespace FileUpload.Controllers
{
    public class UploadController : Controller
    {
        private static ICache _cache = CacheManager.GetCache("ClusteredCache");
        private readonly IHubContext<BroadCastHub> hubContext;
        public UploadController(IHubContext<BroadCastHub> _hubContext)
        {
            hubContext = _hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult UpdateClients()
        { 
            var data = new List<RecordItem>();
            data = _cache.Get<List<RecordItem>>("data");
            SendFeedBack(data);
            return Json(new { data = data });
        }

        public async Task<JsonResult> UploadFile()
        {
            if (_cache.Contains("data"))
            {
                return Json(new { data = UpdateClients() });
            }
            else
            {
                try
                {

                    var counter = 0;
                    var currentCount = 0;

                    var data = new List<RecordItem>();
                    var postedFile = Request.Form.Files;

                    await Task.Delay(500);
                    currentCount++;
                    SendFeedBack(data);

                    if (postedFile.Count <= 0 || postedFile == null)
                        return Json(new { error = true, message = "Empty File was uploaded" });

                    if (postedFile[0] == null || postedFile[0].Length <= 0)
                    {
                        return Json(new { error = true, message = "Empty File was uploaded" });
                    }

                    await Task.Delay(500);
                    currentCount++;
                    SendFeedBack(data);


                    var fileInfo = new FileInfo(postedFile[0].FileName);
                    var extention = fileInfo.Extension;
                    if (extention.ToLower() != ".csv")
                    {
                        return Json(new { error = true, message = "invalid file format" });
                    }

                    using (StreamReader sr = new StreamReader(postedFile[0].OpenReadStream()))
                    {
                        await Task.Delay(500);
                        currentCount++;
                        SendFeedBack(data);

                        while (!sr.EndOfStream)
                        {
                            String Info = sr.ReadLine();
                            String[] Records;
                            if (Info.Contains('\"'))
                            {
                                var row = string.Empty;
                                var model = Info.Replace("\"", "#*").Split('#');
                                foreach (var item in model)
                                {
                                    var d = item.Replace("*,", ",");
                                    if (d.Contains("*"))
                                    {
                                        row += d.Replace("*", "").Replace(",", "");
                                    }
                                    else
                                    {
                                        row += d;
                                    }
                                }
                                Records = new String[row.Split(new char[] { ',' }).Length];
                                row.Split(new char[] { ',' }).CopyTo(Records, 0);

                            }
                            else
                            {
                                Records = new String[Info.Split(new char[] { ',' }).Length];
                                Info.Split(new char[] { ',' }).CopyTo(Records, 0);
                            }
                            var strAmount = Records[7].ToString().Trim();
                            decimal output;
                            if (string.IsNullOrEmpty(strAmount) || !decimal.TryParse(strAmount, out output)) continue;

                            var datafile = new RecordItem()
                            {
                                Company = Records[1].ToString().Trim(),
                                Category = Records[3].ToString().Trim(),
                                City = Records[4].ToString().Trim(),
                                Date = Records[6].ToString().Trim(),
                                Currency = Records[8].ToString().Trim(),
                                Amount = decimal.Parse(Records[7].ToString().Trim()),

                            };

                            data.Add(datafile);

                            counter++;
                            SendFeedBack(data);
                        }

                        sr.Close();
                        sr.Dispose();

                        await Task.Delay(500);
                        currentCount++;
                        SendFeedBack(data);

                    }
                    await Task.Delay(500);
                    currentCount++;
                    _cache.Insert("data", data);
                    SendFeedBack(data);
                    return Json(new { error = false, data = data });

                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        error = true,
                        message = ex.InnerException != null ?
                        ex.InnerException.Message : ex.Message
                    });
                }
            }

        }


        private async void SendFeedBack(List<RecordItem> data)
        {
            foreach (var item in BroadCastHub.connectionIds)
            {
                await hubContext.Clients.Client(item).SendAsync("feedBack", data);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
