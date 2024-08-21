namespace PPgram_desktop.MVVM.Model;

internal class MessageModel
{
    #region API
    public string Id { get; set; }
    public string From { get; set; }
    public int To { get; set; }
    

    public string Text { get; set; } = "";
    public string Date { get; set; } = "00:00";

    public bool Reply { get; set; }
    public int ReplyTo { get; set; }

    public bool Attachment { get; set; }
    public string AttachmentId { get; set; }
    public string AttachmentName { get; set; }
    public string AttachmentSize { get; set; }
    public string AttachmentFormat { get; set; }
    #endregion

    #region UI
    public string Name { get; set; } = "";
    public string AvatarSource { get; set; } = "/Asset/default_avatar.png";


    public bool IsEdited { get; set; }
    public bool IsFirst {  get; set; }
    public bool IsLast { get; set; }
    public bool IsOwn { get; set; }
    public bool InGroup { get; set; }
    #endregion
}
