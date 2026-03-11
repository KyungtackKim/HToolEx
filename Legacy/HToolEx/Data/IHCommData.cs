using HToolEx.Type;
using JetBrains.Annotations;

namespace HToolEx.Data;

/// <summary>
///     Hantas communication data interface
/// </summary>
public interface IHCommData {
    /// <summary>
    ///     Function code
    /// </summary>
    [PublicAPI]
    public CodeTypes CodeTypes { get; }

    /// <summary>
    ///     Data length
    /// </summary>
    [PublicAPI]
    public int Length { get; }

    /// <summary>
    ///     Byte values
    /// </summary>
    [PublicAPI]
    public byte[] Values { get; }

    /// <summary>
    ///     Create message
    /// </summary>
    /// <param name="values">values</param>
    [PublicAPI]
    public void Create(IReadOnlyList<byte> values);
}