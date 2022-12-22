namespace DiscordClientProxy;

public class HttpUtilities
{
    public static string GetContentTypeByFilename(string filename)
    {
        var ext = filename.Split(".").Last();
        var contentType = ext switch
        {
            //text types
            "html" => "text/html",
            "js" => "text/javascript",
            "css" => "text/css",
            "txt" => "text/plain",
            "csv" => "text/csv",
            //image types
            "apng" => "image/apng",
            "gif" => "image/gif",
            "jpg" => "image/jpeg",
            "png" => "image/png",
            "svg" => "image/svg+xml",
            "webp" => "image/webp",
            "ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
        
        if(contentType == "application/octet-stream")
        {
            Console.WriteLine($"[WARN] Unknown content type for {filename}");
        }
        
        return contentType;
    }
}