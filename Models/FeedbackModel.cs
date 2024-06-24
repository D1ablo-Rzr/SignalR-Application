using System;
using System.Collections.Generic;
namespace FileUpload.Models
{
    [Serializable]
    public class FeedbackModel
    {
        public int currentCount { get; set; }
        public string currentPercent { get; set; }
        public int UploadCount { get; set; }
    }
}
