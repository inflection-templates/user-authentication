using System.Collections.Generic;

namespace shala.api.modules.communication;

public class EmailAttachment
{
    public string Filename { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}

public class EmailParams
{
    public string Subject { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public string ToName { get; set; } = string.Empty;

    public List<string> Cc { get; set; } = new List<string>();

    public List<string> Bcc { get; set; } = new List<string>();

    public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

    public string Body { get; set; } = string.Empty;

    public string Purpose { get; set; } = "Unspecified";
}
