namespace RenessansAPI.Service.Helpers;

public class EnvironmentHelper
{
    public static string WebRootPath { get; set; }

    public static string AttachmentPath => EnsurePath("Attachment");
    public static string SshKey => EnsurePath("SshKey");
    public static string SertificatePath => EnsurePath("Sertificate");
    public static string FilePath => EnsurePath("Images");

    private static string EnsurePath(string folderName)
    {
        var path = Path.Combine(WebRootPath, folderName);

        // Papka mavjud bo‘lmasa, avtomatik yaratadi
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}
