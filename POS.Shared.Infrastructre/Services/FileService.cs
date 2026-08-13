#region before
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Modules.Shared.Application.IService;
//namespace Modules.Shared.Application.Services
//{
//    public class FileService : IFileService
//    {
//        private readonly IWebHostEnvironment webHostEnvironment;

//        public FileService(IWebHostEnvironment webHostEnvironment)
//        {
//            this.webHostEnvironment = webHostEnvironment;
//        }

//        public async Task DeleteFileAsync(string FilePath)
//        {
//            // => /wwwroot/
//            //var fullPath = Path.Combine(webHostEnvironment.WebRootPath, FilePath);
//            var fullPath = webHostEnvironment.WebRootPath + "/" + FilePath;
//            if (File.Exists(fullPath))
//            {
//                File.Delete(fullPath);



//            }
//        }

//        public async Task DeleteAllFilesAsync(List<string> FilePaths)
//        {
//            foreach (string FilePath in FilePaths)
//            {
//                await DeleteFileAsync(FilePath);
//            }
//        }

//        public async Task<string> UploadFileAsync(IFormFile file, string folder)
//        {
//            try
//            {

//                if (file == null || file.Length == 0) return string.Empty;


//                //var path = Path.Combine(webHostEnvironment.WebRootPath, folder);
//                var path = webHostEnvironment.WebRootPath + "/" + folder;
//                var extension = Path.GetExtension(file.FileName);
//                var fileName = $"{Guid.NewGuid().ToString()}{extension}";

//                if (!Directory.Exists(path))
//                {
//                    Directory.CreateDirectory(path);
//                }

//                var fullPath = Path.Combine(path, fileName);

//                using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
//                {
//                    await file.CopyToAsync(fileStream);
//                    fileStream.Flush();
//                }

//                return Path.Combine(folder, fileName).Replace("\\", "/"); // Return relative path
//            }
//            catch
//            {
//                return string.Empty;
//            }
//        }

//        // New function to retrieve image as an IFormFile
//        public async Task<IFormFile> GetFileAsIFormFileAsync(string imageSrc)
//        {
//            try
//            {
//                var fullPath = webHostEnvironment.WebRootPath + "/" + imageSrc;
//                if (!File.Exists(fullPath))
//                {
//                    return null; // Return null if the file doesn't exist
//                }

//                // Read the file into a byte array or stream
//                var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(fullPath));

//                // Create an IFormFile from the MemoryStream
//                IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "profilePicture", Path.GetFileName(fullPath))
//                {
//                    Headers = new HeaderDictionary(),
//                    ContentType = "image/jpeg" // Set content type (change based on file type)
//                };

//                return formFile;
//            }
//            catch
//            {
//                return null; // Return null in case of an error
//            }
//        }

//        public async Task<byte[]> GetFileAsByteArrayAsync(string imageSrc)
//        {
//            try
//            {
//                var fullPath = Path.Combine(webHostEnvironment.WebRootPath, imageSrc);
//                if (!File.Exists(fullPath))
//                {
//                    return null; // Return null if the file doesn't exist
//                }

//                var fileBytes = await File.ReadAllBytesAsync(fullPath);
//                return fileBytes;
//            }
//            catch
//            {
//                return null; // Return null in case of an error
//            }
//        }

//        public async Task<string> UploadFileAsync(IFormFile file, string folder, HttpRequest request)
//        {
//            try
//            {
//                if (file == null || file.Length == 0) return string.Empty;

//                //var path = Path.Combine(webHostEnvironment.WebRootPath, folder);
//                var path = webHostEnvironment.WebRootPath + "/" + folder;
//                var extension = Path.GetExtension(file.FileName);
//                var fileName = $"{Guid.NewGuid()}{extension}";

//                if (!Directory.Exists(path))
//                {
//                    Directory.CreateDirectory(path);
//                }

//                var fullPath = Path.Combine(path, fileName);

//                using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
//                {
//                    await file.CopyToAsync(fileStream);
//                    fileStream.Flush();
//                }

//                var relativePath = Path.Combine(folder, fileName).Replace("\\", "/");

//                // Build full public URL (e.g., https://localhost:5001/assets/images/photo.jpg)
//                var baseUrl = $"{request.Scheme}://{request.Host}";
//                var publicUrl = $"{baseUrl}/{relativePath}";

