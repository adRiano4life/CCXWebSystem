using System.IO;
using Microsoft.AspNetCore.Http;

namespace WebStudio.Services
{
    public class FileUploadService
    {
        public async void Upload(string path, IFormFile file)
        {
            using var stream = new FileStream(Program.PathToFiles + path, FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}