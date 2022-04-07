using ERP.Common;
using ERP.Web.API.Domain.Interfaces.Mobile.General;
using ERP.Web.API.Domain.Models.Mobile.General;

namespace ERP.Web.API.Domain.Services.Mobile.General;

public class ImageService : IImageService
{
    public SaveResult AddImage(ImageModel data)
    {
        var result = new SaveResult(false);
            
        try
        {
            var bytes = Convert.FromBase64String(data.FileByte);

            string filedir = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/upload/{data.Folder}");


            if (!Directory.Exists(filedir))
            { //check if the folder exists;
                Directory.CreateDirectory(filedir);
            }
            string file = Path.Combine(filedir, data.FileName);
            //Debug.WriteLine(file);
            //Debug.WriteLine(File.Exists(file));

            if (bytes.Length > 0)
            {
                using (var stream = new FileStream(file, FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }

            result.Success = true;
            result.Data = $"upload/{data.Folder}/{data.FileName}";
            result.Message = "Gambar berhasil disimpan.";
            return result;
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }
    }
}