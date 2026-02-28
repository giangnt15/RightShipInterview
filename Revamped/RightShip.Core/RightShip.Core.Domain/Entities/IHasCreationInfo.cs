namespace RightShip.Core.Domain.Entities;

/// <summary>
/// The interface abstract entities with creation information
/// </summary>
public interface IHasCreationInfo
{
    DateTime CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
}