namespace PPgram_desktop.MVVM.Model;

internal class ChatMessageModel
{
    #region API
    public string Id { get; set; } = "";
    public string Sender { get; set; } = "";
    public string Name { get; set; } = "";
    public string AvatarSource { get; set; } = "";

    public string Text { get; set; } = "";
    public string Date { get; set; } = "00:00";

    public bool Reply { get; set; }
    public string ReplyText { get; set; } = "";

    public bool Attachment { get; set; }
    public string AttachmentId { get; set; } = "";
    public string AttachmentName { get; set; } = "";
    public string AttachmentSize { get; set; } = "0 MB";
    public string AttachmentFormat { get; set; } = "";
    #endregion

    #region UI
    public bool IsEdited { get; set; }
    public bool IsFirst {  get; set; }
    public bool IsLast { get; set; }
    public bool IsOwn { get; set; }
    public bool InGroup { get; set; }
    #endregion
}