//                return publicUrl;
//            }
//            catch
//            {
//                return string.Empty;
//            }
//        }
//    }


//} 
#endregion
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles; // مهم عشان الـ FileExtensionContentTypeProvider
using POS.Shared.Application.IService;
using POS.Shared.Domain;

namespace POS.Shared.Infrastructure.Services // يُفضل نقله للـ Infrastructure
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetWebRootPath()
        {
            var webRoot = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            }

            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }

            return webRoot;
        }

        public async Task<Result> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Result.Failure(new Error("File.Path.Empty", "File path is empty"));

            try
            {
                var fullPath = Path.Combine(
                    GetWebRootPath(),
                    filePath.TrimStart('/', '\\'));

                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    new Error("File.DeleteFailed", ex.Message));
            }
        }

        public async Task<Result> DeleteAllFilesAsync(List<string> filePaths)
        {
            if (filePaths == null || !filePaths.Any())
                return Result.Failure(new Error("File.List.Empty", "Files list is empty"));

            foreach (var filePath in filePaths)
            {
                var result = await DeleteFileAsync(filePath);

                if (!result.IsSuccess)
                    return result;
            }

            return Result.Success();
        }

        public async Task<Result<string>> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return Result<string>.Failure(
                    new Error("File.Invalid", "file is empty"));

            try
            {
                var path = Path.Combine(
                    GetWebRootPath(),
                    folder.TrimStart('/', '\\'));

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var fullPath = Path.Combine(path, fileName);

                using var fileStream = new FileStream(fullPath, FileMode.Create);

                await file.CopyToAsync(fileStream);

                var relativePath = Path.Combine(folder, fileName)
                    .Replace("\\", "/");

                return Result<string>.Success(relativePath);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(
                    new Error("File.UploadFailed", ex.Message));
            }
        }

        public async Task<Result<byte[]>> GetFileAsByteArrayAsync(string imageSrc)
        {
            if (string.IsNullOrWhiteSpace(imageSrc))
                return Result<byte[]>.Failure(
                    new Error("File.Path.Empty", "image path is empty"));

            try
            {
                var fullPath = Path.Combine(
                    GetWebRootPath(),
                    imageSrc.TrimStart('/', '\\'));

                if (!File.Exists(fullPath))
                    return Result<byte[]>.Failure(
                        new Error("File.NotFound", "file not found"));

                var bytes = await File.ReadAllBytesAsync(fullPath);

                return Result<byte[]>.Success(bytes);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure(
                    new Error("File.ReadFailed", ex.Message));
            }
        }

        public async Task<Result<IFormFile>> GetFileAsIFormFileAsync(string imageSrc)
        {
            if (string.IsNullOrWhiteSpace(imageSrc))
                return Result<IFormFile>.Failure(
                    new Error("File.Path.Empty", "image path is empty"));

            try
            {
                var fullPath = Path.Combine(
                    GetWebRootPath(),
                    imageSrc.TrimStart('/', '\\'));

                if (!File.Exists(fullPath))
                    return Result<IFormFile>.Failure(
                        new Error("File.NotFound", "file not found"));

                var memoryStream = new MemoryStream(
                    await File.ReadAllBytesAsync(fullPath));

                var provider = new FileExtensionContentTypeProvider();

                if (!provider.TryGetContentType(fullPath, out var contentType))
                    contentType = "application/octet-stream";

                IFormFile formFile = new FormFile(
                    memoryStream,
                    0,
                    memoryStream.Length,
                    "file",
                    Path.GetFileName(fullPath))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };

                return Result<IFormFile>.Success(formFile);
            }
            catch (Exception ex)
            {
                return Result<IFormFile>.Failure(
                    new Error("File.ReadFailed", ex.Message));
            }
        }

        public async Task<Result<string>> UploadFileWithUrlAsync(
            IFormFile file,
            string folder,
            string baseUrl)
        {
            var uploadResult = await UploadFileAsync(file, folder);

            if (!uploadResult.IsSuccess)
                return Result<string>.Failure(uploadResult.Error);

            var relativePath = uploadResult.Value;

            var publicUrl = $"{baseUrl}/{relativePath.TrimStart('/')}";

            return Result<string>.Success(publicUrl);
        }
    }

}