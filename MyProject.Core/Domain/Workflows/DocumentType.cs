using MyProject.Core.Domain.Common;

namespace MyProject.Core.Domain.Workflows;

/// <summary>
/// Registry entry for a kind of business document (e.g. "LeaveRequest"). Different document types
/// may use different workflows (req 13). The engine references documents by (DocumentTypeId, DocumentId).
/// </summary>
public sealed class DocumentType : IAggregateRoot
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private DocumentType() { }

    public DocumentType(string code, string name)
    {
        Code = code;
        Name = name;
    }
}
