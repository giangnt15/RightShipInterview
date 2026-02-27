namespace RightShip.Core.Domain.Entities;

/// <summary>
/// The interface abstract entities with modification information
/// </summary>
public interface IHasModificationInfo
{
    DateTime UpdatedAt { get; set; }
    Guid UpdatedBy { get; set; }
}