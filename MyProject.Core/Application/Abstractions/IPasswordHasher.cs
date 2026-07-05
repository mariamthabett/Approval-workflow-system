namespace MyProject.Core.Application.Abstractions;

/// <summary>Hashes and verifies user passwords. Implementation chooses the algorithm and encoding.</summary>
public interface IPasswordHasher
{
    /// <summary>Produces a self-describing hash string (algorithm parameters + salt + derived key).</summary>
    string Hash(string password);

    /// <summary>Constant-time verification of a plaintext password against a stored hash.</summary>
    bool Verify(string passwordHash, string password);
}
