using Locksmith.Core.Models;

namespace Locksmith.Core.Revocation;

public interface IKeyRevocationProvider<T> where T : KeyDescriptor
{
	bool IsRevoked(T descriptor);
}