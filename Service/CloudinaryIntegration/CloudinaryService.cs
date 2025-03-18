using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using EzConDo_Service.Cloudinary;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace EzConDo_Service.CloudinaryIntegration
{
    public class CloudinaryService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
            );
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return null;
            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                    Folder = "EzCondo_Cloud"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

                if (uploadResult.Error != null)
                {
                    return null;
                }
                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> DeleteImageAsync(string url)
        {
            // Tìm vị trí "/upload/" và lấy phần sau đó làm publicId
            string pattern = @"/upload/v\d+/";
            Match match = Regex.Match(url, pattern);

            string publicId = url.Substring(match.Index + match.Length);
            publicId = Path.ChangeExtension(publicId, null); // Loại bỏ phần mở rộng (".png", ".jpg", ...)

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result == "ok")
            {
                return $"Ảnh {url} đã xóa thành công";
            }
            return "Xóa ảnh thất bại";
        }
    }
}
