using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

namespace RestMasterService.WebApp
{
    [Route("/UploadFMU", "POST")]
    public class Upload
    {
        public string Url { get; set; }
    }
    
    
    public class FMUFileService :Service
    {

        private string targetDir;

        public FMUFileService()
        {
            targetDir = Environment.GetEnvironmentVariable("PHYSIOVALUES_UPLOAD_DIR");
            if (targetDir==null) targetDir = "c:\\Users\\tomaton\\Documents\\KOFRLAB-simenv\\Release\\worker8.release\\";
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
        }
        public object Post(Upload request)
        {
            if (request.Url != null)
            {
                var newfilename = Path.GetFileName(request.Url);
                var newFilePath = Path.Combine(targetDir, newfilename);
                if (!newfilename.ToLower().EndsWith(".fmu")) throw new FormatException("Only *.FMU files are allowed to upload. Not the '"+newfilename+"'.");
                File.WriteAllBytes(newFilePath, request.Url.GetBytesFromUrl());
            }

            foreach (var uploadedFile in Request.Files.Where(uploadedFile => uploadedFile.ContentLength > 0))
            {
                var newFilePath = Path.Combine(targetDir, uploadedFile.FileName);
                if (!newFilePath.ToLower().EndsWith(".fmu")) throw new FormatException("Only *.FMU files are allowed to upload. Not the '" + uploadedFile.FileName + "'.");
                uploadedFile.SaveTo(newFilePath);
            }

            return ("uploaded"); //HttpResult.Status201Created();
        }
    }
}